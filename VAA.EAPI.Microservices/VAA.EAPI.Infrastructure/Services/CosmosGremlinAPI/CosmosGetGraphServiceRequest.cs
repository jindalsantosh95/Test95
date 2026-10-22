using MediatR;

namespace VAA.EAPI.Infrastructure.Services.CosmosGremlinAPI
{
    class CosmosGetGraphServiceRequest : IRequest<string>
    {
        public string HostEndPoint { get; set; }
        public string DatabaseName { get; set; }
        public string Key { get; set; }
        public string Container { get; set; }
        public string GraphQuery { get; set; }
    }
}
