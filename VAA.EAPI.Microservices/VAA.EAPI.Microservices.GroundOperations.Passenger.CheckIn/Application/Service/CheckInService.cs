using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Application.Interface;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Constants;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.DeltaSsci;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.BoardingPass;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.EmergencyContact;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.ReturnDate;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Helper;
using BoardingPassResponse = VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.BoardingPass.BoardingPassResponse;
using Status = VAA.EAPI.Domain.Entities.Common.Status;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Application.Service
{
    public class CheckInService : ICheckInService
    {
        private readonly IHttpConnector _httpConnector;
        private readonly IDeltaSsciService _deltaSsciService;
        private readonly IDeltaTrmService _deltaTrmService;
        private readonly ILogger<CheckInService> _logger;
        private readonly IDistributedCacheService _distributedCacheService;


        public CheckInService(IHttpConnector httpConnector, IDeltaSsciService deltaSsciService, IDeltaTrmService deltaTrmService, ILogger<CheckInService> logger, IDistributedCacheService distributedCacheService)
        {
            _httpConnector = httpConnector;
            _deltaSsciService = deltaSsciService;
            _deltaTrmService = deltaTrmService;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _distributedCacheService = distributedCacheService;
        }

        public async Task<bool> LogTransactionSummaryAsync(TransactionSummaryRequest request)
        {
            if (request is null ||
                request.TravelPartyId.IsNullOrWhiteSpace() ||
                request.RecordLocator.IsNullOrWhiteSpace() ||
                request.OriginAirportCode.IsNullOrWhiteSpace() ||
                request.TransactionError.IsNullOrWhiteSpace())
            {
                return false;
            }

            var isLogged = await _deltaSsciService.PostBuildTransactionSummaryAsync(request.TravelPartyId, new BuildTransactionSummaryRequest
            {
                RecordLocator = request.RecordLocator,
                OriginAirportCode = request.OriginAirportCode,
                TransactionError = request.TransactionError
            });

            if (!isLogged)
            {
                _logger.LogError("Check-in transaction summary was not recorded for {TravelPartyId}.", request.TravelPartyId);
            }

            return isLogged;
        }

        public async Task<Result<Response<CheckInResponse>>> InitiateCheckInAsync(CheckInRequest checkInRequest)
        {
            CheckInResponse responseObj;

            try
            {
                var ssciObjTask = _deltaSsciService.PostTravelPartiesAsync((DeltaSsci.InputObject)checkInRequest);

                var trmObjTask = _deltaTrmService.PostRequirementsAsync((DeltaTrm.RequirementsRequest)checkInRequest);

                responseObj = (CheckInResponse)(await ssciObjTask, await trmObjTask, false);
            }

            catch (HttpRequestException e)
            {
                _logger.LogError("Check-in initiation failed for {PnrLocator}.", checkInRequest?.PnrLocator);
                responseObj = (CheckInResponse)e;
            }

            return Result.Success(new Response<CheckInResponse> { Status = Status.OK, Value = responseObj });
        }

        public async Task<Result<Response<CheckInResponse>>> GetCheckInAsync(string travelPartyId)
        {
            var ssciTask = _deltaSsciService.GetTravelPartiesAsync(travelPartyId!);
            var ssciObj = await ssciTask;

            //POST call to TRM as GET call doesn't return all the attributes 
            var trmObj = await _deltaTrmService.PostRequirementsAsync(new DeltaTrm.RequirementsRequest
            {
                Using = DeltaTrm.RequestOptionCodeType.CONFIRMATION,
                ConfirmationNum = ssciObj.RecordLocator,
                GivenNames = ssciObj.Passengers.First().FirstName,
                Surname = ssciObj.Passengers.First().LastName
            });

            var responseObj = (CheckInResponse)(ssciObj, trmObj, false);

            return Result.Success(new Response<CheckInResponse> { Status = Status.OK, Value = responseObj });
        }

        public async Task<Result<Response<CheckInResponse>>> SelectPassengersAsync(Domain.SelectPassengerRequest selectPassengerRequest)
        {
            var travelPartyId = selectPassengerRequest?.TravelPartyId;

            if (travelPartyId.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(travelPartyId));

            var trmTask = _deltaTrmService.PostRequirementsAsync(new DeltaTrm.RequirementsRequest
            {
                Using = DeltaTrm.RequestOptionCodeType.CONFIRMATION,
                ConfirmationNum = selectPassengerRequest?.PnrLocator,
                GivenNames = selectPassengerRequest?.FirstName,
                Surname = selectPassengerRequest?.LastName
            });

            var ssciTask = _deltaSsciService.PatchTravelPartiesAsync(travelPartyId!, (DeltaSsci.SelectPassengerRequest)selectPassengerRequest);

            var ssciObj = await ssciTask;

            TravelParty? ssciObjUpdated = null;

            if (ssciObj.Passengers?.Count != selectPassengerRequest.SelectedPassengerIds.Count())
            {
                TravelParty ssciTraPartyResponse = await _deltaSsciService.PostTravelPartiesAsync((InputObject)new CheckInRequest
                {
                    PnrLocator = selectPassengerRequest?.PnrLocator,
                    OriginCode = selectPassengerRequest?.OriginCode
                });

                var ssciTaskUpdated = _deltaSsciService.PatchTravelPartiesAsync(ssciTraPartyResponse?.Id!, (DeltaSsci.SelectPassengerRequest)selectPassengerRequest);

                ssciObjUpdated = await ssciTaskUpdated;
            }

            var trmObj = await trmTask;

            var responseObj = (CheckInResponse)(ssciObjUpdated ?? ssciObj, trmObj, false);


            if (responseObj.OnlineCheckInStatus.Available && responseObj.Passengers.Count() != selectPassengerRequest.SelectedPassengerIds.Count())
            {
                var selectedCustomerIds = responseObj?.Passengers.Select(pId => new PassengerId(pId.PassengerId).CustomerId);
                var requestCustomerIds = selectPassengerRequest?.SelectedPassengerIds.Select(pId => new PassengerId(pId).CustomerId);

                var unSelectedPassengers = requestCustomerIds.Where(p2 => !selectedCustomerIds.Any(p1 => p1 == p2)).ToList();

                _logger.LogInformation("Unable to select {UnselectedCount} passenger(s) for PNR {PnrLocator}.", unSelectedPassengers.Count, selectPassengerRequest?.PnrLocator);

                var errorDetails = CheckInMessageHelper.GetStatusMessage("PassengerSelectionError");

                responseObj.OnlineCheckInStatus = new OnlineCheckInStatus
                {
                    Code = "PassengerSelectionError",
                    Title = errorDetails.Item1,
                    Message = errorDetails.Item2
                };
            }

            return Result.Success(new Response<CheckInResponse> { Status = Status.OK, Value = responseObj });
        }

        public async Task<Result<Response<CheckInResponse>>> SavePassportAsync(SavePassportRequest savePassportRequest)
        {
            CheckInResponse responseObj;

            try
            {
                // Save passport to TRM. This will record it as verified if it has been scanned.
                var deltaTrmObj = await _deltaTrmService.PatchSingleRequirementsAsync(savePassportRequest.RequirementsId!, (DeltaTrm.RequirementFormRequest)savePassportRequest);

                // Resend the same passport details to SSCI. This must be sent after the TRM call is complete so that SSCI
                // will identify the passport as "reused". Otherwise, it will be recorded as unverified because SSCI doesn't
                // support saving verified passports.
                var ssciPostPassport = await _deltaSsciService.PostPassportDetailsAsync(savePassportRequest.TravelPartyId!, (DeltaSsci.AddPassportRequest)savePassportRequest);

                var ssciObj = await _deltaSsciService.GetTravelPartiesAsync(savePassportRequest.TravelPartyId);

                responseObj = (CheckInResponse)(ssciObj, deltaTrmObj, ssciPostPassport, new PassengerId(savePassportRequest.PassengerId), true);
            }

            catch (HttpRequestException e)
            {
                _logger.LogError("Saving passport details failed for {TravelPartyId}.", savePassportRequest?.TravelPartyId);
                responseObj = (CheckInResponse)e;
            }

            return Result.Success(new Response<CheckInResponse> { Status = Status.OK, Value = responseObj });
        }

        public async Task<Result<Response<CheckInResponse>>> SaveContactTracingAsync(Domain.ContactTracing.ContactTracingRequest contactTracingRequest)
        {
            CheckInResponse responseObj;

            try
            {
                var (travelPartyId, requirementsId, selectedPassengers) = (contactTracingRequest?.TravelPartyId, contactTracingRequest?.RequirementsId, contactTracingRequest?.SelectedPassengersIds);

                var trmPatchRequest = new DeltaTrm.RequirementFormRequest
                {
                    Requests = new List<DeltaTrm.RequirementFormRequest>()
                };

                var ssciTask = _deltaSsciService.GetTravelPartiesAsync(travelPartyId);
                var ssciObj = await ssciTask;

                var trmTask = _deltaTrmService.GetRequirementsAsync(requirementsId);
                var trmObj = await trmTask;

                var ssciSegments = ssciObj?.Trip?.Segments;
                var trmTripId = trmObj?.TravelRequirements?.Trips?.First().Id;

                if (contactTracingRequest.ApplyToAllPassengers && selectedPassengers?.Any() == true)
                {
                    var selectedTrmPassengerIds = selectedPassengers.Select(id => new PassengerId(id).TrmPassengerResourceId).ToHashSet(StringComparer.OrdinalIgnoreCase);

                    var passengersToUpdate = (trmObj?.TravelRequirements?.Passengers ?? [])
                        .Where(p => selectedTrmPassengerIds.Contains(p.PassengerResourceId))
                        .Where(p => (p.AdvancePassengerInformation ?? [])
                            .SelectMany(api => api.PassengerTripHealthRequirements ?? [])
                            .SelectMany(pthr => pthr.RequirementCategories ?? [])
                            .Where(rc => rc.Type == DeltaTrm.RequirementCategoryCodeType.GOVERNMENT_FORMS)
                            .SelectMany(rc => rc.Requirements ?? [])
                            .Any(r => r.RequirementType == DeltaTrm.RequirementType.CONTACT_TRACING));

                    foreach (var passenger in passengersToUpdate)
                    {
                        trmPatchRequest.Requests.Add(new DeltaTrm.RequirementFormRequest
                        {
                            PassengerResourceId = passenger.PassengerResourceId,
                            TripId = trmTripId,
                            Using = DeltaTrm.RequirementType.CONTACT_TRACING,
                            ContactInformation = new List<DeltaTrm.ContactInformation> { (DeltaTrm.ContactInformation)contactTracingRequest.ContactTracings.First() }
                        });
                    }
                }

                else
                {
                    foreach (var contact in contactTracingRequest?.ContactTracings)
                    {
                        var passengerId = contact?.PassengerId;
                        if (passengerId.IsNullOrWhiteSpace())
                        {
                            _logger.LogError("Cannot save contact tracing because PassengerId is missing.");
                            throw new HttpRequestException(null, null, System.Net.HttpStatusCode.BadRequest);
                        }

                        trmPatchRequest.Requests.Add(new DeltaTrm.RequirementFormRequest
                        {
                            PassengerResourceId = new PassengerId(passengerId).TrmPassengerResourceId,
                            TripId = trmTripId,
                            Using = DeltaTrm.RequirementType.CONTACT_TRACING,
                            ContactInformation = new List<DeltaTrm.ContactInformation> { (DeltaTrm.ContactInformation)contact }
                        });
                    }
                }

                if (trmPatchRequest.Requests.Any())
                {
                    var deltaTrmBatchObj = await _deltaTrmService.PatchBatchRequirementsAsync(requirementsId!, trmPatchRequest);
                    responseObj = (CheckInResponse)(ssciObj, deltaTrmBatchObj, false);
                }
                else
                {
                    responseObj = (CheckInResponse)(ssciObj, trmObj, false);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError("Saving contact tracing failed for {TravelPartyId} and {RequirementsId}.", contactTracingRequest?.TravelPartyId, contactTracingRequest?.RequirementsId);
                responseObj = (CheckInResponse)ex;
            }

            return Result.Success(new Response<CheckInResponse> { Status = Status.OK, Value = responseObj });
        }

        public async Task<Result<Response<CheckInResponse>>> SaveEmergencyContactDetailsAsync(EmergencyContactRequest emergencyContactRequest)
        {
            CheckInResponse responseObj;

            try
            {
                var (travelPartyId, requirementsId, selectedPassengers) = (emergencyContactRequest?.TravelPartyId, emergencyContactRequest?.RequirementsId, emergencyContactRequest?.SelectedPassengersIds);

                var trmPatchRequest = new DeltaTrm.RequirementFormRequest
                {
                    Requests = new List<DeltaTrm.RequirementFormRequest>()
                };

                if (emergencyContactRequest.ApplyToAllPassengers && selectedPassengers?.Any() == true)
                {
                    foreach (var pId in selectedPassengers)
                    {
                        trmPatchRequest.Requests.Add(new DeltaTrm.RequirementFormRequest
                        {
                            PassengerResourceId = new PassengerId(pId).TrmPassengerResourceId,
                            Using = DeltaTrm.RequirementType.EMERGENCY_CONTACT,
                            EmergencyContact = new List<DeltaTrm.PhoneInfo> { (DeltaTrm.PhoneInfo)emergencyContactRequest.EmergencyContacts.First() }
                        });
                    }
                }

                else
                {
                    foreach (var contact in emergencyContactRequest?.EmergencyContacts)
                    {
                        var passengerId = contact?.PassengerId;
                        if (passengerId.IsNullOrWhiteSpace())
                        {
                            _logger.LogError("Cannot save emergency contact because PassengerId is missing.");
                            throw new HttpRequestException(null, null, System.Net.HttpStatusCode.BadRequest);
                        }

                        trmPatchRequest.Requests.Add(new DeltaTrm.RequirementFormRequest
                        {
                            PassengerResourceId = new PassengerId(passengerId).TrmPassengerResourceId,
                            Using = DeltaTrm.RequirementType.EMERGENCY_CONTACT,
                            EmergencyContact = new List<DeltaTrm.PhoneInfo> { (DeltaTrm.PhoneInfo)contact }
                        });
                    }
                }

                var deltaTrmObj = await _deltaTrmService.PatchBatchRequirementsAsync(requirementsId!, trmPatchRequest);

                var ssciObj = await _deltaSsciService.GetTravelPartiesAsync(travelPartyId!);

                responseObj = (CheckInResponse)(ssciObj, deltaTrmObj, false);
            }

            catch (HttpRequestException e)
            {
                _logger.LogError("Saving emergency contact details failed for {TravelPartyId} and {RequirementsId}.", emergencyContactRequest?.TravelPartyId, emergencyContactRequest?.RequirementsId);
                responseObj = (CheckInResponse)e;
            }

            return Result.Success(new Response<CheckInResponse> { Status = Status.OK, Value = responseObj });
        }

        public async Task<Result<Response<CheckInResponse>>> SaveReturnDateAsync(ReturnDateRequest returnDateRequest)
        {
            CheckInResponse responseObj;

            try
            {
                var (requirementsId, travelPartyId, selectedPassengers) = (returnDateRequest?.RequirementsId, returnDateRequest?.TravelPartyId, returnDateRequest?.SelectedPassengersIds);

                var ssciTask = _deltaSsciService.GetTravelPartiesAsync(travelPartyId);
                var trmObj = await _deltaTrmService.PatchSingleRequirementsAsync(requirementsId!, (DeltaTrm.RequirementFormRequest)returnDateRequest);

                var ssciObj = await ssciTask;

                responseObj = (CheckInResponse)(ssciObj, trmObj, true);

                var selectedTrmPassengerResourceId = new PassengerId(returnDateRequest.PassengerId).TrmPassengerResourceId;
                if (responseObj.OnlineCheckInStatus.Available && responseObj.Passengers.Any(x => 
                    x.Requirements!.ReturnDateStatus == Constants.ReturnDateStatus.REQUIRED &&
                    x.Requirements.IsReturnDateDeclined && 
                    new PassengerId(x.PassengerId).TrmPassengerResourceId.Is(selectedTrmPassengerResourceId)))
                {
                    var currentPassenger = responseObj.Passengers
                        .First(x => new PassengerId(x.PassengerId).TrmPassengerResourceId.Is(selectedTrmPassengerResourceId));
                    
                    var errorCode = (selectedPassengers?.Count() == 1) ? "NoReturnDateSinglePassenger" : "NoReturnDateMultiPassenger";

                    var errorDetails = CheckInMessageHelper.GetNoReturnDateandSecondaryDocErrorMessages(errorCode).First();

                    currentPassenger.Requirements!.PassportErrors = new List<APIErrorResponse>
                    {
                        new() { Code = errorDetails.Code, Title = errorDetails.Title, Message = errorDetails.Message }
                    };
                }
            }

            catch (HttpRequestException e)
            {
                _logger.LogError("Saving return date failed for {TravelPartyId} and {RequirementsId}.", returnDateRequest?.TravelPartyId, returnDateRequest?.RequirementsId);
                responseObj = (CheckInResponse)e;
            }

            return Result.Success(new Response<CheckInResponse> { Status = EAPI.Domain.Entities.Common.Status.OK, Value = responseObj });
        }

        public async Task<Result<Response<BoardingPassResponse>>> BoardingPassAsync(BoardingPassRequest request)
        {
            try
            {
                TravelParty travelPartyRes = null;

                try
                {
                    travelPartyRes = await _deltaSsciService.PostSeatsAsync(request?.TravelPartyId, new AddSeatRequest { RecordLocator = request?.PnrLocator });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Delta seat API failed for PNR: {PnrLocator}; retrieving the existing travel party instead.", request?.PnrLocator);
                    travelPartyRes = await _deltaSsciService.GetTravelPartiesAsync(request?.TravelPartyId);
                }

                var deltaBoardingPassObj = await _deltaSsciService.PostBoardingDocumentsAsync(request?.TravelPartyId);

                if (deltaBoardingPassObj.BoardingDocuments == null && deltaBoardingPassObj.CheckinFailed)
                    throw new Exception("empty boarding documents.");

                var boardingPassRes = (BoardingPassResponse)(travelPartyRes, deltaBoardingPassObj);

                return Result.Success(new Response<BoardingPassResponse>
                {
                    Status = Status.OK,
                    Value = boardingPassRes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delta boarding pass creation failed for PNR: {PnrLocator}.", request?.PnrLocator);
                throw;
            }
        }

        public async Task<Result<Response<BoardingPassResponse>>> RetrieveBoardingPass(BoardingPassRequest boardingPassRequest)
        {
            try
            {
                TravelParty ssciObj = new TravelParty();
                try
                {
                    ssciObj = await _deltaSsciService.PostTravelPartiesAsync((DeltaSsci.InputObject)boardingPassRequest);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Delta travel-party retrieval failed for {PnrLocator}; retrieving from cache instead.", boardingPassRequest?.PnrLocator);

                    var ssciCacheObj = await _distributedCacheService.Get<TravelParty>($"{boardingPassRequest.PnrLocator}-{boardingPassRequest.OriginCode}");
                    ssciObj = ssciCacheObj ?? await _deltaSsciService.GetTravelPartiesAsync(ssciObj?.Id);
                }
                if (ssciObj != null && ssciObj.CheckinAdvisory == null)
                {
                    await _distributedCacheService.Set($"{boardingPassRequest.PnrLocator}-{boardingPassRequest.OriginCode}", ssciObj, false, TimeSpan.FromSeconds(0), TimeSpan.FromHours(ConfigReader.Get<int>(Constants.Environment.BoardingPassSlidingExpiryHours)));
                }

                if (ssciObj?.Trip == null)
                {
                    var ssciCacheObj = await _distributedCacheService.Get<TravelParty>($"{boardingPassRequest.PnrLocator}-{boardingPassRequest.OriginCode}");

                    ssciObj = ssciCacheObj ?? await _deltaSsciService.GetTravelPartiesAsync(ssciObj?.Id);

                }

                DeltaSsci.TravelParty ssciObjForSelectedPsg = null;
                BoardingDocumentsResponse boardingPassObj = null;

                var checkedInPsgs = ssciObj?.Passengers.Where(p => p.CheckedIn || p.CheckedInOnASL).Select(p => (passengerId: $"{p.FirstNIN}.{p.LastNIN}|{p.Id}|{p.CustomerId}|", isInfant: p.InfantPassenger));

                if (checkedInPsgs?.Any() == true)
                {
                    if (ssciObj?.Passengers.Any(p => (!p.CheckedIn && !p.CheckedInOnASL) || p.InfantPassenger) == true)
                    {
                        ssciObjForSelectedPsg = await _deltaSsciService.PatchTravelPartiesAsync(ssciObj.Id, (DeltaSsci.SelectPassengerRequest)new Domain.SelectPassengerRequest { SelectablePassengers = checkedInPsgs });
                    }

                    try
                    {
                        boardingPassObj = await _deltaSsciService.PostBoardingDocumentsAsync(ssciObj?.Id);
                        if (boardingPassObj?.BoardingDocuments?.Any() == true)
                            await _distributedCacheService.Set($"{boardingPassRequest.PnrLocator}-{boardingPassRequest.OriginCode}-BoardingPass", boardingPassObj, false, TimeSpan.FromSeconds(0), TimeSpan.FromHours(ConfigReader.Get<int>(Constants.Environment.BoardingPassSlidingExpiryHours)));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Delta boarding-pass retrieval failed for {PnrLocator}; retrieving from cache instead.", boardingPassRequest?.PnrLocator);
                        boardingPassObj = await _distributedCacheService.Get<BoardingDocumentsResponse>($"{boardingPassRequest.PnrLocator}-{boardingPassRequest.OriginCode}-BoardingPass");
                    }
                }

                if (ssciObjForSelectedPsg?.Passengers != null && ssciObj?.Passengers != null && ssciObjForSelectedPsg.Passengers.Count != ssciObj.Passengers.Count)
                {
                    var unCheckedInPsgs = ssciObj.Passengers.Where(p => ssciObjForSelectedPsg.Passengers.All(existing => existing.CustomerId != p.CustomerId)).ToList();

                    unCheckedInPsgs.ForEach(ssciObjForSelectedPsg.Passengers.Add);
                }

                var boardingPassRes = (BoardingPassResponse)(ssciObjForSelectedPsg ?? ssciObj, boardingPassObj);

                return Result.Success(new Response<BoardingPassResponse>
                {
                    Status = Status.OK,
                    Value = boardingPassRes
                });
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Delta check-in retrieval failed for PNR: {PnrLocator}.", boardingPassRequest?.PnrLocator);
                throw;
            }
        }

    }
}
