using NodaTime;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays
{
    public class TravellerInformation
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CustomerType { get; set; }
        public LocalDate? DateOfBirth { get; set; }
        public bool? LeadTraveller { get; set; }

        public static explicit operator TravellerInformation(Infrastructure.Models.HolidaysMS.TravellerParty source)
        {
            if (source == null)
                return null;

            return new()
            {
                FirstName = source.FirstName,
                LastName = source.LastName,
                CustomerType = source.TravellerType,
                DateOfBirth = source.DateOfBirth.HasValue ? LocalDate.FromDateTime(source.DateOfBirth.Value) : null,
                LeadTraveller = source.LeadTraveller
            };
        }
    }
}
