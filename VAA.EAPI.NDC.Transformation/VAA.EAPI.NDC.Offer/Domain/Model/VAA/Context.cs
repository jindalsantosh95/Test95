using System.Collections.Generic;
using VAA.EAPI.NDC.Offer.Domain.Model.Order;
namespace VAA.EAPI.NDC.Offer.Domain.Model.VAA
{
    public class Context
    {
        public Context(bool? includePrice = default, string currencyCode = default, bool? includeRichContent = default, string recordLocator = default, string deviceType = default, List<Customer> customerChars = default)
        {
            this.IncludePrice = includePrice;
            this.CurrencyCode = currencyCode;
            this.IncludeRichContent = includeRichContent;
            this.RecordLocator = recordLocator;
            this.DeviceType = deviceType;
            this.CustomerChars = customerChars;
        }

        public bool? IncludePrice { get; set; }

        public string CurrencyCode { get; set; }

        public bool? IncludeRichContent { get; set; }

        public string RecordLocator { get; set; }

        public string DeviceType { get; set; }

        public IEnumerable<Customer> CustomerChars { get; set; }

    }
}
