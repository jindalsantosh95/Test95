using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.Trips;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.BoardingPass;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.Enums;
using TripMS = VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.Trips;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Helper
{
    public static class TripsHelper
    {
        private const string CheckedInStatus = "CHECKED_IN";

        public static HashSet<TripMS.Passenger> GetCheckedInPassengers(TripMS.Booking booking, string originCode)
        {
            var sliceIdsForOrigin = GetSliceIdsForOrigin(booking, originCode);

            return booking.Passengers
                .Select(p => new
                {
                    Passenger = p,
                    CheckedInSlices = p.Slices?
                        .Where(s => sliceIdsForOrigin.Contains(s.Id))
                        .Select(s =>
                        {
                            var checkedInSegments = s.Segments?
                                .Where(seg =>
                                    string.Equals(
                                        seg.CheckinStatus,
                                        CheckedInStatus,
                                        StringComparison.OrdinalIgnoreCase))
                                .ToList();

                            if (checkedInSegments == null || !checkedInSegments.Any())
                                return null;

                            s.Segments = checkedInSegments;
                            return s;
                        })
                        .Where(s => s != null)
                        .ToList()
                })
                .Where(x => x.CheckedInSlices?.Any() == true)
                .Select(x =>
                {
                    var clonedPassenger = x.Passenger;
                    clonedPassenger.Slices = x.CheckedInSlices!;
                    return clonedPassenger;
                })
                .ToHashSet();
        }


        public static BoardingPassResponse UpdateBoardingPassData(TripMS.Booking tripResponse, BoardingPassResponse? bpResponse, HashSet<TripMS.Passenger>? checkedInPassengers, string origin)
        {
            checkedInPassengers ??= new HashSet<TripMS.Passenger>();
            var bpResponseTrips = BuildBoardingPassResponseFromTrips(tripResponse, checkedInPassengers, origin);

            if (checkedInPassengers.Any())
            {
                var bpSegmentLookup = BuildBoardingPassSegmentLookup(bpResponse);

                foreach (var bp in bpResponseTrips.BoardingPasses)
                {
                    var key = new BoardingSegmentKey(
                        bp.PassengerCustomerId,
                        bp.OriginAirportCode,
                        bp.DestinationAirportCode,
                        bp.OperatingFlightNumber,
                        bp.DepartureLocal
                    );

                    if (!bpSegmentLookup.TryGetValue(key, out var bpData))
                        continue;

                    if (bpData?.CheckedIn == true)
                    {
                        bp.Zone = bpData.Zone;
                        bp.QrCode = bpData.QrCode;
                        bp.QrCodeData = bpData.QrCodeData;
                        bp.DesktopBoardingPassBase64 = bpData.DesktopBoardingPassBase64;
                        bp.VerificationStatusMessage = bpData.VerificationStatusMessage;
                        bp.Gate = bpData.Gate;
                        bp.BoardingTimeLocalTime = bpData.BoardingTimeLocalTime;
                        bp.ShowSkyPriority = bpData.ShowSkyPriority;
                        bp.ShowTsaPre = bpData.ShowTsaPre;
                    }
                }
            }

            return bpResponseTrips;
        }

        private static BoardingPassResponse BuildBoardingPassResponseFromTrips(TripMS.Booking tripResponse, HashSet<TripMS.Passenger> checkedInPassengers, string originCode)
        {
            var boardingPasses = new List<BoardingPass>();

            var sliceIdsForOrigin = GetSliceIdsForOrigin(tripResponse, originCode);
            var productSlicesById = tripResponse.Product.SelectMany(p => p.Slices).ToDictionary(s => s.Id);

            var pass = tripResponse.Passengers
                .Select(p => new
                {
                    Passenger = p,
                    CheckedInSlices = p.Slices?
                        .Where(s => sliceIdsForOrigin.Contains(s.Id))
                        .ToList()
                })
                .Where(x => x.CheckedInSlices?.Any() == true)
                .Select(x =>
                {
                    var clonedPassenger = x.Passenger;
                    clonedPassenger.Slices = x.CheckedInSlices!;
                    return clonedPassenger;
                });

            foreach (var passenger in pass ?? Enumerable.Empty<TripMS.Passenger>())
            {
                TripMS.Passenger infantPassenger = null;

                if (!string.IsNullOrEmpty(passenger.AssociatedInfantId))
                {
                    infantPassenger = pass?.FirstOrDefault(p => p.PassengerId == passenger.AssociatedInfantId);
                }

                var infantName = infantPassenger != null ? $"{infantPassenger.FirstName} {infantPassenger.LastName}" : " ";

                foreach (var paxSlice in passenger.Slices ?? Enumerable.Empty<TripMS.Slice>())
                {
                    if (!productSlicesById.TryGetValue(paxSlice.Id, out var productSlice))
                        continue;

                    foreach (var productSegment in productSlice.Segments ?? Enumerable.Empty<TripMS.Segment>())
                    {
                        var paxSegment = paxSlice.Segments?.FirstOrDefault(ps => ps.Id == productSegment.Id);
                        var departureLeg = productSegment.Legs?.FirstOrDefault(l => l.Origin?.Code == productSegment.Origin?.Code);
                        var isCheckedIn = string.Equals(paxSegment?.CheckinStatus, CheckedInStatus, StringComparison.OrdinalIgnoreCase);

                        var seatNumber = isCheckedIn ? paxSegment?.Legs?
                                .SelectMany(l => l.SeatAssignments ?? Enumerable.Empty<TripMS.SeatAssignment>())
                                .Select(sa => sa.Seat?.Name)
                                .FirstOrDefault() : null;

                        var bp = new BoardingPass
                        {
                            PnrLocator = tripResponse.Product.FirstOrDefault()?.Reference?.PnrLocator,
                            PassengerNIN = passenger.FirstNameNumber + "." + passenger.LastNameNumber,
                            PassengerCustomerId = passenger.CustomerId,
                            PassengerId = passenger.PassengerId,
                            GivenNames = passenger.FirstName,
                            Surname = passenger.LastName,
                            PassengerTypeCode = passenger.PassengerType?.Code,
                            OriginAirportCode = productSegment.Origin?.Code,
                            DestinationAirportCode = productSegment.Destination?.Code,
                            OperatingFlightNumber = $"{productSegment.OperatingCarrier.CarrierCode}{int.Parse(productSegment.OperatingCarrier.FlightNumber)}",
                            MarketingFlightNumber = $"{productSegment.MarketingCarrier.CarrierCode}{int.Parse(productSegment.MarketingCarrier.FlightNumber)}",
                            OperatingAirlineName = productSegment.OperatingCarrier?.CarrierName,
                            MarketingAirlineName = productSegment.MarketingCarrier?.CarrierName,
                            DepartureDateTime = productSegment.ScheduledDepartureLocalDateTime,
                            Terminal = productSegment.Legs?.FirstOrDefault()?.Origin?.TerminalName,
                            FlightStatusCode = productSegment.Legs?.FirstOrDefault()?.Status,
                            CheckedIn = isCheckedIn,
                            InfantName = infantName,
                            SeatNumber = passenger.PassengerType?.Code == "INF" ? "INF" : seatNumber,
                            CabinName = departureLeg.CabinClass.Name,
                            TemplateCode = GetTemplateCode(departureLeg.CabinClass.Name, departureLeg.Operating.CarrierCode),
                            QrCode = null,
                            QrCodeData = null,
                            DesktopBoardingPassBase64 = null,
                            TierCode = GetTierCode(passenger.LoyaltyMembership),
                            TierName = !string.IsNullOrEmpty(passenger.LoyaltyMembership?.TierName) ? passenger.LoyaltyMembership?.TierName : null,
                            OperationalStatus = productSegment.OperationalStatus == null ? null : (FlightSegmentStatus)productSegment.OperationalStatus
                        };

                        boardingPasses.Add(bp);
                    }
                }
            }

            var totalPassengers = tripResponse.Passengers?.Count() ?? 0;
            var checkedInCount = checkedInPassengers?.Count ?? 0;

            var status = checkedInCount == 0
                ? PassengersCheckInStatus.TO_COMPLETE
                : checkedInCount == totalPassengers
                    ? PassengersCheckInStatus.COMPLETED
                    : PassengersCheckInStatus.PARTIALLY_COMPLETE;

            return new BoardingPassResponse
            {
                BoardingPasses = boardingPasses,
                CheckInStatus = status
            };
        }

        public static LoyaltyTierCode? GetTierCode(LoyaltyMembership loyaltyMembership)
        {
            if (loyaltyMembership == null || loyaltyMembership.TierName.IsNullOrEmpty() || !loyaltyMembership.ProgrammeCode.Is(Common.Constants.Application.VSLoyaltyProgrammeCode))
                return null;

            if (loyaltyMembership.TierName.Is(Common.Constants.Application.GoldTier))
                return LoyaltyTierCode.VS_Gold;
            else if (loyaltyMembership.TierName.Is(Common.Constants.Application.SilverTier))
                return LoyaltyTierCode.VS_Silver;
            else if (loyaltyMembership.TierName.Is(Common.Constants.Application.RedTier))
                return LoyaltyTierCode.VS_Red;

            return null;
        }

        private static Dictionary<BoardingSegmentKey, BoardingPass> BuildBoardingPassSegmentLookup(BoardingPassResponse? bpResponse)
        {
            var lookup = new Dictionary<BoardingSegmentKey, BoardingPass>();

            foreach (var bp in bpResponse.BoardingPasses.Where(bp => bp.CheckedIn))
            {
                var key = new BoardingSegmentKey(
                    bp.PassengerCustomerId,
                    bp.OriginAirportCode,
                    bp.DestinationAirportCode,
                    bp.OperatingFlightNumber,
                    bp.DepartureLocal
                );

                lookup[key] = new BoardingPass
                {
                    Zone = bp.Zone,
                    QrCode = bp.QrCode,
                    QrCodeData = bp.QrCodeData,
                    DesktopBoardingPassBase64 = bp.DesktopBoardingPassBase64,
                    VerificationStatusMessage = bp.VerificationStatusMessage,
                    Gate = bp.Gate,
                    BoardingTimeLocalTime = bp.BoardingTimeLocalTime,
                    ShowSkyPriority = bp.ShowSkyPriority,
                    ShowTsaPre = bp.ShowTsaPre,
                    CheckedIn = bp.CheckedIn
                };
            }
            return lookup;
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

        private static HashSet<string> GetSliceIdsForOrigin(TripMS.Booking booking, string originCode)
        {
            var normalizedOriginCode = originCode?.ToUpperInvariant();

            return booking.Product
                .SelectMany(p => p.Slices)
                .Where(s => s.Origin?.Code == normalizedOriginCode)
                .Where(s => s.IropProtect?.Protect != true)
                .Select(s => s.Id)
                .ToHashSet();
        }
    }
}
