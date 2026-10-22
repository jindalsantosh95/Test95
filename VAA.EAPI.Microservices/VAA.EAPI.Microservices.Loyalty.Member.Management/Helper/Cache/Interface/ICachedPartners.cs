using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.PartnersRetrieval;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Helper.Cache.Interface
{
    public interface ICachedPartners
    {
        Task<Result<Response<PartnersResponse>>> GetCachedPartnersReferenceData();
    }
}
