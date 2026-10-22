using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Vred;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Vred
{
    public class UpdateVredCustomerDetailsRS
    {
        public UpdateVredCustomerDetailsQueryResponse UpdateVredCustomerDetailsResponse { get; set; }
        public IEnumerable<ErrorDetails> Error { get; set; }
    }
}