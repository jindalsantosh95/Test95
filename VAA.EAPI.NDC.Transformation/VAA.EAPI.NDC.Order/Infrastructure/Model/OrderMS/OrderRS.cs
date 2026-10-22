using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS
{
    public class OrderRS
    {
        public OrderDocument OrderDocument { get; set; }
        public IEnumerable<ErrorDetails> Errors { get; set; }

    }
}
