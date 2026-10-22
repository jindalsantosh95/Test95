using CSharpFunctionalExtensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.TripDetails;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Interface
{
    public interface ITripDetailsService
    {
        Task<Result<Response<IEnumerable<TripDetailsResponse>>>> RetrieveTripDetails(TripDetailsRequest request);
    }
}