using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.Remarks;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Application.Services
{
    public interface ISpecialRemarksService
    {
        Task<Result<Response<string>>> AddAsync(AddSpecialRemarkRequest request, string channelId, CancellationToken cancellationToken);
    }
}
