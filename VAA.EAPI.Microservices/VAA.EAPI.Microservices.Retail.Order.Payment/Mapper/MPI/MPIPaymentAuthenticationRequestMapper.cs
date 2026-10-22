using System;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.MPI;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.MPI;
using PaymentCardAuthentication = VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.MPI.PaymentCardAuthentication;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Mapper.MPI
{
    public class MPIPaymentAuthenticationRequestMapper
    {
        public static PaymentCardAuthenticationRequest MPIDataRQMapper(AuthenticationRequest authenticationRequest)
        {


            var paymentCardAuthenticationData = authenticationRequest.PaymentDetails.First().PaymentCardAuthentication;
            var amount = authenticationRequest.PaymentDetails.First().Amount;
            var cardDetails = authenticationRequest.PaymentDetails.First().CardMethod;

            string[] expiryDate = cardDetails.ExpiryDate.Split('-');
            var decimalPrecisionCnt = GetDecimalPlaces(amount.MonetaryAmount);
            var currencyAmt = DecimalToInteger(Convert.ToDecimal(amount.MonetaryAmount), decimalPrecisionCnt);

            return new PaymentCardAuthenticationRequest
            {
                FormOfPayment = new FormOfPayment
                {
                    AuthenticationData = new PaymentCardAuthentication
                    {
                        Cavv = paymentCardAuthenticationData.Cavv,
                        DsTransactionId = paymentCardAuthenticationData.Xid,
                        Eci = paymentCardAuthenticationData.Eci,
                        DsAuthenticationVersionText = paymentCardAuthenticationData.DsAuthenticationVersionText,
                        ValidationStatusText = paymentCardAuthenticationData.ValidationStatusText
                    },
                    Price = new Price
                    {

                        CurrencyEquivalentPrice = new Currency
                        {
                            CurrencyAmt = Convert.ToString(currencyAmt),
                            CurrencyCode = amount.BaseCurrency,
                            DecimalPrecisionCnt = decimalPrecisionCnt
                        }
                    },
                    PaymentMethod = new PaymentMethod
                    {
                        PaymentCard = new PaymentCard
                        {
                            PaymentCardNum = cardDetails.TokenisedCardNumber,
                            CardVerificationValueNum = cardDetails.Cvv,
                            PaymentCardTypeCode = Convert.ToString((CardType)Enum.Parse(typeof(CardType), cardDetails.CardType)),
                            ExpirationMonthNum = expiryDate[0],
                            ExpirationYearNum = expiryDate[1]
                        }
                    }
                }
            };
        }

        private static int GetDecimalPlaces(string amount)
        {
            int length = 0;
            if (amount.IndexOf(".") > 0)
            {
                length = amount.Substring(amount.IndexOf(".") + 1).Length;
            }
            return length;
        }

        public static ulong DecimalToInteger(decimal? monetaryAmount, int numDecimals)
        {
            var converter = Convert.ToDecimal(Math.Pow(10, numDecimals));
            var wholeNumber = (ulong)(monetaryAmount * converter);
            return wholeNumber;
        }
    }
}
