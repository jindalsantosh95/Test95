using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual
{
    public class SkyTeamNlaMessageLookupEntity : TableEntity
    {
        public string CorrelationId { get; set; } = string.Empty;
    }
}
