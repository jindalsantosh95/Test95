namespace VAA.EAPI.GraphQL.Domain.Account.Models.Banner
{
    public class Banners
    {
        public string BannerCode { get; set; }
        public DateTime? LastShownDateTime { get; set; }
        public bool IsLastShown => LastShownDateTime.HasValue;
    }
}