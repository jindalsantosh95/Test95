using System.Collections.Generic;
using VAA.EAPI.GraphQL.Common.Entities;

namespace VAA.EAPI.GraphQL.Infrastructure.Entities
{
    public class EAPIResponse
    {
        public string Value { get; set; }
        public List<ErrorDetails> Errors { get; set; }
    }
}
