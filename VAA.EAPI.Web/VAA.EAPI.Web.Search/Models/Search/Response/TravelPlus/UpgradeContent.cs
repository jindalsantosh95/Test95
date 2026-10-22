using System.Collections.Generic;
/**
* The flight search upgrade content
*
* @author Gareth Williams
*/
namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus
{
    public class UpgradeContent
    {

        public string cabinName { get; set; }

        public string upgradeImage { get; set; }

        public IEnumerable<UpgradeContentFeature> features { get; set; }

    }
}
