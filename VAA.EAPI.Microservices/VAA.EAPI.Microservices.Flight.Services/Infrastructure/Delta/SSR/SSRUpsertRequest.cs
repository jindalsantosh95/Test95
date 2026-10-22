using System.Collections.Generic;
using System.Linq;

namespace VAA.EAPI.Microservices.Flight.Services.Infrastructure.Delta.SSR
{
    public partial class SSRUpsertRequest
    {
        public string PassengerResourceId { get; set; }
        public IEnumerable<SSRSegment> SSRSegments { get; set; }
        public bool? ApplyToAll { get; set; }
        public SSRModifyActionType ActionType { get; set; }


        public static explicit operator SSRUpsertRequest(Models.SSR.SSRUpsertRequest sourceRequest)
        {
            if (sourceRequest == null)
                return null;

            return new SSRUpsertRequest()
            {
                PassengerResourceId = sourceRequest.PassengerResourceId,
                SSRSegments = sourceRequest.SSRSegments?.Select(s => (SSRSegment)s),
                ApplyToAll = sourceRequest.ApplyToAll,
                ActionType = (SSRModifyActionType)sourceRequest.SSRModifyActionType
            };
        }
    }
}
