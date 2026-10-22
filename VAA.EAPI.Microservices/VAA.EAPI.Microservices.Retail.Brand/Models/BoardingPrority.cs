namespace VAA.EAPI.Microservices.Retail.Brand.Models
{
    public class BoardingPrority : SegmentPolicy
    {
        public string Code { get; set; }
        public string CommercialName { get; set; }

        public static explicit operator BoardingPrority(RouteHappy.BoardingPriority source)
        {
            if (source == null) return null;

            return new()
            {
                Application = source.Application.ToString(),
                Code = source.Code,
                Assessment = source.Assessment.ToString(),
                CommercialName = source.CommercialName,
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