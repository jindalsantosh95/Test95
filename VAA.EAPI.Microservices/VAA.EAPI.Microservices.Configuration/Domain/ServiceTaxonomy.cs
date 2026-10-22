using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Configuration.Domain
{
    public class ServiceTaxonomyDTO
    {
        public string ServiceCode { get; set; }
        public bool IncludeInd { get; set; }
    }
}
