
namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo
{
    public class PassengerType
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public PassengerAgeType Category { get; set; }

        public static explicit operator PassengerType(Domain.PassengerDetails.PassengerType source)
        {
            if (source is null) return null;

            return new PassengerType()
            {
                Id = source.Id,
                Code = source.Code,
                Name = source.Name,
                Category = (PassengerAgeType)source.Category
            };
        }
    }
}