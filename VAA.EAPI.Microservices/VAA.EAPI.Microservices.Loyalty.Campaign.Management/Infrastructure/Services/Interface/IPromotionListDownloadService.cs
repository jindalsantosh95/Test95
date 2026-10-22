using CSharpFunctionalExtensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services.Interface
{
    public interface IPromotionListDownloadService
    {
        public Task<Result<Response<string>>> CreateXlStatementAndUploadToBlob(List<PromotionsDownload> promotionList);
    }
}
