using System.Text.RegularExpressions;

namespace VAA.EAPI.GraphQL.Domain.Operations.Models.PassengerManifest
{
    public class PassengerManifestResponse
    {
        private static Regex chldDob = new("(?'dob'[0-3][0-9][A-Z]{3}[0-9]{2})", RegexOptions.Compiled);

        public DateTime Generated { get; set; } = DateTime.UtcNow;
        public FlightDetails FlightDetails { get; set; }

        [UseSorting]
        [UseFiltering]
        public ICollection<Passenger> Passengers { get; set; } = new List<Passenger>();



        /// <summary>
        /// Casting method to convert from SCV v7 Manifiest API response to GraphQL
        /// </summary>
        /// <param name="source"></param>
        public static explicit operator PassengerManifestResponse(DTOs.PassengerManifest_SCVv7.SCVv7_API_Response source)
        {
            PassengerManifestResponse manifest = new()
            {
                FlightDetails = new()
                {
                    NumberOfStops = source.FlightDetails.LegDetails.Length,
                    InitialOrigin = source.FlightDetails.LegDetails.OrderBy(o => o.LegSequenceNumber).First().OriginAirportCode,
                    FinalDestination = source.FlightDetails.LegDetails.OrderBy(o => o.LegSequenceNumber).Last().DestinationAirportCode,
                }
            };

            // Map the flight legs
            Array.ForEach(source.FlightDetails.LegDetails, l =>
            {
                manifest.FlightDetails.Legs.Add(new()
                {
                    LegNumber = int.Parse(l.LegSequenceNumber),
                    OperatingCarrierCode = l.OperatingCarrierCode,
                    OperatingFlightNumber = int.Parse(l.OperatingFlightNumber),
                    OriginAirportCode = l.OriginAirportCode,
                    ScheduleDepartureTime = CreateFlightMovementTime(l.UTCScheduledDepartureTime, l.LocalScheduledDepartureTime),
                    DestinationAirportCode = l.DestinationAirportCode,
                    ScheduleArrivalTime = CreateFlightMovementTime(l.UTCScheduledArrivalTime, l.LocalScheduledArrivalTime),
                });
            });

            // Map Customers
            foreach (var c in source.FlightDetails.CustomerDetails)
            {
                DTOs.PassengerManifest_SCVv7.Order o = c.Order.First();

                Passenger p = new()
                {
                    PNRRefNumber = o.PNRRefNumber,
                    PNRCreationDate = new DateTime(o.PNRCreationDate.Date.Ticks, DateTimeKind.Utc),
                    PaxNumber = c.PaxNumber,
                    Title = c.Title,
                    FirstName = c.FirstName,
                    MiddleName = c.MiddleName,
                    LastName = c.LastName,
                    DateOfBirth = string.IsNullOrWhiteSpace(c.DateOfBirth) ? null : new DateTime(DateTime.Parse(c.DateOfBirth).Date.Ticks, DateTimeKind.Utc),
                    ChildAge = c.ChildAge,
                    Gender = c.Gender,
                    Origin = c.SegmentOriginAirportCode,
                    Destination = c.SegmentDestinationAirportCode,
                    Seat = (Seat)(c.Order.FirstOrDefault()?.OrderItems?.FirstOrDefault()?.Seats?.FirstOrDefault()),
                    BookedCabin = c.BookedRevenueCabin,
                    BookedClass = c.BookedRevenueClassOfService,
                    BookingStatus = c.SegmentCurrentActionCode,
                    StaffDetails = (StaffDetails)c.StaffDetails?.FirstOrDefault(),
                    Profile = new()
                    {
                        isVHols = c.VholsCustomer == "Y",
                        isCrewPosition = c.CrewPositioning == "Y",
                        isStaffBooking = c.StaffCustomer == "Y",
                        isCorp = c.CorporateCustomer == "Y",
                        isNonRev = c.NonRevenue == "Y"
                    }
                };

                // Check for CHLD
                if (!p.DateOfBirth.HasValue)
                {
                    try
                    {
                        var chldService = c.Order?.FirstOrDefault()?
                                           .OrderItems?.FirstOrDefault()?
                                            .Services?.FirstOrDefault(s => s.ServiceCode.Equals("CHLD"));
                        if (!string.IsNullOrEmpty(chldService?.Remarks))
                        {
                            p.DateOfBirth = DateTime.ParseExact(chldDob.Match(chldService.Remarks).Groups["dob"].Value, "ddMMMyy", System.Globalization.CultureInfo.CurrentCulture);
                        }
                    }
                    catch (FormatException)
                    {
                        // If the date is not valid, then leave as NULL                                          
                    }
                }


                // Loyalty details
                if (!string.IsNullOrWhiteSpace(c.LoyaltyNumber))
                {
                    p.LoyaltyDetails = new()
                    {
                        LoyaltyAirline = c.LoyaltyAirline,
                        LoyaltyNumber = c.LoyaltyNumber,
                        LoyaltyTierCode = c.LoyaltyTierCode,
                    };
                }

                if (!string.IsNullOrEmpty(c.LapInfantName))
                {
                    p.LapInfant = new()
                    {
                        Name = c.LapInfantName,
                        PaxNumber = c.LapInfantIndexNumber
                    };
                }

                foreach (var item in c.Order.First().OrderItems?.FirstOrDefault().Services)
                {
                    p.Services.Add((Service)item);
                }
                manifest.Passengers.Add(p);
            }
            ;
            return manifest;
        }
        private static FlightTime CreateFlightMovementTime(DateTime? UTCTime, DateTime? localTime)
        {
            if (localTime.GetValueOrDefault() > DateTime.MinValue && UTCTime.GetValueOrDefault() > DateTime.MinValue)
            {
                TimeSpan tzOffset = localTime.Value - UTCTime.Value;

                return new()
                {
                    UTC = new DateTimeOffset(UTCTime.GetValueOrDefault(), TimeSpan.FromTicks(0)),
                    Local = new DateTimeOffset(localTime.GetValueOrDefault(), tzOffset)
                };
            }

            return null;
        }
    }
}
