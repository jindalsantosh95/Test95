
namespace VAA.EAPI.Microservices.Retail.Trip.Domain.MessageDetails
{
    public class Link
    {

        public string Label { get; set; }
        public string Url { get; set; }
        public string RuleText { get; set; }

        public static explicit operator Link(Infrastructure.Proxy.DeltaMessageCentre.Link source)
        {
            if (source == null)
                return null;

            return new Link
            {
                Label = source.Label,
                Url = source.Url,
                RuleText = source.RuleText
            };
        }
    }
}
