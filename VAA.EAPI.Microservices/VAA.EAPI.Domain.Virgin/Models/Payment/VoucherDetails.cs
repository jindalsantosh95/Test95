using Newtonsoft.Json.Linq;
using System;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.Domain.Virgin.Models.Payment
{
    public class VoucherMethod
    {
        public string VoucherType { get; set; }
        public string Issuer { get; set; }
        public string VoucherNumber { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public CurrencyAmount Amount { get; set; }
        public int? Percentage { get; set; }

        public static explicit operator VoucherMethod(JObject source)
        {
            return new VoucherMethod()
            {
                VoucherType = source["VoucherType"]?.ToString(),
                Issuer = source["Issuer"]?.ToString(),
                VoucherNumber = source["VoucherNumber"]?.ToString(),
                IssueDate = source["IssueDate"] != null ? (DateTime)source["IssueDate"] : null,
                ExpiryDate = source["ExpiryDate"] != null ? (DateTime)source["ExpiryDate"] : null,
                Amount = source["Amount"] != null ? (CurrencyAmount)source["Amount"] : null,
                Percentage = source["Percentage"] != null ? (int)source["Percentage"] : null
            };
        }
    }
}
