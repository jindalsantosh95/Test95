using System.Collections.Generic;
namespace VAA.Facade.EnrolmentFacade.Infrastructure.Entities
{
    public class Response<T>
    {
        public Status Status { get; set; }
        public T Value { get; set; }
        public List<ResponseHeader> HeaderParameters { get; set; }
        public List<ErrorDetails> Errors { get; set; }
    }
}
