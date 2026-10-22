using CSharpFunctionalExtensions;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.NDC.Offer.Application.Query.Delta.SeatAvailability;
using VAA.EAPI.NDC.Offer.Domain.Model.Order;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.SeatAvailabilityRQ;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.SeatAvailabilityRS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.Air4OrderSearchMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.SeatMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ValidationMS;
using static VAA.EAPI.Domain.Virgin.Models.Common.Product;
using DynamicMessagesContent = VAA.EAPI.Domain.Entities.Common.DynamicMessagesContent;
using Segment = VAA.EAPI.Domain.Virgin.Models.Products.Flight.Segment;

namespace VAA.EAPI.NDC.Offer.Validations.SeatAvailability
{
    public class SeatAvailabilityValidations<T> : ISeatAvailabilityValidations<T>
    {
        private readonly IHostEnvironment _hostEnvironment;

        public SeatAvailabilityValidations(IHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }

        public Response<T> ValidateSeatRequest(IATA_SeatAvailabilityRQ seatAvailabilityRQ)
        {
            var errorIds = new List<ErrorDetail>();
            errorIds.AddRange(ValidateSeatAvailability.ValidateSeatAvailabilityRequest(seatAvailabilityRQ));
            errorIds.AddRange(ValidateSeatAvailability.ValidateAgentDetails(seatAvailabilityRQ.DistributionChain));

            return ErrorHelper<T>.GetErrors(errorIds, _hostEnvironment);
        }

        public Response<T> ValidateSeatOrderRequest(IATA_SeatAvailabilityRQ seatAvailabilityRQ)
        {
            var errorIds = new List<ErrorDetail>();
            errorIds.AddRange(ValidateSeatAvailability.ValidateSeatOrderDetails(seatAvailabilityRQ));
            errorIds.AddRange(ValidateSeatAvailability.ValidateAgentDetails(seatAvailabilityRQ.DistributionChain));

            return ErrorHelper<T>.GetErrors(errorIds, _hostEnvironment);
        }

        public Response<T> ValidateAgency(ValidationRS validationRS)
        {
            return ErrorHelper<T>.GetErrors(RequestValidations.ValidateAgencyDetails(validationRS).ToList(), _hostEnvironment);
        }

        public Response<T> ValidateSeatAvailabilityRequest(IATA_SeatAvailabilityRQ seatAvailabilityRQ, OfferResponse offerdoc, string offerId)
        {
            return ErrorHelper<T>.GetErrors(ValidateSeatAvailability.ValidateSeatOfferDetailsInOfferDoc(seatAvailabilityRQ, offerdoc, offerId), _hostEnvironment);
        }

        public Response<T> ValidateSeatAvailabilitySeatOrderRequest(ValidationRS validationMSResponse, OrderDocument orderDocument)
        {
            var errorIds = new List<ErrorDetail>();
            errorIds.AddRange(ValidateSeatAvailability.ValidateOrderDetails(orderDocument));
            // errorIds.AddRange(RequestValidations.ValidateAgentID(validationMSResponse.ValidateResponse, orderDocument));

            return ErrorHelper<T>.GetErrors(errorIds, _hostEnvironment);
        }

        public Response<T> ValidateSegmentSeats(IEnumerable<FlightSegment> segmentList)
        {
            return ErrorHelper<T>.GetErrors(ValidateSeatAvailability.ValidateEligibleSegment(segmentList), _hostEnvironment);
        }

        public Response<T> ValidateHoldBookingPNR(OrderDocument sabreOrderDoc)
        {
            return ErrorHelper<T>.GetErrors(ValidateSeatAvailability.ValidateOrderDetails(sabreOrderDoc), _hostEnvironment);
        }
        public Response<T> ValidateSeatServiceRequest(IATA_SeatAvailabilityRQ seatAvailabilityRQ)
        {
            List<ErrorDetail> errorIds = new();

            errorIds.AddRange(ValidateSeatAvailability.ValidateSeatAvailabilityRequest(seatAvailabilityRQ));
            errorIds.AddRange(ValidateSeatAvailability.ValidateAgentDetails(seatAvailabilityRQ.DistributionChain));
            errorIds.AddRange(ValidateSeatAvailability.ValidateSeatAvailabilityOfferID(seatAvailabilityRQ));

            return ErrorHelper<T>.GetErrors(errorIds, _hostEnvironment);
        }

        public Response<T> ValidateAgencyInPostSale(ValidationRS validationRS, OrderDocument orderDocument)
        {
            var errorIds = new List<ErrorDetail>();
            errorIds.AddRange(RequestValidations.ValidateAgencyDetails(validationRS));
            errorIds.AddRange(RequestValidations.ValidateAgentID(validationRS.ValidateResponse, orderDocument));

            return ErrorHelper<T>.GetErrors(errorIds, _hostEnvironment);
        }

        public Response<T> ValidateSeatInPNR()
        {
            var errorIds = new List<ErrorDetail>();
            errorIds.AddRange(RequestValidations.ValidateSeatsInPNR());

            return ErrorHelper<T>.GetErrors(errorIds, _hostEnvironment);
        }
        public IATA_SeatAvailabilityRS MapToErrorSeatAvailabilityRs(ResponseMessage errors)
        {
            return new IATA_SeatAvailabilityRS { Items = errors.Errors.Select(error => (Domain.Model.VAA.ErrorType)error).ToArray() };
        }

        public IEnumerable<NDCWarning> PopulateNonVSSegmentWarnings(OfferResponse offerDoc)
        {
            var flights = offerDoc.DataList.FlightSegments.Where(x => (x.OperatingCarrier != null) && (x.OperatingCarrier != "DL"));
            return flights.Select(x => new NDCWarning
            {
                Code = "200_1",
                Type = "Warning",
                IsDynamicMessage = true,
                DynamicMessagesContents = new DynamicMessagesContent[]
                {
                    new()
                    {
                        Fieldname = Constants.Application.FlightNumberField,
                        ReplaceWith = x.OperatingCarrier+x.OperatingFlightNumber
                    }
                }
            });
        }

        public IEnumerable<NDCWarning> PopulateFOPWarning()
        {
            return [new NDCWarning
            {
                Code = "466_2",
                Type = "Warning",
                IsDynamicMessage = false
            }];
        }

        public IEnumerable<NDCWarning> PopulateVSSegmentWarnings(OrderDocument orderDocument)
        {
            var paxList = SeatAvailabilityQueryHandler.GetPaxListFromOrderDoc(orderDocument);

            IEnumerable<OrderService> serviceList = orderDocument?.OrderItems?.SelectMany(oi => oi.Services).ToList();
            IEnumerable<Segment> segments = orderDocument?.DataList.SegmentDetails;
            IEnumerable<Segment> segmentList = Enumerable.Empty<Segment>();
            IEnumerable<NDCWarning> warningList = Enumerable.Empty<NDCWarning>();

            segmentList = serviceList.Select(service =>
            {
                var segment = segments.First(segment => segment.SegmentId == service?.SegmentRefIds.FirstOrDefault());

                return new Segment()
                {
                    MarketingCarrier = segment.MarketingCarrier,
                    MarketingFlightNumber = segment.MarketingFlightNumber,
                    OperatingCarrier = segment.OperatingCarrier,
                    OperatingFlightNumber = segment.OperatingFlightNumber,
                    OriginAirportCode = segment.OriginAirportCode,
                    DestinationAirportCode = segment.DestinationAirportCode,
                    ClassOfService = segment.ClassOfService,
                    ScheduledArrivalTime = new()
                    {
                        LocalDateTime = DateTime.Parse(segment.ScheduledArrivalTime?.LocalDateTime, CultureInfo.InvariantCulture).ToString("yyyy-MM-dd")
                    },
                    ScheduledDepartureTime = new()
                    {
                        LocalDateTime = DateTime.Parse(segment.ScheduledDepartureTime?.LocalDateTime, CultureInfo.InvariantCulture).ToString("yyyy-MM-dd")
                    },
                    SegmentNumber = segment.SegmentId,
                    SegmentId = segment.SegmentId,
                    FlightDuration = segment.FlightDuration
                };
            });

            foreach (var seg in segmentList.Where(x => (x.OperatingCarrier == null && x.MarketingCarrier == Constants.Application.OwnerCode) && (x.OperatingCarrier == "DL")))
            {
                warningList = orderDocument?.OrderItems?.SelectMany(oi => oi.Services).ToList().Where(s => s.SegmentRefIds.FirstOrDefault() == seg.SegmentId).Select(x => new NDCWarning
                {
                    Code = "200_1",
                    Type = "Warning",
                    IsDynamicMessage = false,

                });

            };

            return warningList;
        }

        public IEnumerable<NDCWarning> PopulateNonVSSegmentWarnings(OrderDocument orderDocument)
        {
            return orderDocument?.DataList.SegmentDetails.Where(seg => (seg?.OperatingCarrier != null) && (seg.OperatingCarrier != "DL")).Select(x => new NDCWarning
            {
                Code = "200_1",
                Type = "Warning",
                IsDynamicMessage = true,
                DynamicMessagesContents = new DynamicMessagesContent[]
                {
                    new()
                    {
                        Fieldname = Constants.Application.FlightNumberField,
                        ReplaceWith=x.OperatingCarrier+x.OperatingFlightNumber
                    }
                }

            });

        }

        public IEnumerable<NDCWarning> PopulateWarningsForPaxWithSeat(OrderDocument orderDocument)
        {
            var segmentList = orderDocument.DataList.SegmentDetails;

            var seatOrderItems = orderDocument.OrderItems.Where(oi => oi.Services.FirstOrDefault()?.Product?.ProductType == ProductTypes.SeatEnum);

            var warnings = new List<NDCWarning>();

            foreach (var seg in segmentList)
            {
                if (seg.OperatingCarrier is null || seg.OperatingCarrier == "DL")
                {
                    var paxSeatCount = seatOrderItems.Count(sOI => sOI.Services.FirstOrDefault()?.SegmentRefIds?.FirstOrDefault() == seg.SegmentId);

                    if (paxSeatCount > 0)
                    {
                        warnings.Add(new NDCWarning
                        {
                            Code = "207_1",
                            Type = "Warning",
                            IsDynamicMessage = true,
                            DynamicMessagesContents = new[]
                            {
                                new DynamicMessagesContent
                                {
                                  Fieldname = Constants.Application.FlightNumberField,
                                  ReplaceWith = seg.OperatingCarrier + seg.OperatingFlightNumber
                                }
                            }
                        });
                    }
                }
            }

            return warnings;
        }

        public IEnumerable<NDCWarning> PopulateSeatMapResponseWarningsPostSale(SeatRS seatMSResponse)
        {
            if (seatMSResponse?.SeatAvailabilityResponse?.FlightSegmentSeatMap is null) return Enumerable.Empty<NDCWarning>();

            var carrierCode = seatMSResponse?.SeatAvailabilityResponse?.FlightSegmentSeatMap.First().FlightSegment.OperatingCarrier == null ? seatMSResponse?.SeatAvailabilityResponse?.FlightSegmentSeatMap.First().FlightSegment.MarketingCarrier : seatMSResponse?.SeatAvailabilityResponse?.FlightSegmentSeatMap.First().FlightSegment.OperatingCarrier;

            return seatMSResponse?.SeatAvailabilityResponse?.FlightSegmentSeatMap.Where(fssm => !fssm.Catalogue.Any()).Select(x => new NDCWarning
            {
                Code = "200_1",
                Type = "Warning",
                IsDynamicMessage = true,
                DynamicMessagesContents = new DynamicMessagesContent[]
                    {
                        new()
                        {
                            Fieldname = Constants.Application.FlightNumberField,
                            ReplaceWith =   carrierCode+seatMSResponse?.SeatAvailabilityResponse?.FlightSegmentSeatMap.First().FlightSegment.MarketingFlightNumber
                        }
                    }

            });
        }

        public IEnumerable<NDCWarning> PopulateSeatMapResponseWarnings(SeatRS seatMSResponse)
        {
            if (seatMSResponse?.SeatAvailabilityResponse?.FlightSegmentSeatMap is null) return Enumerable.Empty<NDCWarning>();

            var availableFlag = seatMSResponse?.SeatAvailabilityResponse?.FlightSegmentSeatMap.Any(x => x.Seatmap.Any(sr => sr.SeatRows.Any(c => c.SeatColumns.Any(c => c.Available == true && c.Seat == true && c.Blocked == false))));

            if (availableFlag.Value == true) { return Enumerable.Empty<NDCWarning>(); }
            else
            {
                return seatMSResponse?.SeatAvailabilityResponse?.FlightSegmentSeatMap.Select(x => x.Seatmap.Select(sr => sr.SeatRows.Select(c => c.SeatColumns.Where(c => c.Available == true && c.Seat == true && c.Blocked == false)))).Select(x => new NDCWarning
                {
                    Code = "200_1",
                    Type = "Warning",
                    IsDynamicMessage = true,
                    DynamicMessagesContents = new DynamicMessagesContent[]
                    {
                        new()
                        {
                            Fieldname = Constants.Application.FlightNumberField,
                            ReplaceWith =   seatMSResponse?.SeatAvailabilityResponse?.FlightSegmentSeatMap.First().FlightSegment.MarketingCarrier+seatMSResponse?.SeatAvailabilityResponse?.FlightSegmentSeatMap.First().FlightSegment.MarketingFlightNumber
                        }
                    }
                });

            }

        }

        public Response<T> ValidateSegmentSeatsWithOrderId(IEnumerable<EAPI.Domain.Virgin.Models.Products.Flight.Segment> segmentList)
        {
            return ErrorHelper<T>.GetErrors(ValidateSeatAvailability.ValidateEligibleSegmentSeatsWithOrderId(segmentList), _hostEnvironment);
        }

        public Response<T> ValidateEligibleFlights(IEnumerable<FlightSegment> segmentList)
        {
            return ErrorHelper<T>.GetErrors(ValidateSeatAvailability.ValidateEligibleSegment(segmentList), _hostEnvironment);
        }

        public Response<T> ValidateEligibleVssFlights(IEnumerable<Segment> segmentList)
        {
            return ErrorHelper<T>.GetErrors(ValidateSeatAvailability.ValidateEligibleVssSegment(segmentList), _hostEnvironment);
        }

        public Response<T> ValidateAlacarteOfferInResponse(IATA_SeatAvailabilityRS seatResponse)
        {
            return ErrorHelper<T>.GetErrors(ValidateSeatAvailability.ValidateAlacarteOfferInResponse(seatResponse), _hostEnvironment);
        }

        public Response<T> ValidateGDSSegmentSeats(IEnumerable<EAPI.Domain.Virgin.Models.Products.Flight.Segment> segmentList)
        {
            return ErrorHelper<T>.GetErrors(ValidateSeatAvailability.ValidateGDSEligibleSegment(segmentList), _hostEnvironment);
        }

        public Response<T> ValidateOrderSearchRSForGdsAgentBooking(Air4OrderSearchMSResponse air4OrderSearchRS, string orgID)
        {
            var errorIds = new List<ErrorDetail>();

            errorIds.AddRange(ValidateSeatAvailability.ValidateOrderSearchNullResponse(air4OrderSearchRS));

            if ((errorIds?.Any() ?? false) && ConfigReader.Get<bool>(Constants.Application.SkipGdsAgentValidation))
                errorIds.AddRange(ValidateSeatAvailability.ValidateGdsAgentBooking(air4OrderSearchRS, orgID));

            if (errorIds?.Any() ?? false)
                errorIds.AddRange(ValidateSeatAvailability.ValidateGDSFlownSegments(air4OrderSearchRS));

            return ErrorHelper<T>.GetErrors(errorIds, _hostEnvironment);
        }

        public IEnumerable<NDCWarning> PopulateGDSNonVSSegmentWarnings(OrderSearchResponse orderSearchResponse, IHostEnvironment _hostEnvironment)
        {
            return orderSearchResponse?.SegmentDetails.Select(s =>
            {
                if (!(s.MarketingCarrier.ToUpper() == "VS" && string.IsNullOrEmpty(s.OperatingCarrier)) && (s.OperatingCarrier != "DL"))
                {
                    var warning = new NDCWarning
                    {
                        Code = "200_1",
                        Type = "Warning",
                        IsDynamicMessage = true,
                        DynamicMessagesContents = new DynamicMessagesContent[]
                         {
                                new DynamicMessagesContent()
                                {
                                    Fieldname = Constants.Application.FlightNumberField,
                                    ReplaceWith = s.OperatingCarrier+s.OperatingFlightNumber
                                }
                         }
                    };

                    return ErrorHelper<NDCWarning>.GetWarnings(new List<NDCWarning>() { warning }, _hostEnvironment).FirstOrDefault();
                }
                else
                    return null;
            });
        }

        public IEnumerable<NDCWarning> PopulateGDSVSSegmentWarnings(OrderSearchResponse orderSearchResponse, IHostEnvironment _hostEnvironment)
        {
            var segmentList = orderSearchResponse.SegmentDetails;

            //check if vs segment has seat assigned for some pax
            return segmentList.Select(seg =>
            {
                if ((seg.OperatingCarrier == null) || (seg.OperatingCarrier == "DL"))
                {
                    var seatOrderItems = orderSearchResponse.OrderItems.Where(oi => oi.Services.First().ProductOrder.ProductType == ProductTypes.SeatEnum);
                    var paxSeatCount = seatOrderItems.Count(sOI => sOI.Services.First().ProductSegmentAssociation.FlightSegmentIds.First() == seg.SegmentId);

                    if (paxSeatCount > 0)
                    {
                        var warning = new NDCWarning
                        {
                            Code = "207_1",
                            Type = "Warning",
                            IsDynamicMessage = false
                        };

                        return ErrorHelper<NDCWarning>.GetWarnings(new List<NDCWarning>() { warning }, _hostEnvironment).FirstOrDefault();
                    }
                    else
                        return null;
                }
                else
                    return null;
            }).Where(x => x != null).Distinct();
        }

        public IEnumerable<NDCWarning> PopulateGDSSeatMapResponseWarnings(SeatRS seatMSResponse, IHostEnvironment _hostEnvironment)
        {
            return seatMSResponse?.SeatAvailabilityResponse?.FlightSegmentSeatMap.Where(x => x?.Catalogue.Count() == 0).Select(x =>
            {
                var warning = new NDCWarning
                {
                    Code = "200_1",
                    Type = "Warning",
                    IsDynamicMessage = true,
                    DynamicMessagesContents = new DynamicMessagesContent[]
                    {
                        new()
                        {
                            Fieldname = Constants.Application.FlightNumberField,
                            ReplaceWith = x.FlightSegment.MarketingCarrier+x.FlightSegment.MarketingFlightNumber
                        }
                    }
                };

                return ErrorHelper<NDCWarning>.GetWarnings(new List<NDCWarning>() { warning }, _hostEnvironment).FirstOrDefault();
            });
        }
    }
}