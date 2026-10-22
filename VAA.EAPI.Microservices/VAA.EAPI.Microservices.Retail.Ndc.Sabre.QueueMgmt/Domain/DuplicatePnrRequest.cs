namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Domain
{
    public class DuplicatePnrRequest
    {
        public string Queue { get; set; }
        public string PCC { get; set; }
    }
}
