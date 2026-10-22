using VAA.EAPI.GraphQL.Domain.Book.Models.Common;
using VAA.EAPI.GraphQL.Domain.Book.Models.Payment;
using VAA.EAPI.GraphQL.Domain.Book.Models.Products.Flight;
using DomainCustomer = VAA.EAPI.GraphQL.Domain.Book.Models.Customer;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Request.PaymentRequest
{
    public class OrderPaymentRequestMs
    {
        public Pos Pos { get; set; }
        public Models.Payment.Payment paymentAmt { get; set; }
        public LoyaltyMember LoyaltyMemberDetails { get; set; }
        public IEnumerable<Models.Payment.Offer> Selectedoffers { get; set; }
        public IEnumerable<DomainCustomer.CustomerInfo> CustomerDetails { get; set; }
        public IEnumerable<Segment> FlightSegments { get; set; }
        public IEnumerable<Journey> Journey { get; set; }
        public AdditionalConfigParams AdditionalConfigParams { get; set; }
        public IEnumerable<SeatList> SeatDetails { get; set; }
        public InsuranceDetails InsuranceDetails { get; set; }
        public string TripType { get; set; }
    }
}