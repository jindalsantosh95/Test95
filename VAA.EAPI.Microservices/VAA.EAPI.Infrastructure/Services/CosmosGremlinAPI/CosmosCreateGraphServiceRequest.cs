using MediatR;
using System.Collections.Generic;

namespace VAA.EAPI.Infrastructure.Services.CosmosGremlinAPI
{
    public class CosmosCreateGraphServiceRequest : IRequest<IDictionary<string, object>>
    {

        public string HostEndPoint { get; set; }
        public string DatabaseName { get; set; }
        public string Key { get; set; }
        public string Container { get; set; }
        public IDictionary<string, string> GremlinQueries { get; set; }
    }
}
