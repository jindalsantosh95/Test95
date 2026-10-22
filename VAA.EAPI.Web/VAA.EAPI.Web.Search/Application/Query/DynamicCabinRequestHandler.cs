using MediatR;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Web.Common.Helper;
using VAA.EAPI.Web.Common.Infrastructure.Entities.GraphQL;
using VAA.EAPI.Web.Common.Serialisation;
using VAA.EAPI.Web.Search.Models.Amenity;
using VAA.EAPI.Web.Search.Models.Brand.Request;
using VAA.EAPI.Web.Search.Models.Search.Request;

namespace VAA.EAPI.Web.Search.Application.Query
{
    public class DynamicCabinRequestHandler : IRequestHandler<DynamicCabinRequest, CabinAmenity>
    {
        private readonly IMediator _mediator;

        public DynamicCabinRequestHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
        }

        public async Task<CabinAmenity> Handle(DynamicCabinRequest request, CancellationToken cancellationToken)
        {
            var graphQLRequest = new GraphQLRequest
            {
                PqId = ConfigReader.Get(Constants.Environment.GraphQLBrandPqId),
                Variables = JObject.Parse(Json.Serialize(new { request = (BrandQueryRequest)request }, false))
            };

            var graphQLResponse = await _mediator.Send(graphQLRequest, cancellationToken);

            return Json.DeserializeObject<CabinAmenity>(graphQLResponse.Data[graphQLRequest.PqId]);
        }
    }
}
