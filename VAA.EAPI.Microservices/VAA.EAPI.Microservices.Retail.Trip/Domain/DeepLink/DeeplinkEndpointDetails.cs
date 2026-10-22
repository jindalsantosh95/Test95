using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Trip.Helper;
using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaEncryptedBookingDetails;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.DeepLink
{
    public class DeeplinkEndpointDetails
    {
        public EndpointDetail? Endpoint { get; set; }
        public IEnumerable<Parameter>? Parameters { get; set; }
        public IEnumerable<dynamic?> Items { get; set; } = [];

        public static explicit operator DeeplinkEndpointDetails(DeepLinkRequestPayload request)
        {

            return request.DeeplinkOption switch
            {
                Enum.DeeplinkOption.IROP_ViewDetails or Enum.DeeplinkOption.FlyReady or Enum.DeeplinkOption.FlightChange => new DeeplinkEndpointDetails()
                {
                    Endpoint = new EndpointDetail()
                    {
                        EndpointUrl = ConfigReader.Try(Constant.Environment.ManageMyBookingUrl),
                        Http_Method = HttpMethod.Post.ToString().ToUpper()
                    },
                    Parameters =
                    [
                        new Parameter()
                        {
                            ParamName = "confirmationNo",
                            ParamType = "string",
                            ParamValue = BookingIdHelper.GetPnrLocator(request.TripReservationId!).GetAwaiter().GetResult()
                        },
                        new Parameter()
                        {
                            ParamName = "firstName",
                            ParamType = "string",
                            ParamValue = BookingIdHelper.GetGivenName(request.TripReservationId!).GetAwaiter().GetResult()
                        },
                        new Parameter()
                        {
                            ParamName = "lastName",
                            ParamType = "string",
                            ParamValue = BookingIdHelper.GetSurName(request.TripReservationId!).GetAwaiter().GetResult()
                        },
                         new Parameter()
                        {
                            ParamName = "intersitial",
                            ParamType = "string",
                            ParamValue = "true"
                        }
                    ]
                },
                Enum.DeeplinkOption.IROP_FAF => new DeeplinkEndpointDetails()
                {
                    Endpoint = new EndpointDetail()
                    {
                        EndpointUrl = ConfigReader.Try(Constant.Environment.IROP_FAFURL),
                        Http_Method = HttpMethod.Post.ToString().ToUpper()
                    },
                    Parameters =
                    [
                        new Parameter()
                        {
                            ParamName = "recordLocator",
                            ParamType = "string",
                            ParamValue = BookingIdHelper.GetPnrLocator(request.TripReservationId!).GetAwaiter().GetResult()
                        },
                        new Parameter()
                        {
                            ParamName = "firstName",
                            ParamType = "string",
                            ParamValue = BookingIdHelper.GetGivenName(request.TripReservationId!).GetAwaiter().GetResult()
                        },
                        new Parameter()
                        {
                            ParamName = "lastName",
                            ParamType = "string",
                            ParamValue = BookingIdHelper.GetSurName(request.TripReservationId!).GetAwaiter().GetResult()
                        }
                    ]
                },
                Enum.DeeplinkOption.ManageHoliday => CreateManageHolidayEndpointDetails(request),
                Enum.DeeplinkOption.ConciergeTripAware => new DeeplinkEndpointDetails()
                {
                    Items = [.. (request.TripReservationId ?? throw new ArgumentNullException(nameof(request.TripReservationId))).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                                          .Select((string tripReservationId) =>
                                                          {
                                                              var bookingId = BookingIdHelper.GetBookingId(tripReservationId).GetAwaiter().GetResult();
                                                              var pnrLocator = BookingIdHelper.GetPnrLocator(tripReservationId).GetAwaiter().GetResult();
                                                              var surname = BookingIdHelper.GetSurName(tripReservationId).GetAwaiter().GetResult();

                                                              if (!bookingId.IsNullOrWhiteSpace() && !surname.IsNullOrWhiteSpace())
                                                              {
                                                                  var dob = BookingIdHelper.GetDob(tripReservationId).GetAwaiter().GetResult();

                                                                  if (!dob.IsNullOrWhiteSpace())
                                                                  {
                                                                      return new
                                                                      {
                                                                          type = "holiday",
                                                                          bookingReference = bookingId,
                                                                          surname = surname,
                                                                          dob = dob
                                                                      };
                                                                  }
                                                              }

                                                              if (!pnrLocator.IsNullOrWhiteSpace() && !surname.IsNullOrWhiteSpace())
                                                              {
                                                                  var givenName = BookingIdHelper.GetGivenName(tripReservationId).GetAwaiter().GetResult();

                                                                  if (givenName.IsNullOrWhiteSpace())
                                                                      return default(dynamic);

                                                                  return new
                                                                  {
                                                                      type = "flight",
                                                                      pnrLocator = pnrLocator,
                                                                      givenName = givenName,
                                                                      surname = surname,
                                                                  };
                                                              }

                                                              return default(dynamic);
                                                          })
                                                          .Where(item => item is not null)]
                },
                _ => throw new ArgumentOutOfRangeException(nameof(request.DeeplinkOption), $"Unsupported Deeplink Option: {request.DeeplinkOption}"),
            };
        }

        private static DeeplinkEndpointDetails CreateManageHolidayEndpointDetails(DeepLinkRequestPayload request)
        {
            if (string.IsNullOrWhiteSpace(request.TripReservationId))
            {
                throw new ArgumentNullException(nameof(request.TripReservationId), "TripReservationId is required for ManageHoliday deeplink option.");
            }

            var bookingId = BookingIdHelper.GetBookingId(request.TripReservationId).GetAwaiter().GetResult();
            var surname = BookingIdHelper.GetSurName(request.TripReservationId).GetAwaiter().GetResult();
            var dobValue = BookingIdHelper.GetDob(request.TripReservationId).GetAwaiter().GetResult();

            if (string.IsNullOrWhiteSpace(bookingId) || string.IsNullOrWhiteSpace(surname) || string.IsNullOrWhiteSpace(dobValue) || !DateOnly.TryParse(dobValue, out var dob))
            {
                throw new ArgumentException("TripReservationId must contain msBookingId, surName and dob claims (dob format: yyyy-MM-dd) for ManageHoliday deeplink option.", nameof(request.TripReservationId));
            }
            return new DeeplinkEndpointDetails()
            {
                Endpoint = new EndpointDetail()
                {
                    EndpointUrl = ConfigReader.Try(Constant.Environment.ManageMyHolidayUrl),
                    Http_Method = HttpMethod.Get.ToString().ToUpper()                               // Proxy page expects Get Verb
                },
                Parameters =
                [
                    new Parameter()
                    {
                        ParamName = "bookingReference",
                        ParamType = "string",
                        ParamValue = bookingId
                    },
                    new Parameter()
                    {
                        ParamName = "surname",
                        ParamType = "string",
                        ParamValue = surname
                    },
                    new Parameter()
                    {
                        ParamName = "dateOfBirth",
                        ParamType = "object",
                        ParamValue = $"{{\"day\":{dob.Day},\"month\":{dob.Month},\"year\":{dob.Year}}}"
                    }
                ]
            };
        }

        public static explicit operator DeeplinkEndpointDetails((DeepLinkRequestPayload request, EncryptionResponse encryptedData) source)
        {
            var (request, encryptedData) = source;

            switch (request.DeeplinkOption)
            {
                case Enum.DeeplinkOption.FlightCancel:
                    return new DeeplinkEndpointDetails()
                    {
                        Endpoint = new()
                        {
                            EndpointUrl = ConfigReader.Try(Constant.Environment.FlightCancelUrl),
                            Http_Method = HttpMethod.Post.ToString().ToUpper()
                        },
                        Parameters =
                        [
                            new Parameter()
                            {
                              ParamName = "confirmationCode",
                              ParamType = "string",
                              ParamValue = encryptedData.RecordLocator
                            },
                            new Parameter()
                            {
                              ParamName = "firstName",
                              ParamType = "string",
                              ParamValue = BookingIdHelper.GetGivenName(request.TripReservationId).GetAwaiter().GetResult()
                            },
                            new Parameter()
                            {
                              ParamName = "lastName",
                              ParamType = "string",
                              ParamValue = BookingIdHelper.GetSurName(request.TripReservationId).GetAwaiter().GetResult()
                            }
                        ]
                    };
                default:
                    throw new ArgumentOutOfRangeException(nameof(request.DeeplinkOption), $"Unsupported Deeplink Option: {request.DeeplinkOption}");
            }
        }
    }
}
