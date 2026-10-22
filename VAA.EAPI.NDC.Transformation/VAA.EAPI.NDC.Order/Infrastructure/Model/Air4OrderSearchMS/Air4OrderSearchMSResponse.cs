using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.Air4OrderSearchMS
{
    public class Air4OrderSearchMSResponse
    {
        public OrderSearchResponse OrderSearchResponse { get; set; }
        public IEnumerable<ErrorDetails> ErrorDetails { get; set; }
    }
}
