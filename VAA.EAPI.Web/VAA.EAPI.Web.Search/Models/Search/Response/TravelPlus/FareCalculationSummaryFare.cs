/**
* The flight search fare calculation summary fare
*
* @author Gareth Williams
*/
namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus
{
    public class FareCalculationSummaryFare
    {

        public string passengerType { get; set; }

        public int quantity { get; set; }

        public Price price { get; set; }

    }
}
