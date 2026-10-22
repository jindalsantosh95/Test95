using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.FlightInformation.Mapper
{
    public class AirportDetailsResponseMapper
    {
        public static Response<string> ResponseMapper()
        {
            return new Response<string>
            {
                Status = Status.Accepted
            };
        }
    }
}