using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignAmexUpdate;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Interface
{
    public interface ICampaignAmexUpdateRequestCommand
    {
        Task<Result<Response<string>>> Handle(CampaginAmexUpdateRequest campaginAmexUpdateRequest, CancellationToken cancellationToken);
    }
}

