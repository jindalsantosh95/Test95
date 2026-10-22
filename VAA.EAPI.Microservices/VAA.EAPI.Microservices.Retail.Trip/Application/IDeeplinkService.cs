using CSharpFunctionalExtensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Trip.Domain.DeepLink;

namespace VAA.EAPI.Microservices.Retail.Trip.Application
{
    public interface IDeeplinkService
    {
        Task<Result<Response<DeeplinkResponse>>> GetDeeplinkUrlDetails(DeepLinkRequestPayload request);

        Task<Result<Response<DeeplinkEndpointDetails>>> GetDeeplinkDetailsForProxy(string cacheId);
    }
}
