using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual
{
    public sealed class AutoAccrualFeedbackMessageLookup : TableEntity
    {
        public string CorrelationId { get; set; } = string.Empty;
    }
}
