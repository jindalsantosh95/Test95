using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.DeltaSsci;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.ConfirmBaggage
{
    public class ConfirmBaggageResponse
    {
        public string OriginAirportCode { get; set; }
        public string OriginAirportName { get; set; }
        public string DestinationAirportCode { get; set; }
        public string DestinationAirportName { get; set; }
        public string TotalPaidAmount { get; set; }
        public IEnumerable<Passenger> PassengerList { get; set; }
        public string CurrencySymbol { get; set; }
        public string CurrencyCode { get; set; }
        public string DecimalPrecisionCnt { get; set; }

        public static explicit operator ConfirmBaggageResponse(BagsPurchaseReceipt response)
        {
            if (!response.NextAction.Is("success")) return null;

            var currencyCode = response.WithLocalAmt ? response.CurrencyLocal : response.CurrencyCode;

            return new ConfirmBaggageResponse
            {
                OriginAirportCode = response.DepartureAirportCode,
                OriginAirportName = response.DepartureAirportName,
                DestinationAirportCode = response.ArrivalAirportCode,
                DestinationAirportName = response.ArrivalAirportName,
                TotalPaidAmount = response.WithLocalAmt ? response.TotalAmtLocalDecimal.IsNullOrEmpty() ? response.TotalAmtLocal : $"{response.TotalAmtLocal}.{response.TotalAmtLocalDecimal}" : response.TotalAmtUSDDecimal.IsNullOrEmpty() ? response.TotalAmtUSD : $"{response.TotalAmtUSD}.{response.TotalAmtUSDDecimal}",
                PassengerList = response.PassengerList?
                    .Select(p => (Passenger)p)
                    .Where(p => p is not null)
                    .ToList() ?? new List<Passenger>(),
                CurrencyCode = currencyCode,
                CurrencySymbol = GetCurrencySymbol(response, currencyCode)
            };
        }

        private static string GetCurrencySymbol(BagsPurchaseReceipt response, string currencyCode)
        {
            if (response.WithLocalAmt && !response.CurrencyLocalSymbol.IsNullOrEmpty())
            {
                return response.CurrencyLocalSymbol;
            }

            return currencyCode;
        }
    }
}
