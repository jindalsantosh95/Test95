namespace VAA.EAPI.Web.Search.Models.Search.Request
{
    public class BaseSearchRequest : BaseRequest
    {
        public string SliceId { get; set; }
        public string OfferId { get; set; }
        public string FareBasisCode { get; set; }
        public string AuthToken { get; set; }
    }
}
