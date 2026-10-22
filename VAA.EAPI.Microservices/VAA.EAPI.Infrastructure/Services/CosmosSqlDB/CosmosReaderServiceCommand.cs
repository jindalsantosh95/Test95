using MediatR;
using System;
using System.Collections.Generic;

namespace VAA.EAPI.Infrastructure.Services.CosmosSqlDB
{
    [Obsolete("Use VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2")]
    public class CosmosReaderServiceCommand : IRequest<List<object>>
    {
        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }
        public string SqlQuery { get; set; }
        public List<KeyValuePair<string, object>> QueryParams { get; set; }
        public string DatabaseEndpoint { get; set; }
    }
}
