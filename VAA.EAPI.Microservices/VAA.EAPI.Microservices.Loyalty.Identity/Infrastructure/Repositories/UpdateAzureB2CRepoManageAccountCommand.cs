using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories
{
    public class UpdateAzureB2CRepoManageAccountCommand:IRequest<bool>
    {
        public ManageAccountAzureB2CUpdateRequest manageAccountAzureB2CUpdateRequest { get; set; }

        public UpdateAzureB2CRepoManageAccountCommand(ManageAccountAzureB2CUpdateRequest updateDocument)
        {
            manageAccountAzureB2CUpdateRequest = updateDocument;
        }
    }
}
