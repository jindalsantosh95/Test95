using System.Xml.Serialization;

namespace VAA.EAPI.Infrastructure.PSS.Entities.RetrieveAmenities
{
    [XmlRoot(ElementName = "Root")]
    public class BrandTokenResponse
    {
        [XmlElement(ElementName = "token_type")]
        public string Token_type { get; set; }
        [XmlElement(ElementName = "access_token")]
        public string Access_token { get; set; }
        [XmlElement(ElementName = "issued_at")]
        public string Issued_at { get; set; }
        [XmlElement(ElementName = "expires_in")]
        public string Expires_in { get; set; }
    }
}
