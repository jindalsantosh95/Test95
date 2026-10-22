using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Web.Book.Domain.RefData.Requests;
using VAA.EAPI.Web.Book.Models.RefData;
using VAA.EAPI.Web.Common.Helper;
using VAA.EAPI.Web.Common.Infrastructure.Entities.GraphQL;
using VAA.EAPI.Web.Common.Serialisation;

namespace VAA.EAPI.Web.Book.Domain.RefData.Handlers
{
    public class FQTProgrammesRetrieveRequestHandler : IRequestHandler<FQTProgrammesRetrieveRequest, IEnumerable<FQTProgramme>>
    {
        private readonly IMediator _mediator;

        public FQTProgrammesRetrieveRequestHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<IEnumerable<FQTProgramme>> Handle(FQTProgrammesRetrieveRequest request, CancellationToken cancellationToken)
        {
            var graphQLRequest = new GraphQLRequest
            {
                PqId = ConfigReader.Get(Constants.Environment.RetrieveFQTProgrammesPqId)
            };

            var graphQLResponse = await _mediator.Send(graphQLRequest, cancellationToken);

            return Json.DeserializeArray<IEnumerable<FQTProgramme>>((JArray)graphQLResponse.Data["retrieveFQTProgrammes"]);
        }
    }
}
