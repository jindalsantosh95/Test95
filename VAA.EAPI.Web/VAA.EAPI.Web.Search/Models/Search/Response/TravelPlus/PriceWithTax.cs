/**
* The price with tax
*
* @author Gareth Williams
*/
namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus
{
    public class PriceWithTax : Price
    {

        public decimal? Tax { get; set; }

        public decimal? AmountIncludingTax { get; set; }

        public string PriceDifference { get; set; }

        public string PriceDifferenceSign { get; set; }

        public string AwardPointsDifference { get; set; }

        public string AwardPointsDifferenceSign { get; set; }

    }
}
