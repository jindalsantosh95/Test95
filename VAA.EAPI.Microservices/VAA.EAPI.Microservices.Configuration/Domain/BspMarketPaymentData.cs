using VAA.EAPI.Microservices.Configuration.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Configuration.Domain
{
    public class BspMarketPaymentData
    {
        public bool Amex { get; set; }
        public string BSPMarket { get; set; }
        public string CountryCode { get; set; }
        public bool Diners { get; set; }
        public bool Discover { get; set; }
        public bool IsCreditCardAccepted { get; set; }
        public bool IsCurrencySetUpForCCAcquirer { get; set; }
        public bool Maestro { get; set; }
        public bool Mastercard { get; set; }
        public bool UATP { get; set; }
        public bool Visa { get; set; }

        public static explicit operator BspMarketPaymentData(BspMarketPaymentDbEntity bspMarketPaymentListDbEntity)
        {
            return new BspMarketPaymentData()
            {
                Amex = bspMarketPaymentListDbEntity.Amex,
                BSPMarket = bspMarketPaymentListDbEntity.BSPMarket,
                CountryCode = bspMarketPaymentListDbEntity.CountryCode,
                Diners = bspMarketPaymentListDbEntity.Diners,
                Discover = bspMarketPaymentListDbEntity.Discover,
                IsCreditCardAccepted = bspMarketPaymentListDbEntity.IsCreditCardAccepted,
                IsCurrencySetUpForCCAcquirer = bspMarketPaymentListDbEntity.IsCurrencySetUpForCCAcquirer,
                Maestro = bspMarketPaymentListDbEntity.Maestro,
                Mastercard = bspMarketPaymentListDbEntity.Mastercard,
                UATP = bspMarketPaymentListDbEntity.UATP,
                Visa = bspMarketPaymentListDbEntity.Visa,
            };
        }
    }
}
