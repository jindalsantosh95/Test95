using CSharpFunctionalExtensions;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Sustainability.Application.Providers;
using VAA.EAPI.Microservices.Retail.Sustainability.Domain;

namespace VAA.EAPI.Microservices.Retail.Sustainability.Application.Query
{
    public sealed class SustainabilityRequestHandler : IRequestHandler<SustainabilityRequest, Result<Response<IEnumerable<SustainMSResponse>>>>
    {
        private readonly ICarbonFootprintProvider carbonFootprintProvider;

        public SustainabilityRequestHandler(ICarbonFootprintProvider carbonFootprintProvider) => this.carbonFootprintProvider = carbonFootprintProvider ?? throw new System.ArgumentNullException(nameof(carbonFootprintProvider));

        public async Task<Result<Response<IEnumerable<SustainMSResponse>>>> Handle(SustainabilityRequest request, CancellationToken cancellationToken) => new Response<IEnumerable<SustainMSResponse>>
        {
            Status = Status.OK,
            Value = (await carbonFootprintProvider.FetchCarbonFootprintsAsync((CarbonRequest)request.Request, cancellationToken)).Select(x => (SustainMSResponse)x)
        };
    }
}
