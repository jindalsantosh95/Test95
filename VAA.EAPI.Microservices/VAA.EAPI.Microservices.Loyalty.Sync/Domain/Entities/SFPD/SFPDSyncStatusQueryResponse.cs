using VAA.EAPI.Microservices.Loyalty.Sync.Domain.Base;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Domain.Entities.SFPD
{
    public class SFPDSyncStatusQueryResponse: EventStatusQueryResponse
    {        
        public string PsdsSyncStatus { get; set; }     
    }
}
