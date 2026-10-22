using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Flight.Services.Models.SSR
{
    public class SSRUpsertRequest
    {
        public string PassengerResourceId { get; set; }
        public IEnumerable<SSRSegment> SSRSegments { get; set; }
        public bool? ApplyToAll { get; set; }
        public SSRModifyActionType SSRModifyActionType { get; set; }
    }
}
