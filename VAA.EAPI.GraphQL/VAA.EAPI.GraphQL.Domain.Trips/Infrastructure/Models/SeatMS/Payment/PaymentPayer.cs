using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS.Payment
{
    public class PaymentPayer
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Address PayerAddress { get; set; }
        public LoyaltyMembership LoyaltyMemberDetails { get; set; }
        public bool LoyaltyMemberTravelling { get; set; }
        public string PayerType { get; set; }
        public Contacts PayerContacts { get; set; }
        
        public static explicit operator PaymentPayer((PaymentMS.PaymentDeails.PaymentDetails payment, BasketMS.ContactDetails contactDetails) input)
        {
            var (payment, basketContact) = input;
            if (payment.Payer == null) return null;

            return new PaymentPayer
            {
                FirstName = payment?.Payer.FirstName,
                LastName = payment?.Payer.LastName,
                PayerAddress = (Address)payment?.BillingAddress,
                PayerContacts = (Contacts)payment.Payer.PayerContacts ?? new() { Email = basketContact?.EmailAddress }
            };
        }
    }
}