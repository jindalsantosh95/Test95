using System.Collections.Generic;
/**
* The flight search request
*
* @author Gareth Williams
*/
namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus
{
    public class Request
    {

        public IEnumerable<Slice> slices { get; set; }

        public IEnumerable<RequestPassengerType> passengers { get; set; }

        public Descriptor descriptors { get; set; }
    }
}

