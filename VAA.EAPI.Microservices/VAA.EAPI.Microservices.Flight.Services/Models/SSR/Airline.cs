using VAA.EAPI.Microservices.Flight.Services.Infrastructure.Delta.SSR;

namespace VAA.EAPI.Microservices.Flight.Services.Models.SSR
{
    public class Airline
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public static explicit operator Airline(Carrier source)
        {
            if (source == null)
                return null;

            return new Airline
            {
                Code = source.Code,
                Name = source.Name
            };
        }
    }
}
