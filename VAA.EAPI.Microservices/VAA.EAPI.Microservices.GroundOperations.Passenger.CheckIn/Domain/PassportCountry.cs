using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain
{
    public class PassportCountry
    {
        public string Code { get; set; }

        public static explicit operator PassportCountry(DeltaTrm.Country trmCountry)
        {
            return new()
            {
                Code = trmCountry.Code.Is("DEU") ? "D" : trmCountry.Code
            };
        }
    }
}