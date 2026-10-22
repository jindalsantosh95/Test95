using System;
using System.Collections.Generic;

namespace VAA.EAPI.Domain.Entities.Common
{
    public class Response<T>
    {
        public Status Status { get; set; }
        public T Value { get; set; }
        public List<ResponseHeader> HeaderParameters { get; set; } = new();
        public bool IgnoreNullValues { get; set; } = true;
        public List<ErrorDetails> Errors { get; set; }
        public Exception Exception { get; set; }
        public string ErrorMessage { get; set; }
        public ResponseMessage ResponseMessages { get; set; }
    }
}
