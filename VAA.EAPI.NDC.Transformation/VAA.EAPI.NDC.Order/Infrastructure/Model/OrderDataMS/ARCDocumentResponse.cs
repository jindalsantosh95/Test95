using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.OrderDataMS
{
    public class ARCDocumentResponse
    {
        public ARCDocument ARCDocument { get; set; }
        public IEnumerable<ErrorDetails> Errors { get; set; }

    }
}
