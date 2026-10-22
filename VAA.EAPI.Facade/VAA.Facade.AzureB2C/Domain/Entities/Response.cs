using System;
using System.Collections.Generic;


namespace VAA.Facade.AzureB2C.Domain.Entities
{
    public class Response<T>
    {
        public Status Status { get; set; }
        public T Value { get; set; }
        public List<FailureResponse> Errors { get; set; }
    }
}
