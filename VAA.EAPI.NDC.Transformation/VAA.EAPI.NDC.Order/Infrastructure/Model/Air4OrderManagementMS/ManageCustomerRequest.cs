using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.Air4OrderManagementMS
{
    public class ManageCustomerRequest
    {
        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public string RLOC { get; set; }
        public IEnumerable<Customer> CustomerDetails { get; set; }
        public IEnumerable<OrderInstructions> OrderInstructions { get; set; }
    }
}
