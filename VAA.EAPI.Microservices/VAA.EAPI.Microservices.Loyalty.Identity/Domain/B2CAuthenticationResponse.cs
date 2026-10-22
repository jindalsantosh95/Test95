namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain
{
    public class B2CAuthenticationResponse
    {
        public string Token_type { get; set; }
        public string Scope { get; set; }
        public string Expires_in { get; set; }
        public string Ext_expires_in { get; set; }
        public string Expires_on { get; set; }
        public string Not_before { get; set; }
        public string Resource { get; set; }
        public string Access_token { get; set; }
        public string Refresh_token { get; set; }
        public string Id_token { get; set; }
    }
}
