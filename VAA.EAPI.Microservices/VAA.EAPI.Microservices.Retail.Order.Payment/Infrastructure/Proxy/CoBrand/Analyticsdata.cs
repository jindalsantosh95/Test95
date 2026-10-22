using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.CoBrand
{
    public class Analyticsdata
    {
        public string ApplicationId { get; set; }
        public string AbTestName { get; set; }
        public string AcorId { get; set; }
        public string ClientName { get; set; }
        public string CountryLanguage { get; set; }
        public string CustomerType { get; set; }
        public string ClientMerchantId { get; set; }
        public string DirectToDeviceToken { get; set; }
        public string DirectToDeviceSourceChannel { get; set; }
        public string DeclineReason { get; set; }
        public string Device { get; set; }
        public string GeoLocation { get; set; }
        public string Market { get; set; }
        public string MerchantId { get; set; }
        public string OrganizationChannel { get; set; }
        public string OrganizationFlow { get; set; }
        public string PlatformIndicator { get; set; }
        public string PageFunction { get; set; }
        public string PageKind { get; set; }
        public string PageName { get; set; }
        public string PageSubFlow { get; set; }
        public string PageSubFunction { get; set; }
        public string PrefillType { get; set; }
        public string Product { get; set; }
        public string Program { get; set; }
        public string Result { get; set; }
        public string SsnType { get; set; }
        public string SiteIdentifier { get; set; }
        public string Token { get; set; }
        public string PromoCode { get; set; }
        public string SiteCode { get; set; }
        public string PlatformToken { get; set; }
        public string InternalCampaign { get; set; }
    }
}
