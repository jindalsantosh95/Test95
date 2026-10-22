using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.OrderManagementMS
{
    public class ManageCustomerRequest
    {
        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public string RLOC { get; set; }
        public bool AmendApis { get; set; }
        public bool TicketedNameCorrection { get; set; }
        public bool UnTicketedNameCorrection { get; set; }
        public IEnumerable<Customer> CustomerDetails { get; set; }
        public IEnumerable<OrderInstructions> OrderInstructions { get; set; }
        public IEnumerable<Segment> SegmentDetails { get; set; }
    }
}
