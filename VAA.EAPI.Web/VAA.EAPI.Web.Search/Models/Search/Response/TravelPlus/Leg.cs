using System;
/**
* The flight search leg
*
* @author Gareth Williams
*/
namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus
{
    public class Leg
    {

        public Airport origin { get; set; }

        public Airport destination { get; set; }

        public string duration { get; set; }

        public DateTime departure { get; set; }

        public DateTime arrival { get; set; }

        public string stopOver { get; set; }

        public bool isDominantLeg { get; set; }
    }
}
