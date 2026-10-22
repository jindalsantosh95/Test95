namespace VAA.EAPI.Microservices.Retail.Insurance.Domain
{
    public class Message
    {
        public int MessageSequence { get; set; }
        public string MessageTag { get; set; }
        public string CompiledMessage { get; set; }
    }
}