using System.Linq;
using System.Xml;
using System.Xml.Linq;
using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Infrastructure.Email.Policies
{
    public static class EmailRecipientPolicy
    {
        public const string YoungFlyerDummyEmailDomain = "U18.virgin.com";

        public static bool ShouldSuppress(string emailAddress)
        {
            if (emailAddress.IsNullOrWhiteSpace())
            {
                return false;
            }

            var trimmedEmailAddress = emailAddress.Trim();
            var separatorIndex = trimmedEmailAddress.LastIndexOf('@');

            if (separatorIndex <= 0 || separatorIndex == trimmedEmailAddress.Length - 1)
            {
                return false;
            }

            var domain = trimmedEmailAddress[(separatorIndex + 1)..].TrimEnd('.');

            return domain.Is(YoungFlyerDummyEmailDomain);
        }

        public static bool ShouldSuppressAcmRequest(string request)
        {
            if (request.IsNullOrWhiteSpace())
            {
                return false;
            }

            try
            {
                var document = XDocument.Parse(request, LoadOptions.None);
                var recipient = document.Root?
                    .DescendantsAndSelf()
                    .FirstOrDefault(element => element.Name.LocalName.Is("rtEvent"))?
                    .Attribute("email")?
                    .Value;

                return ShouldSuppress(recipient);
            }
            catch (XmlException)
            {
                return false;
            }
        }
    }
}
