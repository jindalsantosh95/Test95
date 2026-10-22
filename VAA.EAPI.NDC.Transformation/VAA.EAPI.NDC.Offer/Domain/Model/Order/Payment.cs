using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class Payment
    {
        public IEnumerable<string> FormOfPayments { get; set; }
        public CurrencyAmount Amount { get; set; }
        public CurrencyAmount SurchargeAmount { get; set; }
        public float? SurchargePercentage { get; set; }
        public string BSPMethod { get; set; }
        public CardDetails CardMethod { get; set; }
        public VoucherDetails VoucherMethod { get; set; }
        public AwardDetails AwardMethod { get; set; }

    }
}
