
namespace VAA.EAPI.Infrastructure.Email.Entities
{
    public class WorkFlowRequest
    {
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public string CcAddress { get; set; }
        public string Subject { get; set; }
        public string Template { get; set; }
        public object Data { get; set; }
    }
}
