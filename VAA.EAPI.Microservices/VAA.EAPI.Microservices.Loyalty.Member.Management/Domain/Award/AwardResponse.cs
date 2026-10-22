using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Award
{
    public class AwardResponse
    {
        public IEnumerable<Award> Awards { get; set; }
        public string CustomerId { get; set; }
    }
}
