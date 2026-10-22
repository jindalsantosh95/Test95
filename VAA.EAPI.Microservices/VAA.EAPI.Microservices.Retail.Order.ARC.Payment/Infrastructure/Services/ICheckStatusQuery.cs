using CSharpFunctionalExtensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Domain;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Infrastructure.Services
{
    public interface ICheckStatusQuery
    {
        Task<Result<Response<CheckResponse>>> Handle(string agencyNumber);
    }
}
