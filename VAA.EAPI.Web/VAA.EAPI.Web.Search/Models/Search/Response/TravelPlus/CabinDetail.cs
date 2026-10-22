using System.Collections.Generic;
/**
* The flight search cabin detail
*
* @author Gareth Williams
*/
namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus
{
    public class CabinDetail
    {

        public IEnumerable<CabinDetailSegment> segments { get; set; }

        public string fareFamilyType { get; set; }

        public string operatedBy { get; set; }

        public string dominantCabinName { get; set; }

        public string title { get; set; }

        public string heroImageURI { get; set; }

        public IEnumerable<string> description { get; set; }

        public CabinInformationFeature keyFeatures { get; set; }

        public CabinInformationFeature genericFeatures { get; set; }

        public IEnumerable<string> disclaimers { get; set; }

    }
}
