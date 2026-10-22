
namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin
{
    public class PassengerType
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Category { get; set; }

        public static explicit operator PassengerType(Infrastructure.Models.TripMS.PassengerType source)
        {
            if (source == null)
                return null;

            return new PassengerType()
            {
                Code = source.Code,
                Name = source.Name,
                Category = source.Category
            };
        }
    }
}
