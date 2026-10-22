using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class SegmentCertificateEligibility
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum CertificateCategoryEnum
        {
            [EnumMember(Value = "UPGRADE")]
            UPGRADE = 1
        }
        public CertificateCategoryEnum CertificateCategory { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum CertificateTypeEnum
        {
            [EnumMember(Value = "GUC")]
            GUC = 1,
            [EnumMember(Value = "RUC")]
            RUC = 2
        }
        public CertificateTypeEnum CertificateType { get; set; }
    }
}
