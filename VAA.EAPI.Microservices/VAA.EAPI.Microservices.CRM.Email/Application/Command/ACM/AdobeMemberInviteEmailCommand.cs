using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.CRM.Email.Domain;

namespace VAA.EAPI.Microservices.CRM.Email.Application.Command.ACM
{
    public class AdobeMemberInviteEmailCommand : IRequest<Response<string>>
    {
        public string Email { get; set; }
        public string EmailTemplate { get; set; }
        public AdobeMemberInviteRequest adobeInviteMemberRequest { get; set; }

    }
}
