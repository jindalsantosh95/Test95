using VAA.EAPI.GraphQL.Domain.NDCHealthCheck.Models;

namespace VAA.EAPI.GraphQL.Domain.NDCHealthCheck.Interfaces
{
    public interface IHeartBeatService
    {
        Task<HeartBeatResponse> GetOfferHeartBeatStatus();
        Task<HeartBeatResponse> GetOrderHeartBeatStatus();
        Task<HeartBeatResponse> GetPaymentFacadeHeartBeatStatus();
    }
}
