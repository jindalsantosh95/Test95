namespace VAA.EAPI.Microservices.Retail.Brand.Models
{
    public class UpgradeEligibility : SegmentPolicy
    {
        public string Code { get; set; }
        public string ComercialName { get; set; }

        public static explicit operator UpgradeEligibility(RouteHappy.UpgradeEligibility source)
        {
            if (source == null)
                return null;

            return new()
            {
                Application = source.Application.ToString(),
                Assessment = source.Assessment.ToString(),
                Code = source.Code,
                ComercialName = source.CommercialName,
                Description = source.Description,
                GroupCode = source.GroupCode,
                Headline = source.Headline,
                LargeImageLink = source.LargeIconUrl,
                SmallImageLink = source.SmallIconUrl,
                SubGroupCode = source.SubGroupCode
            };
        }
    }
}
