namespace VAA.EAPI.Web.Common.Entities
{
    public class Response<T>
    {
        public Status Status { get; set; }
        public T Value { get; set; }
        public List<ResponseHeader> HeaderParameters { get; set; } = new();
        public bool IgnoreNullValues { get; set; } = false;
        public List<ErrorDetails> Errors { get; set; }
    }
}
