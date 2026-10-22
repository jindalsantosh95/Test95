using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Vred.Applications.Commands.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.ManageAccount;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services.Interface
{
    public interface IVredCustomerDetails
    {
        public Task<bool> Handle(UpdateVredMemberDetailsRequestDataPayload command, CancellationToken cancellationToken);
    }
}
