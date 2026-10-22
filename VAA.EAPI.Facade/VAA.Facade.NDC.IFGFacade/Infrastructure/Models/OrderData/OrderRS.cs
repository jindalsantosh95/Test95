using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.Facade.NDC.IFGFacade.Infrastructure.Models.OrderData
{
    public class OrderRS
    {
        public OrderDocument OrderDocument { get; set; }
        public IEnumerable<ErrorDetails> Errors { get; set; }
    }
}
