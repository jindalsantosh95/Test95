namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Domain.AgencyOCNDetails
{
    public class EndpointDetails
    {
        public string Endpoint { get; set; }
        public int EndpointId {  get; set; }
        public string SoapAction {  get; set; }
        public string TokenUrl {  get; set; }
        public string AuthType { get; set; }
        public string ClientKey {  get; set; }
        public string ClientSecret { get; set; }
    }
}
