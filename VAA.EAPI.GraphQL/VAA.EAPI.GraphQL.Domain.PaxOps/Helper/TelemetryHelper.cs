using Newtonsoft.Json;
using NodaTime;
using NodaTime.Text;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.Baggage;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.Basket;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.BoardingPass;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.ContactTracing;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.EmergencyContact;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.Enums;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.SavePassport;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.SaveReturnDate;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.SecureFlightInfo;
using CheckInMSResponse = VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.CheckInMS.CheckInResponse;
using CheckInResponse = VAA.EAPI.GraphQL.Domain.PaxOps.Models.CheckInResponse;


namespace VAA.EAPI.GraphQL.Domain.PaxOps.Helper
{
    public static class TelemetryHelper
    {
        public static Dictionary<string, object> SecureFlightInfoDetails(SecureFlightInfoResponse response, string? pnrLocator = null)
        {
            var passengerDetails = response.PassengerDetails;

            return new Dictionary<string, object>
            {
                { "PnrLocator", pnrLocator },
                { "SecureFlightReservationId", response.SecureFlightReservationId },
                { "PassengerId", passengerDetails.PassengerId },
                { "PassengerFirstNin", passengerDetails.PassengerFirstNin },
                { "PassengerLastNin", passengerDetails.PassengerLastNin },
                { "HasKnownTravellerNumber", !passengerDetails.KnownTravellerNumber.IsNullOrWhiteSpace() },
                { "HasCanadianTravelNumber", !passengerDetails.CanadianTravelNumber.IsNullOrWhiteSpace() },
                { "HasRedressNumber", !passengerDetails.RedressNumber.IsNullOrWhiteSpace() }
            };
        }

        public static Dictionary<string, object> SecureFlightInfoFailed(string? pnrLocator, LoggingFailureContext failureContext, ErrorDetails? error, string? secureFlightReservationId = null)
        {
            if (pnrLocator.IsNullOrWhiteSpace() && secureFlightReservationId.IsNullOrWhiteSpace())
            {
                return [];
            }

            return new Dictionary<string, object>
            {
                { "PnrLocator", pnrLocator },
                { "SecureFlightReservationId", secureFlightReservationId },
                { "Status", "FAILED" },
                { "SecureFlightInfoStage", failureContext },
                { "SecureFlightInfoError", error is null ? null : Json.Serialize(new
                    {
                        error.Code,
                        error.Message,
                        error.Title
                    }) }
            };
        }

        public static async Task<Dictionary<string, object>> CheckinSessionCreated(CheckInMSResponse response, string originCode, string pnrLocator)
        {
            if (response == null)
            {
                return new Dictionary<string, object>();
            }
            var domainResponse = await CheckInResponse.CheckInResponseAsync(response, null);

            return new Dictionary<string, object>()
            {
                { "PnrLocator", pnrLocator },
                { "TravelPartyId", response.TravelPartyId },
                { "RequirementsId", response.RequirementsId },
                { "PassengerIds", Json.Serialize( response.Passengers?.Select(p => p.PassengerId).ToList())},
                { "PassengerCount", response.Passengers?.Select(p => p.PassengerId).ToList()?.Count },
                { "Segments", GetSegmentDetails(domainResponse) },
                { "OriginCode", originCode }
            };
        }
        public static async Task<Dictionary<string, object>> CheckinPassengersRequested(string checkInId, IEnumerable<string> passengerIds)
        {
            var checkInInfo = await GetCheckInInfoAsync(checkInId, null);

            return new Dictionary<string, object>()
            {
                { "PnrLocator", checkInInfo.Pnr },
                { "TravelPartyId",  checkInInfo.TravelPartyId },
                { "RequirementsId", checkInInfo.RequirementsId },
                { "PassengerIds", Json.Serialize( passengerIds.ToList())},
                { "OriginCode", checkInInfo.OriginCode }
            };
        }
        public static async Task<Dictionary<string, object>> CheckinPassengersSelected(CheckInResponse response, string checkInId)
        {
            if (response == null)
            {
                return new Dictionary<string, object>(); ;
            }

            var checkInInfo = await GetCheckInInfoAsync(checkInId, null);

            return new Dictionary<string, object>()
            {
                { "PnrLocator", checkInInfo.Pnr },
                { "TravelPartyId",  checkInInfo.TravelPartyId },
                { "RequirementsId", checkInInfo.RequirementsId },
                { "PassengerIds", Json.Serialize( response.Passengers?.Select(p => p.PassengerId).ToList())},
                { "PassengerCount", response.Passengers?.Select(p => p.PassengerId).ToList()?.Count },
                { "Segments", GetSegmentDetails(response) },
                { "OriginCode", checkInInfo.OriginCode }
            };
        }
        public static async Task<Dictionary<string, object>> PassportDetailsSubmitted(SavePassportRequest savePassportRequest)
        {
            if (savePassportRequest == null)
            {
                return new Dictionary<string, object>(); ;
            }
            string passengerIds = null;
            var checkInInfo = new CheckInInfo();

            if (!string.IsNullOrWhiteSpace(savePassportRequest.CheckInId))
            {
                checkInInfo = await GetCheckInInfoAsync(savePassportRequest?.CheckInId, null);
                passengerIds = await GetPassengerListJson(savePassportRequest?.CheckInId);
            }
            
            return new Dictionary<string, object>()
            {
                { "PnrLocator", checkInInfo.Pnr },
                { "OriginCode", checkInInfo.OriginCode },
                { "TravelPartyId",  checkInInfo.TravelPartyId },
                { "RequirementsId", checkInInfo.RequirementsId },
                { "PassengerId",  savePassportRequest?.PassengerId },
                { "IssuingCountry", savePassportRequest?.Document?.IssuingCountry?.Code },
                { "Scanned", savePassportRequest?.IsScanned },
                { "Nationality", savePassportRequest?.Document?.Nationality?.Code},
                { "ExpiresWithinOneYear",  DateTime.TryParse(savePassportRequest?.Document?.ExpirationDate, out var exp) &&
                exp > DateTime.UtcNow &&
                exp <= DateTime.UtcNow.AddYears(1) },
            };
        }
        public static async Task<Dictionary<string, object>> PassportDetailsSaved(CheckInMSResponse response, string checkInId)
        {
            if (response == null)
            {
                return new Dictionary<string, object>(); ;
            }

            var domainResponse = await CheckInResponse.CheckInResponseAsync(response, checkInId);
            var checkInInfo = await GetCheckInInfoAsync(checkInId, null);

            return new Dictionary<string, object>()
            {
                { "PnrLocator", checkInInfo.Pnr },
                { "OriginCode", checkInInfo.OriginCode },
                { "TravelPartyId",  checkInInfo.TravelPartyId },
                { "RequirementsId", checkInInfo.RequirementsId },
                { "PassengerId", Json.Serialize(response.Passengers?.Select(p => p.PassengerId).ToList())},
                { "Destination", response.CurrentSlice?.Destination?.CountryCode },
                { "Segments", GetSegmentDetails(domainResponse) },
                { "CheckInRequirements", GetCheckInRequirements(domainResponse) }
            };
        }
        public static async Task<Dictionary<string, object>> ReturnDateSubmitted(SaveReturnDateRequest saveReturnDateRequest)
        {
            if (saveReturnDateRequest == null)
            {
                return new Dictionary<string, object>(); ;
            }

            var checkInInfo = await GetCheckInInfoAsync(saveReturnDateRequest.CheckInId, null);

            return new Dictionary<string, object>()
            {
                { "PnrLocator", checkInInfo.Pnr },
                { "OriginCode", checkInInfo.OriginCode },
                { "TravelPartyId",  checkInInfo.TravelPartyId },
                { "RequirementsId", checkInInfo.RequirementsId },
                { "PassengerId", saveReturnDateRequest.PassengerId},
                { "ReturnDate", saveReturnDateRequest.ReturnDate},
                { "ReturnOn", saveReturnDateRequest.ReturnOn}
            };
        }
        public static async Task<Dictionary<string, object>> ReturnDateSaved(CheckInMSResponse response, string checkInId)
        {
            if (response == null)
            {
                return new Dictionary<string, object>(); ;
            }
            var domainResponse = await CheckInResponse.CheckInResponseAsync(response, checkInId);
            var checkInInfo = await GetCheckInInfoAsync(checkInId, null);

            return new Dictionary<string, object>()
            {
                { "PnrLocator", checkInInfo.Pnr },
                { "OriginCode", checkInInfo.OriginCode },
                { "TravelPartyId",  checkInInfo.TravelPartyId },
                { "RequirementsId", checkInInfo.RequirementsId },
                { "PassengerId", Json.Serialize( response.Passengers?.Select(p => p.PassengerId).ToList()) },
                { "Destination", response.CurrentSlice?.Destination?.CountryCode },
                { "Segments", GetSegmentDetails(domainResponse) },
                { "CheckInRequirements", GetCheckInRequirements(domainResponse) }

            };
        }
        public static async Task<Dictionary<string, object>> EmergencyContactSubmitted(string checkInId, bool applyToAllPassengers, IEnumerable<EmergencyContact> emergencyContacts)
        {
            var passengerIds = await GetPassengerListJson(checkInId);
            var checkInInfo = await GetCheckInInfoAsync(checkInId, null);

            return new Dictionary<string, object>()
            {
                { "PnrLocator", checkInInfo.Pnr },
                { "OriginCode", checkInInfo.OriginCode },
                { "TravelPartyId",  checkInInfo.TravelPartyId },
                { "RequirementsId", checkInInfo.RequirementsId },
                { "PassengerIds", passengerIds },
                { "SameForAllPassengers", applyToAllPassengers }
            };
        }
        public static async Task<Dictionary<string, object>> EmergencyContactSaved(CheckInResponse response, string checkInId)
        {
            if (response == null)
            {
                return new Dictionary<string, object>(); ;
            }

            var checkInInfo = await GetCheckInInfoAsync(checkInId, null);

            return new Dictionary<string, object>()
            {
                { "PnrLocator", checkInInfo.Pnr },
                { "OriginCode", checkInInfo.OriginCode },
                { "TravelPartyId",  checkInInfo.TravelPartyId },
                { "RequirementsId", checkInInfo.RequirementsId },
                { "PassengerId",Json.Serialize( response.Passengers?.Select(p => p.PassengerId).ToList())},
                { "Segments", GetSegmentDetails(response) },
                { "CheckInRequirements", GetCheckInRequirements(response) }
            };
        }
        public static async Task<Dictionary<string, object>> ContactTracingSubmitted(string checkInId, bool applyToAll, IEnumerable<ContactTracing> contactTracings)
        {
            var passengerIds = await GetPassengerListJson(checkInId);
            var checkInInfo = await GetCheckInInfoAsync(checkInId, null);

            return new Dictionary<string, object>()
            {
                { "PnrLocator", checkInInfo.Pnr },
                { "OriginCode", checkInInfo.OriginCode },
                { "TravelPartyId",  checkInInfo.TravelPartyId },
                { "RequirementsId", checkInInfo.RequirementsId },
                { "PassengerIds", passengerIds },
                { "SameForAllPassengers", applyToAll }
            };
        }
        public static async Task<Dictionary<string, object>> ContactTracingSaved(CheckInResponse response, string checkInId)
        {
            if (response == null)
            {
                return new Dictionary<string, object>(); ;
            }
            var checkInInfo = await GetCheckInInfoAsync(checkInId, null);

            return new Dictionary<string, object>()
            {
                { "PnrLocator", checkInInfo.Pnr },
                { "OriginCode", checkInInfo.OriginCode },
                { "TravelPartyId",  checkInInfo.TravelPartyId },
                { "RequirementsId", checkInInfo.RequirementsId },
                { "Segments", GetSegmentDetails(response) },
                { "CheckInRequirements", GetCheckInRequirements(response) }
            };
        }
        public static async Task<Dictionary<string, object>> AutoSeatAssigned(string checkInId, BoardingPassResponse boardingPassResponse)
        {
            var passengerIds = await GetPassengerListJson(checkInId);
            var checkInInfo = await GetCheckInInfoAsync(checkInId, null);
            return new Dictionary<string, object>()
            {
                { "PnrLocator", checkInInfo.Pnr },
                { "OriginCode", checkInInfo.OriginCode },
                { "PassengerIds", passengerIds },
                { "Seats", Json.Serialize(boardingPassResponse.BoardingPasses?.Select(p => p.SeatNumber).ToList()) }
            };
        }

        private static string SerializeBoardingPasses(IEnumerable<BoardingPass> boardingPasses)
        {
            if (boardingPasses.IsNullOrEmpty()) return string.Empty;

            return JsonConvert.SerializeObject(
                boardingPasses.Select(bp => new
                {
                    Generated = bp.CheckedIn,
                    OriginCode = bp.OriginAirportCode ?? string.Empty,
                    DestinationCode = bp.DestinationAirportCode ?? string.Empty,
                    OperatingAirline = bp.OperatingAirlineName ?? string.Empty,
                    PassengerNIN = bp.PassengerNIN ?? string.Empty,
                    Remarks = bp.VerificationStatusMessage ?? string.Empty,
                    Seat = bp.SeatNumber ?? string.Empty,
                    HasQr = !bp.QrCode.IsNullOrEmpty(),
                    HasDesktopBoardingPass = !bp.DesktopBoardingPassBase64.IsNullOrEmpty(),
                    // Only available from Trips API, so will be null when generating boarding pass
                    PTC = bp.PassengerTypeCode ?? string.Empty
                }).ToList()
            );
        }

        public static async Task<Dictionary<string, object>> DayOfTravel(string pnrLocator, string originCode, string givenNames, string surname, BoardingPassResponse boardingPassResponse, string? errorReason)
        {
            return new Dictionary<string, object>
            {
                { "PassengerCount", boardingPassResponse?.BoardingPasses?.Select(p => p.PassengerNIN).Distinct().Count() },
                { "BoardingPasses", SerializeBoardingPasses(boardingPassResponse?.BoardingPasses) },
                { "PnrLocator", pnrLocator },
                { "PaxInitials", givenNames.IsNullOrEmpty() || surname.IsNullOrEmpty() ? "Missing" : $"{givenNames[0]}{surname[0]}" },
                { "OriginCode", originCode },
                { "ErrorReason", errorReason }
            };
        }

        public static async Task<Dictionary<string, object>> CheckinEndSuccess(string checkInId, BoardingPassResponse boardingPassResponse)
        {
            var checkInInfo = await GetCheckInInfoAsync(checkInId, null);

            return new Dictionary<string, object>
            {
                { "PnrLocator", checkInInfo.Pnr },
                { "TravelPartyId", checkInInfo.TravelPartyId },
                { "RequirementsId", checkInInfo.RequirementsId },
                { "PassengerCount", boardingPassResponse?.BoardingPasses?.Select(p => p.PassengerNIN).Distinct().Count() },
                { "BoardingPasses", SerializeBoardingPasses(boardingPassResponse?.BoardingPasses) },
                { "OriginCode", checkInInfo.OriginCode }
            };
        }

        public static async Task<Dictionary<string, object>> CheckinEndRefused(OnlineCheckInStatus onlineCheckInStatus, CheckInMSResponse? msResponse, CheckInResponse? response, BoardingPassResponse boardingResponse, string? pnrLocator, string? checkInId, Enum? checkInStage)
        {
            var passes = boardingResponse?.BoardingPasses ?? Enumerable.Empty<BoardingPass>();

            var checkInInfo = await GetCheckInInfoAsync(checkInId, pnrLocator);
            string passengerIds = !string.IsNullOrWhiteSpace(checkInId) ? await GetPassengerListJson(checkInId) : null;
            var telemetry = new Dictionary<string, object>
            {
                { "PnrLocator", pnrLocator.IsNullOrWhiteSpace()? checkInInfo.Pnr: pnrLocator },
                { "TravelPartyId", checkInInfo.TravelPartyId },
                { "RequirementsId", checkInInfo.RequirementsId },
                { "PassengerIds", passengerIds },
                { "CheckInStage", checkInStage?.ToString() },
                { "ErrorCode", onlineCheckInStatus?.Code?? msResponse?.OnlineCheckInStatus?.Code ?? response?.OnlineCheckInStatus?.Code},
                { "ErrorMessage", onlineCheckInStatus?.Message?? msResponse?.OnlineCheckInStatus?.Message ?? response?.OnlineCheckInStatus?.Message},
                { "AdvisoryText1", msResponse?.OnlineCheckInStatus?.AdvisoryText1 },
                { "BoardingPasses", SerializeBoardingPasses(boardingResponse?.BoardingPasses) },
                { "OriginCode", checkInInfo.OriginCode }
            };

            if (msResponse?.Passengers != null)
            {
                telemetry.Add("CheckInRequirements", GetCheckInRequirements(msResponse.Passengers));
            }

            if (msResponse?.CurrentSlice?.Segments != null)
            {
                telemetry.Add("SegmentDetails", GetSegmentDetails(msResponse.CurrentSlice.Segments));
            }

            return telemetry;
        }

        public static async Task<Dictionary<string, object>> BaggageOffersRetrievalInitiated(string checkInId)
        {
            var passengerIds = await GetPassengerListJson(checkInId);
            var checkInInfo = await GetCheckInInfoAsync(checkInId, null);

            return new Dictionary<string, object>()
            {
                { "PnrLocator", checkInInfo.Pnr },
                { "TravelPartyId",  checkInInfo.TravelPartyId },
                { "RequirementsId", checkInInfo.RequirementsId },
                { "PassengerIds", passengerIds },
                { "OriginCode", checkInInfo.OriginCode }
            };
        }

        public static async Task<Dictionary<string, object>> BaggageOptionsRequested(string checkInId, BaggageResponse baggageResponse)
        {
            var passengerIds = await GetPassengerListJson(checkInId);
            var checkInInfo = await GetCheckInInfoAsync(checkInId, null);

            var baggageAdvisory = baggageResponse.Passengers?
                .Where(p => p.BaggageAdvisory != null)
                .Select(p => new
                {
                    PassengerNiN = p.PassengerNiN,
                    Title = p.BaggageAdvisory?.Title ?? string.Empty,
                    Message = p.BaggageAdvisory?.Message ?? string.Empty
                });

            return new Dictionary<string, object>()
            {
                { "PnrLocator", checkInInfo.Pnr },
                { "TravelPartyId",  checkInInfo.TravelPartyId },
                { "RequirementsId", checkInInfo.RequirementsId },
                { "PassengerIds", passengerIds },
                { "OriginCode", checkInInfo.OriginCode },
                { "baggageAdvisory", Json.Serialize(baggageAdvisory ?? Enumerable.Empty<object>()) }
            };
        }

        public static async Task<Dictionary<string, object>> BaggageDetailsSubmitted(SaveBaggageRequest saveBaggageRequest)
        {
            if (saveBaggageRequest == null)
            {
                return new Dictionary<string, object>(); ;
            }
            string passengerIds = null;
            var checkInInfo = new CheckInInfo();

            if (!string.IsNullOrWhiteSpace(saveBaggageRequest.CheckInId))
            {
                checkInInfo = await GetCheckInInfoAsync(saveBaggageRequest?.CheckInId, null);
                passengerIds = await GetPassengerListJson(saveBaggageRequest?.CheckInId);
            }

            return new Dictionary<string, object>()
            {
                { "PnrLocator", checkInInfo.Pnr },
                { "OriginCode", checkInInfo.OriginCode },
                { "TravelPartyId",  checkInInfo.TravelPartyId },
                { "RequirementsId", checkInInfo.RequirementsId },
                { "PassengerIds", passengerIds },
                { "SelectedBaggageOffer", GetSelectedBaggageOffers(saveBaggageRequest.SelectedBaggageOffer) }
            };
        }
        public static async Task<Dictionary<string, object>> BaggageDetailsSaved(string checkInId, bool? isPaidBags)
        {
            if (string.IsNullOrWhiteSpace(checkInId))
            {
                return new Dictionary<string, object>(); ;
            }

            var checkInInfo = await GetCheckInInfoAsync(checkInId, null);
            string passengerIds = await GetPassengerListJson(checkInId);

            return new Dictionary<string, object>()
            {
                { "PnrLocator", checkInInfo.Pnr },
                { "OriginCode", checkInInfo.OriginCode },
                { "TravelPartyId",  checkInInfo.TravelPartyId },
                { "RequirementsId", checkInInfo.RequirementsId },
                { "PassengerIds", passengerIds },
                { "Status", "SUCCESS" },
                { "HasPaidBaggage", isPaidBags },
                { "BasketId", checkInInfo?.BasketId }
            };
        }
        public static async Task<Dictionary<string, object>> BaggageDetailsFailed(string checkInId, bool? isPaidBags, LoggingFailureContext baggageFailureContext)
        {
            if (string.IsNullOrWhiteSpace(checkInId))
            {
                return new Dictionary<string, object>(); ;
            }

            var checkInInfo = await GetCheckInInfoAsync(checkInId, null);
            string passengerIds = await GetPassengerListJson(checkInId);

            return new Dictionary<string, object>()
            {
                { "PnrLocator", checkInInfo.Pnr },
                { "OriginCode", checkInInfo.OriginCode },
                { "TravelPartyId",  checkInInfo.TravelPartyId },
                { "RequirementsId", checkInInfo.RequirementsId },
                { "PassengerIds", passengerIds },
                { "Status", "FAILED" },
                { "HasPaidBaggage", isPaidBags },
                { "BasketId", checkInInfo?.BasketId },
                { "BaggageStage", baggageFailureContext }
            };
        }

        public static async Task<Dictionary<string, object>> PaymentEligibilityInitiation(string checkinId)
        {
            if (checkinId.IsNullOrEmpty())
            {
                return new Dictionary<string, object>();
            }

            var checkInInfo = await GetCheckInInfoAsync(checkinId, null);
            string passengerIds = await GetPassengerListJson(checkinId);

            return new Dictionary<string, object>()
            {
                { "PnrLocator", checkInInfo.Pnr },
                { "OriginCode", checkInInfo.OriginCode },
                { "TravelPartyId",  checkInInfo.TravelPartyId },
                { "RequirementsId", checkInInfo.RequirementsId },
                { "PassengerIds", passengerIds },
                { "BasketId", checkInInfo.BasketId }
            };
        }

        public static async Task<Dictionary<string, object>> PaymentEligibilityChecked(PaymentCheckRequest request, Basket basketResponse, string travelPartyId, bool? isPaymentRequired)
        {
            if (request == null)
            {
                return new Dictionary<string, object>();
            }

            var checkInInfo = await GetCheckInInfoAsync(request.CheckInId, null);
            string passengerIds = await GetPassengerListJson(request.CheckInId);

            return new Dictionary<string, object>()
            {
                { "PnrLocator", checkInInfo.Pnr },
                { "OriginCode", checkInInfo.OriginCode },
                { "TravelPartyId",  travelPartyId },
                { "RequirementsId", checkInInfo.RequirementsId },
                { "PassengerIds", passengerIds },
                { "BasketId", basketResponse?.BasketId },
                { "CheckedBaggageDetails", GetBasketDetails(basketResponse) },
                { "HasPaidBaggage", isPaymentRequired}
            };
        }
        public static async Task<Dictionary<string, object>> PaymentSessionCreated(BaggagePaymentCheckResponse paymentEligibilityCheckResponse, Basket basketResponse)
        {
            if (paymentEligibilityCheckResponse == null)
            {
                return new Dictionary<string, object>();
            }

            var checkInInfo = await GetCheckInInfoAsync(paymentEligibilityCheckResponse.CheckInId, null);
            string passengerIds = await GetPassengerListJson(paymentEligibilityCheckResponse.CheckInId);

            var freeBagsByPassenger = GetFreeBagsPerPassenger(basketResponse);

            return new Dictionary<string, object>()
            {
                { "PnrLocator", checkInInfo.Pnr },
                { "OriginCode", checkInInfo.OriginCode },
                { "TravelPartyId",  checkInInfo.TravelPartyId },
                { "RequirementsId", checkInInfo.RequirementsId },
                { "PassengerIds", passengerIds },
                { "BasketId", basketResponse.BasketId },
                { "FreeBagsByPassenger", freeBagsByPassenger },
                { "PaymentPending", paymentEligibilityCheckResponse.PaymentPending.ToString() },
                { "TotalAmount", paymentEligibilityCheckResponse.Amount.TotalAmount },
                { "Currency", paymentEligibilityCheckResponse.Amount.Currency },
            };
        }

        private static string GetFreeBagsPerPassenger(Basket basket)
        {
            var freeBagsByPassenger = basket?.CheckedBaggageDetails?.PassengerBaggage?
                .GroupBy(pb => pb.PassengerNiN)?
                .Select(g => new
                {
                    PassengerNiN = g.Key,
                    FreeBagsCount = g.First().NumOfFreeBags,
                });

            return JsonConvert.SerializeObject(freeBagsByPassenger);
        }

        private static string GetBasketDetails(Basket basket)
        {
            return JsonConvert.SerializeObject(new
            {
                CheckedBaggageDetails = basket.CheckedBaggageDetails == null ? null : new
                {
                    basket.CheckedBaggageDetails.ProductType,
                    PassengerBaggage = basket.CheckedBaggageDetails.PassengerBaggage?
                            .Select(pb => new
                            {
                                pb.PassengerNiN,
                                pb.BaggageOfferId,
                                pb.BaggageCount,
                                pb.TotalPrice,
                                pb.CurrencyCode,
                                pb.DecimalPrecisionCount
                            })
                }
            });
        }

        private static string GetSelectedBaggageOffers(IEnumerable<SelectedBaggageOffer> baggageOffers)
        {
            if (baggageOffers == null)
            {
                return string.Empty;
            }

            return JsonConvert.SerializeObject(
                baggageOffers.Select(bo => new
                {
                    bo.PassengerNiN,
                    bo.BaggageOfferId,
                    bo.BaggageCount,
                    bo.TotalPrice,
                    bo.CurrencyCode,
                    bo.DecimalPrecisionCount
                }).ToList()
            );
        }

        private static string GetSegmentDetails(CheckInResponse response)
        {
            return JsonConvert.SerializeObject(
                 response.CurrentSlice?.Segments?.Select(s => new
                 {
                     OriginAirportCode = s.Origin?.Code,
                     DestinationAirportCode = s.Destination?.Code,
                     DestinationCountryCode = s.Destination?.CountryCode,

                     OperatingAirline = s.OperatingCarrier?.CarrierCode,
                     OperatingFlightNumber = s.OperatingCarrier?.FlightNum,

                     ScheduledDepartureUtcDateTime = s.ScheduledDepartureUtcDateTime
                     .InZoneStrictly(DateTimeZone.Utc)
                     .ToInstant()
                     .ToDateTimeUtc()
                     .ToString("o"),
                     ScheduledArrivalUtcDateTime =
                     s.ScheduledArrivalUtcDateTime
                     .InZoneStrictly(DateTimeZone.Utc)
                     .ToInstant()
                     .ToDateTimeUtc()
                     .ToString("o")
                 }).ToList());
        }

        private static string GetSegmentDetails(IEnumerable<Infrastructure.Models.CheckInMS.Segment> segments)
        {
            return JsonConvert.SerializeObject(
                segments.Select(s => new
                {
                    OriginAirportCode = s.Origin?.Code,
                    DestinationAirportCode = s.Destination?.Code,
                    DestinationCountryCode = s.Destination?.CountryCode,
                    OperatingAirline = s.OperatingCarrier?.CarrierCode,
                    OperatingFlightNumber = s.OperatingCarrier?.FlightNum,
                    ScheduledDepartureUtcDateTime = s.ScheduledDepartureUtcDateTime
                        .InZoneStrictly(DateTimeZone.Utc)
                        .ToInstant()
                        .ToDateTimeUtc()
                        .ToString("o"),
                    ScheduledArrivalUtcDateTime = s.ScheduledArrivalUtcDateTime
                        .InZoneStrictly(DateTimeZone.Utc)
                        .ToInstant()
                        .ToDateTimeUtc()
                        .ToString("o")
                }).ToList());
        }

        private static string GetCheckInRequirements(CheckInResponse response)
        {
            return JsonConvert.SerializeObject(response.Passengers?
                .Select(p => new
                {
                    PassengerId = p.PassengerId,
                    PassportProvided = p.Requirements?.PassportProvided,
                    PassportVerified = p.Requirements?.PassportVerified,
                    BaggageDeclared = p.Requirements?.BaggageDeclared,
                    ContactTracingStatus = p.Requirements != null ? p.Requirements.ContactTracing.ToString() : null,
                    ContactTracingAvailableFromUtc = p.Requirements?.ContactTracingAvailableFromUtc,
                    ContactTracingAvailableFromDescription = p.Requirements?.ContactTracingAvailableFromDescription,
                    CheckedIn = p.Requirements?.CheckedIn,
                    ReturnDate = p.Requirements?.ReturnDate.HasValue == true
                        ? LocalDatePattern.CreateWithInvariantCulture("yyyy-MM-dd").Format(p.Requirements.ReturnDate.Value)
                        : null,
                    PassportErrorCodes = p.Requirements?.PassportErrors?
                        .Select(e => e.Code)
                        .Where(x => !x.IsNullOrWhiteSpace())
                        .ToList(),
                    SecondaryDocumentStatus = p.Requirements?.SecondaryDocumentStatus.ToString(),
                    SecondaryDocuments = p.Requirements?.SecondaryDocuments?
                        .Select(sd => sd.CountryCode + ":" + sd.DocumentType)
                        .ToList()
                })
                .ToList());
        }

        private static string GetCheckInRequirements(IEnumerable<Infrastructure.Models.CheckInMS.Passenger> passengers)
        {
            return JsonConvert.SerializeObject(passengers
                .Select(p => new
                {
                    PassengerId = p.PassengerId,
                    PassportProvided = p.Requirements?.PassportProvided,
                    PassportVerified = p.Requirements?.PassportVerified,
                    BaggageDeclared = p.Requirements?.BaggageDeclared,
                    ContactTracingStatus = p.Requirements?.ContactTracing.ToString(),
                    ContactTracingAvailableFromUtc = p.Requirements?.ContactTracingAvailableFromUtc,
                    ContactTracingAvailableFromDescription = p.Requirements?.ContactTracingAvailableFromDescription,
                    CheckedIn = p.Requirements?.CheckedIn,
                    ReturnDate = p.Requirements?.ReturnDate.HasValue == true
                        ? LocalDatePattern.CreateWithInvariantCulture("yyyy-MM-dd").Format(p.Requirements.ReturnDate.Value)
                        : null,
                    PassportErrorCodes = p.Requirements?.PassportErrors?
                        .Select(e => e.Code)
                        .Where(code => !code.IsNullOrWhiteSpace())
                        .ToList(),
                    SecondaryDocumentStatus = p.Requirements?.SecondaryDocumentStatus.ToString(),
                    SecondaryDocuments = p.Requirements?.SecondaryDocuments?
                        .Select(document => document.CountryCode + ":" + document.DocumentType)
                        .ToList()
                })
                .ToList());
        }

        private static async Task<string> GetPassengerListJson(string checkInId)
        {
            var selectedPassengers = await CheckInIdHelper.GetSelectedPassengers(checkInId) ?? Enumerable.Empty<string>();
            return Json.Serialize(selectedPassengers.ToList());
        }

        private static async Task<CheckInInfo> GetCheckInInfoAsync(string checkInId, string? pnrLocator)
        {
            if (string.IsNullOrWhiteSpace(checkInId))
                return new CheckInInfo();

            var pnrTask = pnrLocator.IsNullOrWhiteSpace() ? CheckInIdHelper.GetPnrLocator(checkInId)
                : Task.FromResult(pnrLocator);

            var travelPartyIdTask = CheckInIdHelper.GetTravelPartyId(checkInId);
            var requirementsTask = CheckInIdHelper.GetRequirementsId(checkInId);
            var originCodeTask = CheckInIdHelper.GetOriginCode(checkInId);
            var basketIdTask = CheckInIdHelper.GetBasketId(checkInId);

            await Task.WhenAll(pnrTask, travelPartyIdTask, requirementsTask, originCodeTask, basketIdTask);

            return new CheckInInfo
            {
                Pnr = pnrTask.Result,
                TravelPartyId = travelPartyIdTask.Result,
                RequirementsId = requirementsTask.Result,
                OriginCode = originCodeTask.Result,
                BasketId = basketIdTask.Result
            };
        }

        public class CheckInInfo
        {
            public string Pnr { get; set; }
            public string TravelPartyId { get; set; }
            public string RequirementsId { get; set; }
            public string OriginCode { get; set; }
            public string? BasketId { get; set; }
        }

        public static async Task<Dictionary<string, object>> PaymentTransactionDetails(ConfirmBaggageRequest request, string paymentType)
        {
            if (request == null)
            {
                return new Dictionary<string, object>();
            }

            var checkInInfo = await GetCheckInInfoAsync(request.CheckInId, null);
            string passengerIds = await GetPassengerListJson(request.CheckInId);

            return new Dictionary<string, object>()
            {
                { "PnrLocator", checkInInfo.Pnr },
                { "OriginCode", checkInInfo.OriginCode },
                { "RequirementsId", checkInInfo.RequirementsId },
                { "PassengerIds", passengerIds },
                { "PaymentReferenceId", request.PaymentReferenceId },
                { "PaymentStatus", request.PaymentStatus },
                { "PaymentType",paymentType},
                {"BasketId", checkInInfo.BasketId }
            };
        }

        public static async Task<Dictionary<string, object>> BaggageReviewPayment(ConfirmBaggageResponse response, Basket basket, string checkInId)
        {
            if (response == null)
            {
                return new Dictionary<string, object>();
            }
            var checkInInfo = await GetCheckInInfoAsync(checkInId, null);
            string passengerIds = await GetPassengerListJson(checkInId);

            var baggagesByPassenger = GetBaggageCountsByPassengerNiN(response.PassengerBags, basket);

            return new Dictionary<string, object>()
            {
                { "PnrLocator", checkInInfo.Pnr },
                { "PassengerIds", passengerIds },
                { "OriginAirportCode", response.OriginAirportCode },
                { "OriginAirportName", response.OriginAirportName },
                { "DestinationAirportCode", response.DestinationAirportCode },
                { "DestinationAirportName", response.DestinationAirportName },
                { "TotalPaidAmount", response.TotalPaidAmount },
                { "CurrencyCode", response.CurrencyCode },
                { "CurrencySymbol", response.CurrencySymbol },
                { "DecimalPrecisionCount", response.DecimalPrecisionCount },
                { "NumOfBags", baggagesByPassenger }
            };
        }

        private static string GetBaggageCountsByPassengerNiN(IEnumerable<PassengerBag> passengerBags, Basket basket)
        {

            var baggagesByPassenger = passengerBags
                .Select(pb =>
                {
                    var basketPassengerBaggage = basket.CheckedBaggageDetails.PassengerBaggage
                        .Where(bpb => bpb.FirstName.Is(pb.GivenNames) && bpb.LastName.Is(pb.Surname))
                        .ToList();

                    // If basket details are missing due to caching, FirstOrDefault may return null, so we use a null-conditional when accessing PassengerNiN.
                    var passengerNiN = basketPassengerBaggage.FirstOrDefault()?.PassengerNiN;

                    return new
                    {
                        PassengerNiN = passengerNiN,
                        PaidBagsCount = pb.NumberOfBags,
                        FreeBagsCount = basketPassengerBaggage.Sum(b => b.NumOfFreeBags ?? 0),
                        TotalBagsCount = pb.NumberOfBags + basketPassengerBaggage.Sum(b => b.NumOfFreeBags ?? 0),
                        TotalPaidAmount = pb.TotalPaidAmount,
                        CurrencyCode = pb.CurrencyCode
                    };
                })
                .ToList();

            return JsonConvert.SerializeObject(baggagesByPassenger);
        }

        public static Dictionary<string, object> GetTravelRequirementsSessionCreated( Infrastructure.Models.TravelRequirements.TravelRequirementsServiceResponse response)
        {
            var passengers = response?.Passengers ?? [];
            var slices = response?.Slices != null ? response.Slices : [];

            return new()
            {
                { "PnrLocator", response?.Reference?.PnrLocator },
                { "RequirementsId", response?.RequirementsId },
                { "PassengerIds", Json.Serialize(passengers.Select(p => p.PassengerId)) },
                { "PassengerSliceRequirements", Json.Serialize(passengers.Select(TravelRequirements)) },
                { "SliceDetails", Json.Serialize(slices.Select(GetSliceDetails)) }
            };
        }

        private static Dictionary<string, object> GetSliceDetails(Infrastructure.Models.CheckInMS.Slice slice) =>
        new()
        {
            { "SliceId", slice?.SliceId },
            { "OriginCode",  slice?.Origin?.Code},
            { "DestinationCode",  slice?.Destination?.Code},
        };

        private static Dictionary<string, object> TravelRequirements(Infrastructure.Models.CheckInMS.Passenger passenger) =>
        new()
        {
            { "PassengerId", passenger?.PassengerId },
            { "SliceRequirements", Json.Serialize(passenger?.SliceRequirements?.Select(GetSliceRequirements) ?? []) }

        };

        private static Dictionary<string, object> GetSliceRequirements(Infrastructure.Models.CheckInMS.CheckInRequirements requirements) =>
        new()
        {
            { "SliceId", requirements?.SliceId },
            { "PassportRequired", requirements?.PassportRequired },
            { "PassportProvided", requirements?.PassportProvided },
            { "PassportVerified", requirements?.PassportVerified },
            { "ContactTracingStatus", requirements?.ContactTracing.ToString() },
            { "ContactTracingAvailableFromUtc", requirements?.ContactTracingAvailableFromUtc },
            { "ContactTracingAvailableFromDescription", requirements?.ContactTracingAvailableFromDescription },
            { "CheckInEligible", requirements?.CheckinEligible },
            { "ReturnDateStatus", requirements?.ReturnDateStatus.ToString() },
            { "ReturnDateDeclined", requirements?.IsReturnDateDeclined },
            { "ReturnDate", requirements?.ReturnDate.HasValue == true ? "PROVIDED" : null },
            { "SecondaryDocumentStatus", requirements?.SecondaryDocumentStatus.ToString() },
            { "SecondaryDocuments", Json.Serialize(requirements?.SecondaryDocuments?.Select(SecondaryDocument) ?? []) },
            { "GovernmentForms", Json.Serialize(requirements?.GovernmentForms == null
                ? [new { Status = "NOT_NEEDED", IsAvailable = false }]
                : requirements.GovernmentForms.Select(form => new { Status = form.Status.ToString(), form.IsAvailable })) },
            { "PassportErrors", Json.Serialize(requirements?.PassportErrors?.Select(e => e.Code) ?? []) },
            { "TravelRequirementErrors", Json.Serialize(requirements?.TravelRequirementErrors?.Select(e => e.Code) ?? []) },
            { "UnhappyPassport", requirements?.UnhappyPassport }
        };

        private static Dictionary<string, object> SecondaryDocument(Infrastructure.Models.CheckInMS.SecondaryDocumentRequirement document) =>
        new()
        {
            { "DocumentType", document?.DocumentType.ToString() },
            { "CountryCode", document?.CountryCode },
            { "Name", document?.Name },
            { "Status", document?.Status.ToString() },
            { "VendorUrlPresent", document?.SecondaryDocumentApplicationOptions?.Any(application =>
                application.ApplicationType == ApplicationType.Vendor &&
                !application.Url.IsNullOrWhiteSpace()) == true },
            { "GovernmentUrlPresent", document?.SecondaryDocumentApplicationOptions?.Any(application =>
                application.ApplicationType == ApplicationType.Government &&
                !application.Url.IsNullOrWhiteSpace()) == true }
        };

        public static Dictionary<string, object> TravelRequirementsFailed(string pnrLocator, LoggingFailureContext failureContext, ErrorDetails? errors)
        {
            if (string.IsNullOrWhiteSpace(pnrLocator))
            {
                return [];
            }

            return new Dictionary<string, object>()
            {
                { "PnrLocator", pnrLocator },
                { "Status", "FAILED" },
                { "TravelRequirementsStage", failureContext },
                { "PassportTRMError", errors is null ? null : Json.Serialize(errors)  }
            };
        }

        public static async Task<Dictionary<string, object>> TravelRequirementsPassportDetailsSubmitted(Models.TravelRequirements.TravelRequirementsSavePassportRequest request)
        {
            var travelRequirementsDetails = await GetCheckInInfoAsync(request.TravelRequirementsId, null);

            return new Dictionary<string, object>()
            {
                { "PnrLocator", travelRequirementsDetails.Pnr },
                { "RequirementsId", travelRequirementsDetails.RequirementsId },
                { "PassengerId",  request?.PassengerId },
                { "IssuingCountry", request?.Document?.IssuingCountry?.Code },
                { "Scanned", request?.IsScanned },
                { "Nationality", request?.Document?.Nationality?.Code}
            };
        }

        public static Dictionary<string, object> TravelRequirementsPassportDetailsSaved(Infrastructure.Models.TravelRequirements.TravelRequirementsServiceResponse response, string passengerId)
        {
            var slices = response?.Slices != null ? response.Slices : [];

            return new Dictionary<string, object>()
            {
                { "PnrLocator", response?.Reference?.PnrLocator },
                { "RequirementsId", response?.RequirementsId },
                { "PassengerId",  passengerId },
                { "PassengerSliceRequirements", Json.Serialize(TravelRequirements(response.Passengers?.FirstOrDefault(p => p.PassengerId.Is(passengerId)))) },
                { "SliceDetails", Json.Serialize(slices.Select(GetSliceDetails)) }
            };
        }

        public static async Task<Dictionary<string, object>> TravelRequirementsReturnDateSubmitted(Models.TravelRequirements.TravelRequirementsSaveReturnDateRequest request)
        {
            var travelRequirementsDetails = await GetCheckInInfoAsync(request.TravelRequirementsId, null);

            return new Dictionary<string, object>()
            {
                { "PnrLocator", travelRequirementsDetails.Pnr },
                { "RequirementsId", travelRequirementsDetails.RequirementsId },
                { "PassengerId", request?.PassengerId },
                { "ReturnOn", request?.ReturnOn },
                { "NoReturnDate", request?.NoReturnDate }
            };
        }

        public static Dictionary<string, object> TravelRequirementsReturnDateSaved(Infrastructure.Models.TravelRequirements.TravelRequirementsServiceResponse response, string passengerId)
        {
            var slices = response?.Slices != null ? response.Slices : [];

            return new Dictionary<string, object>()
            {
                { "PnrLocator", response?.Reference?.PnrLocator },
                { "RequirementsId", response?.RequirementsId },
                { "PassengerId", passengerId },
                { "PassengerSliceRequirements", Json.Serialize(TravelRequirements(response.Passengers?.FirstOrDefault(p => p.PassengerId.Is(passengerId)))) },
                { "SliceDetails", Json.Serialize(slices.Select(GetSliceDetails)) }
            };
        }

        public static async Task<Dictionary<string, object>> TravelRequirementsContactTracingSubmitted(string travelRequirementsId, IEnumerable<ContactTracing> contactTracing)
        {
            var travelRequirementsDetails = await GetCheckInInfoAsync(travelRequirementsId, null);
            var contactTracingList = contactTracing?.ToList();

            return new Dictionary<string, object>()
            {
                { "PnrLocator", travelRequirementsDetails.Pnr },
                { "RequirementsId", travelRequirementsDetails.RequirementsId },
                { "Passengers", Json.Serialize(contactTracingList?.Select(x => new Dictionary<string, object>
                    {
                        { "PassengerId", x.PassengerId },
                        { "InTransit", x.InTransit },
                        { "NoEmail", x.NoEmail },
                        { "NoSecondaryPhoneNumber", x.NoSecondaryPhoneNumber }
                    }).ToList()) }
            };
        }

        public static Dictionary<string, object> TravelRequirementsContactTracingSaved(Infrastructure.Models.TravelRequirements.TravelRequirementsServiceResponse response, IEnumerable<string> passengerIds)
        {
            var slices = response?.Slices != null ? response.Slices : [];
            var passengerIdList = passengerIds?.ToList();

            return new Dictionary<string, object>()
            {
                { "PnrLocator", response?.Reference?.PnrLocator },
                { "RequirementsId", response?.RequirementsId },
                { "PassengerSliceRequirements", Json.Serialize(response?.Passengers?.Where(p => passengerIdList?.Any(passengerId => p.PassengerId.Is(passengerId)) == true).Select(TravelRequirements).ToList()) },
                { "SliceDetails", Json.Serialize(slices.Select(GetSliceDetails)) }
            };
        }

        public static async Task<Dictionary<string, object>> TravelRequirementsEmergencyContactSubmitted(string travelRequirementsId, bool applyToAllPassengers, IEnumerable<EmergencyContact> emergencyContacts)
        {
            var travelRequirementsDetails = await GetCheckInInfoAsync(travelRequirementsId, null);

            return new Dictionary<string, object>()
            {
                { "PnrLocator", travelRequirementsDetails.Pnr },
                { "RequirementsId", travelRequirementsDetails.RequirementsId },
                { "ApplyToAllPassengers", applyToAllPassengers },
                { "PassengerIds", Json.Serialize(emergencyContacts?.Select(contact => contact.PassengerId).ToList() ?? []) }
            };
        }

        public static Dictionary<string, object> TravelRequirementsEmergencyContactSaved(Infrastructure.Models.TravelRequirements.TravelRequirementsServiceResponse response, IEnumerable<EmergencyContact> emergencyContacts)
        {
            var passengerIdList = emergencyContacts?
                .Select(contact => contact.PassengerId)
                .Where(passengerId => !passengerId.IsNullOrWhiteSpace())
                .Distinct()
                .ToList() ?? [];
            var emergencyContactPresent = passengerIdList.Any() && passengerIdList.All(passengerId =>
                response?.Passengers?.Any(passenger =>
                    passenger.PassengerId.Is(passengerId) &&
                    passenger.EmergencyContact != null &&
                    !passenger.EmergencyContact.FirstName.IsNullOrWhiteSpace() &&
                    !passenger.EmergencyContact.LastName.IsNullOrWhiteSpace() &&
                    !passenger.EmergencyContact.PhoneNumber.IsNullOrWhiteSpace()) == true);

            return new Dictionary<string, object>()
            {
                { "PnrLocator", response?.Reference?.PnrLocator },
                { "RequirementsId", response?.RequirementsId },
                { "EmergencyContactPresent", emergencyContactPresent },
                { "ApplyToAllPassengers", response?.ApplyEmergencyContactToAllPax }
            };
        }

        public static async Task<Dictionary<string, object>> TravelRequirementsGovernmentFormSubmitted(string travelRequirementsId, string passengerId, string sliceId) =>
            await TravelRequirementsSubmitted(travelRequirementsId, passengerId, sliceId);

        public static Dictionary<string, object> TravelRequirementsGovernmentFormSaved(Infrastructure.Models.TravelRequirements.TravelRequirementsServiceResponse response, string passengerId, string sliceId) =>
            TravelRequirementsSaved(response, [passengerId], sliceId);

        public static async Task<Dictionary<string, object>> TravelRequirementsSecondaryDocumentSubmitted(string travelRequirementsId, string passengerId, string sliceId) =>
            await TravelRequirementsSubmitted(travelRequirementsId, passengerId, sliceId);

        public static Dictionary<string, object> TravelRequirementsSecondaryDocumentSaved(Infrastructure.Models.TravelRequirements.TravelRequirementsServiceResponse response, string passengerId, string sliceId) =>
            TravelRequirementsSaved(response, [passengerId], sliceId);

        private static async Task<Dictionary<string, object>> TravelRequirementsSubmitted(string travelRequirementsId, string passengerId, string sliceId)
        {
            var travelRequirementsDetails = await GetCheckInInfoAsync(travelRequirementsId, null);

            return new Dictionary<string, object>()
            {
                { "PnrLocator", travelRequirementsDetails.Pnr },
                { "RequirementsId", travelRequirementsDetails.RequirementsId },
                { "PassengerId", passengerId },
                { "SliceId", sliceId }
            };
        }

        private static Dictionary<string, object> TravelRequirementsSaved(Infrastructure.Models.TravelRequirements.TravelRequirementsServiceResponse response, IEnumerable<string> passengerIds, string? sliceId = null)
        {
            var passengerIdList = passengerIds?.ToList();
            var data = new Dictionary<string, object>()
            {
                { "PnrLocator", response?.Reference?.PnrLocator },
                { "RequirementsId", response?.RequirementsId },
                { "PassengerIds", Json.Serialize(passengerIdList ?? []) },
                { "PassengerSliceRequirements", Json.Serialize(response?.Passengers?.Where(passenger => passengerIdList?.Any(passengerId => passenger.PassengerId.Is(passengerId)) == true).Select(TravelRequirements).ToList()) }
            };

            if (!sliceId.IsNullOrWhiteSpace())
            {
                data.Add("SliceId", sliceId);
            }

            return data;
        }
    }
}
