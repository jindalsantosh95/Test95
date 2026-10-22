namespace VAA.EAPI.Microservices.Retail.Brand.Infrastructure.ATPCO
{

    public class ATPCOTokenResponse
    {
        public string Access_Token { get; set; }
        public string Token_Type { get; set; }
        public int Expires_In { get; set; }
        public string Scope { get; set; }
    }

}
