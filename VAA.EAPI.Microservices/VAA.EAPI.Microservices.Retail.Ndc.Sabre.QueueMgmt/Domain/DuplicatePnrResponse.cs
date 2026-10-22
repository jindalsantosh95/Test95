using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Domain
{
    public class DuplicatePnrResponse
    {
        public IEnumerable<string> Rlocs { get; set; }
        public IEnumerable<ErrorDetails> Errors { get; set; }
    }
}
