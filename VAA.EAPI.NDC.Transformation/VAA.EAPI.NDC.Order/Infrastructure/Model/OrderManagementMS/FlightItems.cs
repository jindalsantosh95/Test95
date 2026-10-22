using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.NDC.Order.Constants;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.OrderManagementMS
{
    public class FlightItems
    {        
        public ActionContextEnum? ActionContext { get; set; }
        public string ProductCode { get; set; }
        public string CurrentSeatAssignment { get; set; }
        public IEnumerable<Segment> Segments { get; set; }
    }
}
