using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Models.CosmosGraph;

namespace VAA.EAPI.Infrastructure.Repository.CosmosGremlinAPI
{
    public class CosmosGremlinRepoCommand : IRequest<Response<string>>
    {
        public string GraphService { get; set; }

        public IDictionary<string, GraphObject> GraphTraversalItems { get; set; }

    }
}
