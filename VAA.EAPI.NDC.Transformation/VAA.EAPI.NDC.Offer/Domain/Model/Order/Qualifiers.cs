using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class Qualifiers
    {
        public string TransactionCurrency { get; set; }
        public bool? BaggageQualifier { get; set; }
        public bool? SeatQualifier { get; set; }
        public bool? ServiceQualifier { get; set; }
        public bool? FlightServiceQualifer { get; set; }
        public string ExistingOrderQualifier { get; set; }
        public IEnumerable<string> PaymentCardsQualifier { get; set; }
        public IEnumerable<ProgrammeQualifier> ProgrammesQualifier { get; set; }
        public string TripPurposeQualifier { get; set; }
        public string SpecialNeedsQualifier { get; set; }
        public IEnumerable<string> SpecialFaresQualifier { get; set; }
        public IEnumerable<VoucherDetails> VoucherCertQualifier { get; set; }


    }
}
