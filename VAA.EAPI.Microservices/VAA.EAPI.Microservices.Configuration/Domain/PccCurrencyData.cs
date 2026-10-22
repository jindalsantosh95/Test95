using VAA.EAPI.Microservices.Configuration.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Configuration.Domain
{
    public class PccCurrencyData
    {
        public string CurrencyCode { get; set; }
        public string PCC { get; set; }

       public static explicit operator PccCurrencyData(PccCurrencyDbEntity pccCurrencyListDbEntity)
        {
            return new PccCurrencyData()
            {
                CurrencyCode = pccCurrencyListDbEntity.CurrencyCode,
                PCC = pccCurrencyListDbEntity.PCC
            };
        }
    }
}
