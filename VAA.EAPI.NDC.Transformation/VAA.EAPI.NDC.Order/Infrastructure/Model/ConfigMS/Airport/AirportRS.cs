using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ConfigMS.Airport;

namespace VAA.EAPI.NDC.Order.Infrastructure.ConfigMS.Airport
{
    public class AirportRS
    {
        public AirportLocationResponse AirportLocationResponse { get; set; }
        public IEnumerable<ErrorDetails> Error { get; set; }
    }
}
