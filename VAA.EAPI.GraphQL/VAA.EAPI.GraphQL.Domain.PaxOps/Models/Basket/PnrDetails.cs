namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.Basket
{
    public class PnrDetails
    {
        public string PnrLocator { get; set; }
        public IEnumerable<PassengerDetail> Passenger { get; set; }
        public IEnumerable<Slice>? Slices { get; set; }

        public static explicit operator PnrDetails(Infrastructure.Models.CheckInMS.CheckInResponse response)
        {
            return new PnrDetails()
            {
                PnrLocator = response.Reference.PnrLocator,
                Passenger = response.Passengers.Select(p => new PassengerDetail
                {
                    FirstName = p?.GivenNames,
                    LastName = p?.Surname,
                    DateOfBirth = p?.DateOfBirth?.ToDateTimeUnspecified(),
                    MembershipDetails = new FQTMembership
                    {
                        FQTNumber = p?.LoyaltyMembership?.Number,
                        FQTTierCode = p?.LoyaltyMembership?.ProgrammeCode
                    },
                    BookingClass = new BookingClass
                    {
                        Code = "X" // Booking class is not provided in the check-in response, so using a placeholder value. This can be updated when the actual value is available.
                    }
                }),
                Slices = new List<Slice>
                {
                    new()
                    {
                        Id = response.CurrentSlice?.SliceId,
                        ScheduledDepartureLocalDateTime = response.CurrentSlice.ScheduledDepartureLocalDateTime,
                        ScheduledArrivalLocalDateTime = response.CurrentSlice.ScheduledArrivalLocalDateTime,
                        Origin = response.CurrentSlice?.Origin,
                        Destination = response.CurrentSlice?.Destination,
                        Segments = response.CurrentSlice?.Segments.Select(s => new Segment
                        {
                            Id = s.SegmentId,
                            OperatingCarrier = (CarrierDetails)s?.OperatingCarrier,
                            MarketingCarrier = (CarrierDetails)s?.MarketingCarrier,
                            ScheduledDepartureLocalDateTime = s.ScheduledDepartureLocalDateTime.ToDateTimeUnspecified(),
                            ScheduledArrivalLocalDateTime = s.ScheduledArrivalLocalDateTime.ToDateTimeUnspecified(),
                            Origin = s?.Origin,
                            Destination = s?.Destination,
                            Legs = s.Legs.Select(l => new Leg
                            {
                                Id = l.LegId,
                                ScheduledDepartureLocalDateTime = l.ScheduledDepartureLocalDateTime,
                                ScheduledArrivalLocalDateTime = l.ScheduledArrivalLocalDateTime,
                                Origin = l.Origin,
                                Destination = l.Destination
                            })
                        })
                    }
                }
            };
        }
    }
}
