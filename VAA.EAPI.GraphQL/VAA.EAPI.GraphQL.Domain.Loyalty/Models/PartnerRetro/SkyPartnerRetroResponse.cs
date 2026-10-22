using System.Collections.Generic;
using VAA.EAPI.GraphQL.Common.Entities;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerRetro
{
    public class SkyPartnerRetroResponse
    {
        public string CustomerId { get; set; }
        public string TransactionId { get; set; }
        public IEnumerable<ErrorDetails> Errors { get; set; }
    }
}
