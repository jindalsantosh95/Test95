using MediatR;
using System;
using System.Collections.Generic;

namespace VAA.EAPI.Infrastructure.Repository.CosmosSqlDB
{
    [Obsolete("Use VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2")]
    public class CosmosReaderRepoCommand : IRequest<string>
    {
        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }
        public string SqlQuery { get; set; }
        public List<KeyValuePair<string, object>> QueryParams { get; set; }
        public string DatabaseEndpoint { get; set; }
    }
}
