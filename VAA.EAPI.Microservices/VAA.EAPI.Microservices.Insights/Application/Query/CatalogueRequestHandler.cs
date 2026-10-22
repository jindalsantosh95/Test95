using CSharpFunctionalExtensions;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Insights.Application.Providers;
using VAA.EAPI.Microservices.Insights.Domain;

namespace VAA.EAPI.Microservices.Insights.Application.Query
{
    public sealed class CatalogueRequestHandler : IRequestHandler<CatalogueRequest, Result<Response<ICollection<Endpoint>>>>
    {
        private readonly ICatalogueProvider catalogueProvider;

        public CatalogueRequestHandler(ICatalogueProvider catalogueProvider)
        {
            this.catalogueProvider = catalogueProvider ?? throw new System.ArgumentNullException(nameof(catalogueProvider));
        }

        public async Task<Result<Response<ICollection<Endpoint>>>> Handle(CatalogueRequest request, CancellationToken cancellationToken)
        {
            var catalogue = await catalogueProvider.FetchCatalogueAsync(request.SubscriptionId, request.ApiVersion, request?.Environment);

            return new Response<ICollection<Endpoint>>
            {
                Status = Status.OK,
                Value = catalogue
            };
        }
    }
}
