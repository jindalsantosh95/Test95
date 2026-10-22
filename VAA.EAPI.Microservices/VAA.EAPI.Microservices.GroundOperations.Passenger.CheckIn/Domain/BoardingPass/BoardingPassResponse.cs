using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Constants;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.DeltaSsci;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.BoardingPass
{
    public class BoardingPassResponse
    {
        public IEnumerable<BoardingPass> BoardingPasses { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public PassengersCheckInStatus CheckInStatus { get; set; }

        public static explicit operator BoardingPassResponse((TravelParty travelPartyResponse, BoardingDocumentsResponse? passResponse) source)
        {
            var travelPartyResponse = source.travelPartyResponse;
            var passResponse = source.passResponse;

            var bardingPasses = GetPassengerFlightDetails(travelPartyResponse, passResponse);

            return new BoardingPassResponse
            {
                BoardingPasses = bardingPasses,
                CheckInStatus = GetPnrStatus(bardingPasses, travelPartyResponse)
            };
        }

        public static explicit operator BoardingPassResponse(PassengersCheckInStatus status)
        {
            return new BoardingPassResponse
            {
                CheckInStatus = status
            };
        }

        private static PassengersCheckInStatus GetPnrStatus(IEnumerable<BoardingPass> boardingPasses, TravelParty travelPartyResponse)
        {
            if (travelPartyResponse?.CheckinAdvisory != null)
                return PassengersCheckInStatus.NOT_AVAILABLE;
            else if (boardingPasses.All(x => x.CheckedIn))
                return PassengersCheckInStatus.COMPLETED;
            else if (boardingPasses.All(x => x.CheckedIn == false))
                return PassengersCheckInStatus.TO_COMPLETE;
            else
                return PassengersCheckInStatus.PARTIALLY_COMPLETE;
        }

        private static IEnumerable<BoardingPass> GetPassengerFlightDetails(TravelParty travelPartyResponse, BoardingDocumentsResponse boardingPassResponse)
        {
            List<BoardingPass> boardingPasses = [];
            var boardingPassArray = boardingPassResponse?.BoardingDocuments?.Where(x => !x.MobileBase64.IsNullOrEmpty()).ToArray();

            var checkInFailed = boardingPassResponse?.CheckinFailed ?? false;

            if (boardingPassResponse?.AdditionalProperties != null &&
                boardingPassResponse.AdditionalProperties.TryGetValue("checkinFailed", out var value) &&
                value is bool b)
            {
                checkInFailed = b;
            }

            if (travelPartyResponse.Trip.Segments.Any() && travelPartyResponse.Passengers.Any())
            {
                var passengerIteration = 0;
                foreach (DeltaSsci.Passenger1 passenger in travelPartyResponse.Passengers)
                {
                    var segmentsGroups = travelPartyResponse.Trip?.Segments
                        .GroupBy(leg => leg.AdditionalProperties["segmentID"].ToString()).ToList();

                    foreach (var seg in segmentsGroups)
                    {
                        var firstleg = seg.First();
                        var lastleg = seg.Last();
                        string cabinName = passenger.Seats.FirstOrDefault(s => s.SegmentNumber == seg.Key)?.Cabin ?? "";
                        var segmentBoardingPasses = boardingPassArray?.Where(bpa => bpa.SegmentNumber == int.Parse(seg.Key ?? string.Empty)).ToArray();
                        var matchingBoardingDoc = (segmentBoardingPasses != null && passengerIteration < segmentBoardingPasses.Length) ? segmentBoardingPasses[passengerIteration] : null;                     
                        var Infantpax = travelPartyResponse.Passengers.FirstOrDefault(x => x.AssociatedAdultFirstNin.Is(passenger.FirstNIN) && x.AssociatedAdultLastNin.Is(passenger.LastNIN));
                        boardingPasses.Add(new BoardingPass
                        {
                            PnrLocator = travelPartyResponse.RecordLocator,
                            GivenNames = passenger?.FirstName,
                            Surname = passenger?.LastName,
                            PassengerNIN = $"{passenger.FirstNIN}.{passenger.LastNIN}",
                            PassengerCustomerId = $"{passenger.CustomerId}",
                            PassengerId = $"{passenger.Id}",
                            SeatNumber = passenger.InfantPresent ? "INF"
                                        : !string.IsNullOrEmpty(matchingBoardingDoc?.SeatNumber)
                                            ? matchingBoardingDoc.SeatNumber
                                            : passenger?.Seats?.FirstOrDefault(s => s.SegmentNumber.Is(firstleg.AdditionalProperties["segmentID"].ToString()))?.Number,
                            Terminal = firstleg.Terminal,
                            Zone = matchingBoardingDoc?.BoardingGroup,
                            OperatingAirlineName = firstleg.OperatingAirlineLongName,
                            OperatingFlightNumber = $"{firstleg.OperatingAirlineCode}{int.Parse(firstleg.OperatingFlightNumber)}",
                            MarketingAirlineName = firstleg.MarketingAirlineCode,
                            MarketingFlightNumber = $"{firstleg.MarketingAirlineCode}{int.Parse(firstleg.MarketingFlightNumber)}",
                            OriginAirportCode = firstleg.OriginCode,
                            DestinationAirportCode = lastleg.DestinationCode,
                            DepartureDateTime = DateTime.ParseExact($"{firstleg.DepartureDate} {firstleg.DepartureTime}", "dd MMM yyyy hh:mmtt", System.Globalization.CultureInfo.InvariantCulture),
                            VerificationStatusMessage = matchingBoardingDoc?.TravelAdvisory,
                            QrCode = matchingBoardingDoc?.MobileBase64,
                            QrCodeData = (matchingBoardingDoc?.MobileBase64 ?? string.Empty).ExtractQRData(),
                            DesktopBoardingPassBase64 = matchingBoardingDoc?.DesktopBase64,
                            CabinName = cabinName.Replace("&#174;", string.Empty),
                            Gate = matchingBoardingDoc?.BoardingGate,
                            BoardingTimeLocalTime = DateTimeHelper.Air4PNRLocalTimeConversion(matchingBoardingDoc?.BoardingDepartureTime),
                            ShowSkyPriority = matchingBoardingDoc?.SkyPriority ?? false,
                            ShowTsaPre = matchingBoardingDoc?.TsaPreCheck ?? false,
                            FlightStatusCode = GetFlightStatusCode(matchingBoardingDoc?.FlightStatus, travelPartyResponse.IropData, firstleg.Status),
                            InfantName = $"{Infantpax?.FirstName} {Infantpax?.LastName}",
                            TemplateCode = GetTemplateCode(firstleg.Cabin, firstleg.OperatingAirlineCode),
                            CheckedIn = string.IsNullOrEmpty(matchingBoardingDoc?.MobileBase64) ? false : !checkInFailed
                        });
                    }

                    passengerIteration++;
                }
            }
            return boardingPasses;
        }


        private static TemplateCode GetTemplateCode(string cabin, string operatingAirlineCode)
        {
            operatingAirlineCode = operatingAirlineCode?.Trim().ToUpperInvariant();

            return (operatingAirlineCode, cabin) switch
            {
                ("VS", "Business") => TemplateCode.VS_UPPER,
                ("VS", "Upper Class") => TemplateCode.VS_UPPER,
                ("VS", "Upper") => TemplateCode.VS_UPPER,
                ("VS", "Premium Economy") => TemplateCode.VS_PREMIUM,
                ("VS", "Premium") => TemplateCode.VS_PREMIUM,
                ("VS", _) => TemplateCode.VS_ECONOMY,
                ("DL", _) => TemplateCode.DL,
                _ => TemplateCode.OA
            };
        }

        private static FlightStatusCode GetFlightStatusCode(string boardingDocumentFlightStatus, IropData ssciIropData, string ssciSegmentFlightStatus)
        {
            switch (boardingDocumentFlightStatus)
            {
                case "On-Time": return FlightStatusCode.ON_TIME;
                case "Delayed": return FlightStatusCode.DELAYED;
                case "Cancelled": return FlightStatusCode.CANCELLED;
                default:
                    if (ssciIropData?.CancelledFlag == true) return FlightStatusCode.CANCELLED;
                    if (ssciIropData?.DelayedFlag == true) return FlightStatusCode.DELAYED;
                    switch (ssciSegmentFlightStatus)
                    {
                        case "On time": return FlightStatusCode.ON_TIME;
                        case "Delayed": return FlightStatusCode.DELAYED;
                        case "Cancelled": return FlightStatusCode.CANCELLED;
                        default: return FlightStatusCode.UNKNOWN;
                    }
            }
        }
    }
}
