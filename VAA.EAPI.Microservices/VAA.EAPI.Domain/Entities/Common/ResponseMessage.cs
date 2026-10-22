using System.Collections.Generic;

namespace VAA.EAPI.Domain.Entities.Common
{
    public class ResponseMessage
    {
        public IEnumerable<Warning> Warnings { get; set; }
        public IEnumerable<ErrorDetails> Errors { get; set; }
    }
}
