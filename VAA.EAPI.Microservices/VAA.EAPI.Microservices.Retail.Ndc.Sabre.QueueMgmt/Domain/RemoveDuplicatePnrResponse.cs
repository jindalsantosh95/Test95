using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Domain
{
    public class RemoveDuplicatePnrResponse
    {
        public bool Removed { get; set; }
        public IEnumerable<ErrorDetails> Errors { get; set; }
    }
}