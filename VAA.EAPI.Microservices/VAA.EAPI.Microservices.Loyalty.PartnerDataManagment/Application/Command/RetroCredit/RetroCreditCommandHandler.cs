using MediatR;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using System.Globalization;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.RetroCredit;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.DataModels;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.FlightDetails;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.FlownDB.Interface;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Validations;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Command.RetroCredit
{
    public class RetroCreditCommandHandler : IRequestHandler<RetroCreditCommand, Response<RetroCreditResponse>>
    {

        private readonly IFlownDbRepository _flownDBRepository;

        private readonly IRetroApiService _apiServices;

        private readonly ILogger<RetroCreditCommandHandler> _logger;

        public RetroCreditCommandHandler(IFlownDbRepository flownDBRepository, IRetroApiService retroApiService, ILogger<RetroCreditCommandHandler> logger)
        {
            _flownDBRepository = flownDBRepository ?? throw new ArgumentNullException(nameof(flownDBRepository));
            _apiServices = retroApiService ?? throw new ArgumentNullException(nameof(retroApiService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Response<RetroCreditResponse>> Handle(RetroCreditCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var retrocreditRequestValidation = new RetroCreditRequestValidator().Validate(request.RetroCreditRequest);

                if (retrocreditRequestValidation.IsValid)
                {
                    var response = await CheckFlightLevelValidationForRetr(request);

                    if (!response.isvalid)
                    {
                        return response.edcResponse;
                    }

                    return await FlownDetailsValidationForRetro(request);
                }
                else
                {
                    return new Response<RetroCreditResponse>
                    {
                        Status = Status.BadRequest,
                        Value = null
                    };
                }
            }
            catch (ApiFailedException ex)
            {
                _logger.LogError(ex, "RetroCreditCommandHandler:ApiFailedException");

                return new Response<RetroCreditResponse>
                {
                    Status = Status.Error,
                    Errors = new List<ErrorDetails>
                    {
                        new ErrorDetails
                        {
                             Message = Constants.ErrorCodes.Tech_Error,
                             ErrorSource = ex.Message
                        }
                    }                    
                };
            }
            catch(SqlException ex)
            {
                _logger.LogError(ex, "RetroCreditCommandHandler:SqlException");

                return new Response<RetroCreditResponse>
                {
                    Status = Status.Error,
                    Errors = new List<ErrorDetails>
                    {
                        new ErrorDetails
                        {
                             Message = Constants.ErrorCodes.Tech_Error,
                             ErrorSource = ex.Message,
                             Code = ex.Number.ToString()
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RetroCreditCommandHandler:Exception");

                return new Response<RetroCreditResponse>
                {
                    Status = Status.Error,
                    Errors = new List<ErrorDetails>
                    {
                        new ErrorDetails
                        {
                             Message = Constants.ErrorCodes.Tech_Error,
                             ErrorSource = ex.Message,
                        }
                    }
                };
            }
        }

        private async Task<(bool isvalid, Response<RetroCreditResponse> edcResponse)> CheckFlightLevelValidationForRetr(RetroCreditCommand request)
        {

            DateTime activityDate = DateTime.ParseExact(request.RetroCreditRequest.ScheduledFlightSegmentDepartureDate, "yyyyMMdd", CultureInfo.InvariantCulture);

            var maxDuration = ConfigReader.Get(Constants.Environment.RetroDurationinDays);

            if (activityDate.AddDays(7) > DateTime.Now || activityDate.AddDays(Convert.ToInt16(maxDuration)) < DateTime.Now)
            {
                return new(false, new Response<RetroCreditResponse>
                {
                    Status = Status.OK,
                    Value = new RetroCreditResponse
                    {
                        EligibilityDenialCode = Constants.ErrorCodes.EDC101,
                        TrackID = request.RetroCreditRequest.TrackID,
                        OperatingCarrierCode = request.RetroCreditRequest.OperatingCarrierCode,
                        OperatingFlightNumber = request.RetroCreditRequest.OperatingFlightNumber,
                    }
                });
            }

            FlightDetails flightDetails = await _apiServices.GetCodeShareFlightDetails(request.RetroCreditRequest.OperatingFlightNumber, activityDate.ToString("yyyy-MM-dd"));


            if (flightDetails == null)
            {

                flightDetails = await _apiServices.GetCodeShareFlightDetails(request.RetroCreditRequest.OperatingFlightNumber, activityDate.AddDays(1).ToString("yyyy-MM-dd"));


                if (flightDetails == null)
                {

                    flightDetails = await _apiServices.GetCodeShareFlightDetails(request.RetroCreditRequest.OperatingFlightNumber, activityDate.AddDays(-1).ToString("yyyy-MM-dd"));

                }
            }


            if (flightDetails == null || flightDetails.Flights == null || flightDetails.Flights?.Count() == 0)
            {
                //EDC102 - Flight is not valid
                return new(false, new Response<RetroCreditResponse>
                {
                    Status = Status.OK,
                    Value = new RetroCreditResponse
                    {
                        EligibilityDenialCode = Constants.ErrorCodes.EDC102,
                        TrackID = request.RetroCreditRequest.TrackID,
                        OperatingCarrierCode = request.RetroCreditRequest.OperatingCarrierCode,
                        OperatingFlightNumber = request.RetroCreditRequest.OperatingFlightNumber,
                    }
                });
            }
            else
            {
                var flight = flightDetails.Flights.FirstOrDefault();

                if (flight != null && !string.IsNullOrWhiteSpace(flight.OperatingFlightNumber.FlightNumber) && !string.IsNullOrWhiteSpace(flight.OperatingFlightNumber.AirlineCode) && !flight.OperatingFlightNumber.AirlineCode.Equals("VS"))
                {
                    //EDC888 - Flight is a VS marketed flight
                    return new(false, new Response<RetroCreditResponse>
                    {
                        Status = Status.OK,
                        Value = new RetroCreditResponse
                        {
                            EligibilityDenialCode = Constants.ErrorCodes.EDC888,
                            TrackID = request.RetroCreditRequest.TrackID,
                            MarketingCarrierCode = request.RetroCreditRequest.OperatingCarrierCode,
                            MarketingFlightNumber = request.RetroCreditRequest.OperatingFlightNumber,
                            OperatingCarrierCode = flight.OperatingFlightNumber.AirlineCode,
                            OperatingFlightNumber = flight.OperatingFlightNumber.FlightNumber,
                        }
                    });
                }

                if (flight != null && !flight.Legs.Any(l => l.OriginAirportCode.Equals(request.RetroCreditRequest.FlightSegmentOrigin) && l.DestinationAirportCode.Equals(request.RetroCreditRequest.FlightSegmentDestination)))
                {
                    //EDC102 - City pair not valid for the given flight
                    return new(false, new Response<RetroCreditResponse>
                    {
                        Status = Status.OK,
                        Value = new RetroCreditResponse
                        {
                            EligibilityDenialCode = Constants.ErrorCodes.EDC103,
                            TrackID = request.RetroCreditRequest.TrackID,
                            OperatingCarrierCode = request.RetroCreditRequest.OperatingCarrierCode,
                            OperatingFlightNumber = request.RetroCreditRequest.OperatingFlightNumber,
                        }
                    });
                }
                return new(true, null);
            }
        }

        private async Task<Response<RetroCreditResponse>> FlownDetailsValidationForRetro(RetroCreditCommand request)
        {
            var objRetroCreditFlownDetails = await _flownDBRepository.GetFlownDetailsForRetroCreditAsync(request.RetroCreditRequest);
            
            var nonAccrualClasses = ConfigReader.Get(Constants.Environment.NonAccrualVSClasses);

            var nonAccClassList = nonAccrualClasses.Split(',');

            if (objRetroCreditFlownDetails.FdNoShow == "Y" || nonAccClassList.Any(c => c.Equals(objRetroCreditFlownDetails.OperatingCarierBookingClass)))
            {
                return new Response<RetroCreditResponse>
                {
                    Status = Status.OK,
                    Value = new RetroCreditResponse
                    {
                        EligibilityDenialCode = Constants.ErrorCodes.EDC106,
                        TrackID = request.RetroCreditRequest.TrackID,
                        OperatingCarrierCode = request.RetroCreditRequest.OperatingCarrierCode,
                        OperatingFlightNumber = request.RetroCreditRequest.OperatingFlightNumber,
                    }
                };
            }

            if (objRetroCreditFlownDetails == null || !string.IsNullOrWhiteSpace(objRetroCreditFlownDetails.Error))
            {
                var errorEDC = objRetroCreditFlownDetails != null && objRetroCreditFlownDetails.Error.Equals("ERROR CODE 666") ? Constants.ErrorCodes.EDC666 : Constants.ErrorCodes.EDC104;

                return new Response<RetroCreditResponse>
                {
                    Status = Status.OK,
                    Value = new RetroCreditResponse
                    {
                        EligibilityDenialCode = errorEDC,
                        TrackID = request.RetroCreditRequest.TrackID,
                        OperatingCarrierCode = request.RetroCreditRequest.OperatingCarrierCode,
                        OperatingFlightNumber = request.RetroCreditRequest.OperatingFlightNumber,
                    }
                };
            }
            else
            {
                var retroStatus = objRetroCreditFlownDetails.ProcessingStatus != null ? objRetroCreditFlownDetails.ProcessingStatus.ToLower() : string.Empty;

                switch (retroStatus)
                {
                    case "allocated":

                        return new Response<RetroCreditResponse>
                        {
                            Status = Status.OK,
                            Value = new RetroCreditResponse
                            {
                                EligibilityDenialCode = Constants.ErrorCodes.EDC201,
                                TrackID = request.RetroCreditRequest.TrackID,
                                FFPCarrierCode = objRetroCreditFlownDetails.FFPCarrierCode,
                                FFPAccountNumber = objRetroCreditFlownDetails.FFPAccountNumber,
                                OperatingCarrierCode = request.RetroCreditRequest.OperatingCarrierCode,
                                OperatingFlightNumber = request.RetroCreditRequest.OperatingFlightNumber,
                                PostingDate = objRetroCreditFlownDetails.PostingDate,
                                PostingNonEligibilityIndicator = "Y"
                            }
                        };
                    case "pending":

                        var pendingEDC = objRetroCreditFlownDetails.FFPCarrierCode == request.RetroCreditRequest.FFPCarrierCode && objRetroCreditFlownDetails.FFPAccountNumber == request.RetroCreditRequest.FFPAccountNumber ? Constants.ErrorCodes.EDC001 : Constants.ErrorCodes.EDC201;

                        if (pendingEDC.Equals(Constants.ErrorCodes.EDC201))
                        {
                            return new Response<RetroCreditResponse>
                            {
                                Status = Status.OK,
                                Value = new RetroCreditResponse
                                {
                                    EligibilityDenialCode = pendingEDC,
                                    TrackID = request.RetroCreditRequest.TrackID,
                                    FFPCarrierCode = objRetroCreditFlownDetails.FFPCarrierCode,
                                    FFPAccountNumber = objRetroCreditFlownDetails.FFPAccountNumber,
                                    OperatingCarrierCode = request.RetroCreditRequest.OperatingCarrierCode,
                                    OperatingFlightNumber = request.RetroCreditRequest.OperatingFlightNumber,                                    
                                    PostingNonEligibilityIndicator = "R"
                                }
                            };
                        }
                        else
                        {
                            return await CreateEligibleRetroResponse(Constants.ErrorCodes.EDC001, request, objRetroCreditFlownDetails);
                        }
                    case "free":
                    default:
                        return await CreateEligibleRetroResponse(Constants.ErrorCodes.EDC000, request, objRetroCreditFlownDetails);
                }
            }
        }

        private async Task<Response<RetroCreditResponse>> CreateEligibleRetroResponse(string eligibleEdc, RetroCreditCommand request, RetroCreditFlownDetails objRetroCreditFlownDetails)
        {
            var dbResp = await _flownDBRepository.UpdateFlownDetailsForEligibleRetroAsync(request.RetroCreditRequest, objRetroCreditFlownDetails.OperatingAirAuthNumber);

            return new Response<RetroCreditResponse>
            {
                Status = Status.OK,
                Value = new RetroCreditResponse
                {
                    TrackID = request.RetroCreditRequest.TrackID,
                    EligibilityDenialCode = eligibleEdc,
                    UniqueIdentificationNumber = request.RetroCreditRequest.OperatingCarrierCode + objRetroCreditFlownDetails.OperatingAirAuthNumber,
                    FFPCarrierCode = request.RetroCreditRequest.FFPCarrierCode,
                    FFPAccountNumber = request.RetroCreditRequest.FFPAccountNumber,
                    PassengerFirstName = objRetroCreditFlownDetails.FirstName,
                    PassengerLastName = objRetroCreditFlownDetails.LastName,
                    MarketingCarrierCode = objRetroCreditFlownDetails.MarketingAirlineCode,
                    MarketingFlightNumber = objRetroCreditFlownDetails.MarketingFlightNum,
                    OperatingCarrierCode = objRetroCreditFlownDetails.OperatingAirlineCode,
                    OperatingFlightNumber = objRetroCreditFlownDetails.OperatingFlightNum,
                    ScheduledFlightSegmentDepartureDate = objRetroCreditFlownDetails.DepartureDate,
                    CabinClassCode = objRetroCreditFlownDetails.FlownCabinClass,
                    MarketingCarrierBookingClass = objRetroCreditFlownDetails.MarketingCarrierBookingClass,
                    OperatingCarierBookingClass = objRetroCreditFlownDetails.OperatingCarierBookingClass,
                    FlightSegmentOrigin = objRetroCreditFlownDetails.OrginPort,
                    FlightSegmentDestination = objRetroCreditFlownDetails.DestinationPort,
                    EticketNumber = objRetroCreditFlownDetails.TicketNumber,
                    EticketCouponNumber = objRetroCreditFlownDetails.CoupanNumber
                }
            };
        }
    }
}