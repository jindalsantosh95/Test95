using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.Domain.Virgin.Models.Products.Seat
{
    public class SeatEligibility
    {
        public string FirstNameNumber { get; set; }
        public string LastNameNumber { get; set; }
        public CurrencyAmount BasePrice { get; set; }
        public string EligibilityReason { get; set; }
        public string CatalogueRefId { get; set; }
        public IEnumerable<string> CustomerId { get; set; }
        public IEnumerable<string> CustomerPTC { get; set; }
        public IEnumerable<OfferItemNumber> OfferItemId { get; set; }
    }
}
