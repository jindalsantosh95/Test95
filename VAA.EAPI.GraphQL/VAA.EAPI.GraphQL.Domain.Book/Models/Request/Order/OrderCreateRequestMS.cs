using VAA.EAPI.GraphQL.Domain.Book.Models.Common;
using VAA.EAPI.GraphQL.Domain.Book.Models.Offer;
using VAA.EAPI.GraphQL.Domain.Book.Models.Payment;
using DomainCustomer = VAA.EAPI.GraphQL.Domain.Book.Models.Customer;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Request.Order
{
    public class OrderCreateRequestMS
    {
        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public IEnumerable<Offers> SelectedOffers { get; set; }
        public IEnumerable<PaymentDetails> PaymentDetails { get; set; }
        public IEnumerable<DomainCustomer.CustomerInfo> CustomerDetails { get; set; }
        public bool CreditCardCheckConsent { get; set; }
        public InsuranceDetails InsuranceDetails { get; set; }
        public bool? IsInsuranceAdded { get; set; }
        public bool VoucherApplied { get; set; }
        public OrderVoucherDetails VoucherDetails { get; set; }
        public Dictionary<string, string> MetaPartner { get; set; }
    }
}
