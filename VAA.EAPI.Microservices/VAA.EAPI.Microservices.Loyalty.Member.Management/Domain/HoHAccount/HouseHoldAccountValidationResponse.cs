using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.HoHAccount
{
    public class HouseHoldAccountValidationResponse
    {
        public string ValidationResponse { get; set; }
        public IEnumerable<ErrorDetails> ErrorDetails { get; set; }
    }
}
