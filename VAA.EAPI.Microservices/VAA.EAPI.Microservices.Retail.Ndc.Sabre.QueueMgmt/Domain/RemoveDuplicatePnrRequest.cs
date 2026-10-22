namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Domain
{
    public class RemoveDuplicatePnrRequest
    {
        public string PCC { get; set; }
        public string Pnr { get; set; }
    }
}