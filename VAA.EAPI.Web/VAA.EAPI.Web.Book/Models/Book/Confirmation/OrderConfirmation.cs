using System.Collections.Generic;

namespace VAA.EAPI.Web.Book.Models.Book.Confirmation
{
    public class OrderConfirmation
    {
        public CustomerConsentInfo CustomerConsentInfo { get; set; }
        public Dictionary<string, string>? MetaPartner { get; set; }
    }
}