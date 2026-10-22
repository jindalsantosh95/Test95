using VAA.EAPI.NDC.Order.Constants;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.Air4OrderManagementMS
{
    public class FlightItems
    {        
        public ActionContextEnum? ActionContext { get; set; }
        public string ProductCode { get; set; }
        public string CurrentSeatAssignment { get; set; }
        public Segment Segments { get; set; }
    }
}
