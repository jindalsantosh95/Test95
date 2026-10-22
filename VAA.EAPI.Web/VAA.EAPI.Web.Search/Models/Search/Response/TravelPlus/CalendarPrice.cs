/**
* The flight search calendar price
*
* @author Gareth Williams
*/
namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus
{
    public class CalendarPrice
    {

        public decimal amount { get; set; }

        public string currency { get; set; }

        public bool minimumPriceInWeek { get; set; }

        public bool minimumPriceInMonth { get; set; }

        public int remaining { get; set; }

        public bool direct { get; set; }

        public string awardPoints { get; set; }
    }
}
