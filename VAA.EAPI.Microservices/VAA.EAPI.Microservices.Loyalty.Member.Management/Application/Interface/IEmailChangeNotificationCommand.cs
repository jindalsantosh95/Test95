using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.EmailUpdate;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Interface
{
    public interface IEmailChangeNotificationCommand
    {
        Task<Result<Response<string>>> Handle(EmailChangeEvent emailChangeRequest, CancellationToken cancellationToken);

    }
}
