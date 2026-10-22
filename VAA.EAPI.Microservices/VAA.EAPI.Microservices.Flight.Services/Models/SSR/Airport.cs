using VAA.EAPI.Microservices.Flight.Services.Infrastructure.Delta.SSR;

namespace VAA.EAPI.Microservices.Flight.Services.Models.SSR
{
    public class Airport
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public static explicit operator Airport(Station source)
        {
            if (source == null)
                return null;

            return new Airport
            {
                Code = source.Code,
                Name = source.Name
            };
        }
    }
}
