
namespace VAA.EAPI.Microservices.Retail.Trip.Domain.MessageDetails
{
    public class Station
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public static explicit operator Station(Infrastructure.Proxy.DeltaMessageCentre.Station source)
        {
            if (source == null)
                return null;

            return new Station()
            {
                Code = source.Code,
                Name = source.Name
            };
        }
    }
}
