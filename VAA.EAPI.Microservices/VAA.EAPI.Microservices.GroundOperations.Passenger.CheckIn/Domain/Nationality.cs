using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain
{
    public class Nationality
    {
        public string Name { get; set; }
        public string Code { get; set; }

        public static explicit operator Nationality(DeltaTrm.Country trmCountry)
        {
            return new()
            {
                Code = trmCountry.Code.Is("DEU") ? "D" : trmCountry.Code
            };
        }
    }
}