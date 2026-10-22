using System.Collections.Generic;

namespace VAA.Facade.NDC.IFGFacade.Domain
{
    public class OrderRequest
    {
        public IEnumerable<Order> Orders { get; set; }
        public BasicInformation BasicInformation { get; set; }
        public IEnumerable<FormOfPayment> FormOfPayment { get; set; }
        public string TransactionId { get; set; }
    }
}
