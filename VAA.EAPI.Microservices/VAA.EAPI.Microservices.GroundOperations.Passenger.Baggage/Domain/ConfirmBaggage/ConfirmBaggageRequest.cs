using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.DeltaSsci;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.ConfirmBaggage
{
    public class ConfirmBaggageRequest
    {
        public string TravelPartyId { get; set; }
        public string PaymentReferenceId { get; set; }
        public CardHolderDetail CardHolderDetail { get; set; }
        public BillingAddress BillingAddress { get; set; }
        public PaymentDetail PaymentDetails { get; set; }
        public string Email { get; set; }
        public string PnrLocator { get; set; }

        public static explicit operator PurchaseRequest(ConfirmBaggageRequest request)
        {
            return new PurchaseRequest
            {
                PaymentReferenceId = request.PaymentReferenceId,
                CreditCardType = request?.CardHolderDetail?.CardType,
                CreditCardNumber = request?.CardHolderDetail?.CardNumber,
                CreditFirstName = request?.CardHolderDetail?.CardHolderFirstName,
                CreditLastName = request?.CardHolderDetail?.CardHolderLastName,
                ExpirationMonth = request?.CardHolderDetail?.ExpiryMonth,
                ExpirationYear = request?.CardHolderDetail?.ExpiryYear,
                SecurityCode = request?.CardHolderDetail?.CardVerificationValue,
                Address1 = request?.BillingAddress?.AddressLine1,
                Address2 = request?.BillingAddress?.AddressLine2,
                City = request?.BillingAddress?.City,
                Country = request?.BillingAddress?.Country,
                ZipCode = request?.BillingAddress?.PostalCode,
                PaymentAmount = request?.PaymentDetails?.PaymentAmount,
                PaymentAmountDecimal = request?.PaymentDetails?.DecimalPrecisionCount,
                CurrencyCode = request?.PaymentDetails?.CurrencyCode,
                PayWithMilesToggle = request?.PaymentDetails?.PayWithMilesToggle ?? false,
                Psd2ErrorKey = string.Empty,
            };
        }
    }
}
