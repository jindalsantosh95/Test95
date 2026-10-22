using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Models.CustProfile
{
    public class CustomerProfileRS
    {
        public CustomerProfileResponse CustomerProfileResponse { get; set; }
        public IEnumerable<ErrorDetails> Error { get; set; }

    }
}
