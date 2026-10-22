using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services.Interface
{
    public interface IAssociatedMemberListDownloadService
    {
        public Task<string> CreateXlStatementAndUploadToBlob(string campaignCode, List<AssociatedMemberData> associatedMemberList);
    }
}
