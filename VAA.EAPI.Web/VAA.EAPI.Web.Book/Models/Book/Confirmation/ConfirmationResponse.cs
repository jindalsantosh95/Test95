using System.Collections.Generic;
using VAA.EAPI.Web.Book.Models.YourDetails.Response;
using VAA.EAPI.Web.Common.Entities;

namespace VAA.EAPI.Web.Book.Models.Book.Confirmation
{
    public class ConfirmationResponse
    {
        public PointOfSale PointOfSale { get; set; }
        public Summary Summary { get; set; }
        public int BookingReference { get; set; }
        public IEnumerable<object> Infants { get; set; }
        public IEnumerable<object> Children { get; set; }
        public Configuration Configuration { get; set; }
        public IEnumerable<object> Adults { get; set; }
        public IEnumerable<Person> People { get; set; }
        public LeadPassenger LeadPassenger { get; set; }
        public object LeadDriver { get; set; }
        public IEnumerable<PaymentInformation> PaymentInformation { get; set; }
        public object PayPal { get; set; }
        public string ResourceType { get; set; }
        public IEnumerable<ErrorDetails> Errors { get; set; }
        public IEnumerable<Warning> Warnings { get; set; }
        public CustomerProfile CustomerProfile { get; set; }
        public bool NewVacCreated { get; set; }
        public FCEarnPoints EarnPoints { get; set; }
        public CustomerProfile VACCustomer { get; set; }
    }
}
