using System.Collections.Generic;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Models
{
    public class SpecialServiceInfo
    {
        public IEnumerable<SecureFlight> SecureFlight { get; set; }
        public IEnumerable<Service> Service { get; set; }
        public IEnumerable<AdvancePassenger> AdvancePassenger { get; set; }
    }
}
