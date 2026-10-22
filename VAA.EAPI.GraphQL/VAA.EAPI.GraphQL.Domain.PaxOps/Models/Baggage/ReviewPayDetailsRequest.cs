using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.BaggageMS;
using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.PaymentMS.PaymentDetails;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.Baggage
{
    public class ReviewPayDetailsRequest
    {
        public string TravelPartyId { get; set; }
        public string PaymentReferenceId { get; set; }
        public CardHolderDetail CardHolderDetail { get; set; }
        public BillingAddress BillingAddress { get; set; }
        public PaymentInfo PaymentDetails { get; set; }
        public string PnrLocator { get; set; }
        public string Email { get; set; }

        public static explicit operator ReviewPayDetailsRequest((PaymentTransactionDetails paymentTransactionDetails, string travelPartyId, string emailId, string bookingReference) source)
        {
            var paymentDetails = source.paymentTransactionDetails?.PaymentDetails?.FirstOrDefault();
            var cardExpirationDate = paymentDetails?.CardMethod?.ExpiryDate.Split('-');

            return new ReviewPayDetailsRequest
            {
                TravelPartyId = source.travelPartyId,
                PaymentReferenceId = source.paymentTransactionDetails.PaymentReferenceId,
                CardHolderDetail = new CardHolderDetail
                {
                    CardHolderFirstName = paymentDetails.Payer.FirstName,
                    CardHolderLastName = paymentDetails.Payer.LastName,
                    ExpiryMonth = cardExpirationDate[0],
                    ExpiryYear = $"20{cardExpirationDate[1]}",
                    CardNumber = paymentDetails.CardMethod.CardNumber,
                    CardType = paymentDetails.CardMethod.CardType.ToString(),
                    CardVerificationValue = paymentDetails.CardMethod.Cvv
                },
                BillingAddress = new BillingAddress
                {
                    AddressLine1 = paymentDetails.BillingAddress.AddressLine1,
                    AddressLine2 = paymentDetails.BillingAddress.AddressLine2,
                    City = paymentDetails.BillingAddress.City,
                    Country = paymentDetails.BillingAddress.Country,
                    PostalCode = paymentDetails.BillingAddress.PostalCode
                },
                PaymentDetails = new PaymentInfo
                {
                    PaymentAmount = paymentDetails.Amount.MonetaryAmount,
                    DecimalPrecisionCnt = paymentDetails.Amount.DecimalPrecisionCount.ToString(),
                    CurrencyCode = paymentDetails.Amount.BaseCurrency,
                    PayWithMilesToggle = false,
                },
                Email = source.emailId,
                PnrLocator = source.bookingReference
            };
        }
    }
}
