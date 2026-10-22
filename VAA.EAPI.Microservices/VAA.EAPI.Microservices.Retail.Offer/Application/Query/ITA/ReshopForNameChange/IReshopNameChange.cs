using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop;

namespace VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.ReshopForNameChange
{
    public interface IReshopNameChange
    {
        Task<Result<Response<OfferResponse>>> Handle(ReshopRequest reshopRequest, CancellationToken cancellationToken);

    }
}