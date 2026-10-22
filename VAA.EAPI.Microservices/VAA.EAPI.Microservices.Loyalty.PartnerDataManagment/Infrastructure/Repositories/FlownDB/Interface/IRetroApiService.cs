using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.FlightDetails;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.FlownDB.Interface
{
    public interface IRetroApiService
    {
        public Task<FlightDetails> GetCodeShareFlightDetails(string flightNumber, string activityDate);
    }
}
