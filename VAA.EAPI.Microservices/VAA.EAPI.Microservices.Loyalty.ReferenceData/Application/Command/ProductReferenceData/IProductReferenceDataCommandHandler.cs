using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;


namespace VAA.EAPI.Microservices.Loyalty.ReferenceData.Application.Command.ProductReferenceData
{
    public interface IProductReferenceDataCommandHandler
    {
        Task<Result<string>> Handle(ProductReferenceDataCommand request, CancellationToken cancellationToken);
    }
}

