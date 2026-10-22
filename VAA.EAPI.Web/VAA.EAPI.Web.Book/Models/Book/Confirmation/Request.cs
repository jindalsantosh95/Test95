using System.Collections.Generic;

namespace VAA.EAPI.Web.Book.Models.Book.Confirmation
{
    public class Request
    {
        public string BasketId { get; set; }
        public string PaymentStatus { get; set; }
        public string DeviceInfo { get; set; }
        public string IpAddress { get; set; }
        public string BrowserInfo { get; set; }
        public bool CcCheckConsent { get; set; }
        public IEnumerable<KeyValuePair<string, string>>? MetaPartner { get; set; }
    }
}
