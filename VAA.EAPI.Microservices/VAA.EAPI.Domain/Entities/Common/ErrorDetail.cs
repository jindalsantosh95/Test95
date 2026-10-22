using System.Collections.Generic;

namespace VAA.EAPI.Domain.Entities.Common
{
    public class ErrorDetail
    {
        public bool IsDynamicMessage { get; set; }
        public string ErrorCode { get; set; }
        public IEnumerable<DynamicMessagesContent> DynamicMessagesContents { get; set; }
    }
}
