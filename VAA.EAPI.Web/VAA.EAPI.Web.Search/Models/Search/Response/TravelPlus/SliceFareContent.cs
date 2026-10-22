using System.Collections.Generic;
/**
* The flight search slice fare content
*
* @author Gareth Williams
*/
namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus
{
    public class SliceFareContent
    {

        public string cabinName { get; set; }

        public IEnumerable<SliceFareContentFeature> features { get; set; }

    }
}
