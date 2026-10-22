using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Models
{
    public class AgentTktAuthResponse
    {
        public IEnumerable<AgentTktAuthIATAObjResponse> IATAObj { get; set; }
        public bool HasError { get; set; }
        public string Error { get; set; }
    }
}
