using System;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Helper
{
    public static class CampaignHelper
    {
        public static List<ErrorDetails> GetErrorDetail(string errorCode)
        {
            switch (errorCode)
            {
                case Constants.Application.InvalidCampaignCode:
                    return ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidCampaignId), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath);
                case Constants.Application.CampaignCodeAlreadyUsed:
                    return ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.CampaignCodeAlreadyUsed), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath);
                case Constants.Application.InvalidCampaignType:
                    return ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidCampaignType), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath);
                case Constants.Application.PromotionNameAlreadyUsed:
                    return ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.PromotionNameAlreadyUsed), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath);
                case Constants.Application.InvalidPromotionId:
                    return ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidPromotionId), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath);
                case Constants.Application.LinkingAlreadyExist:
                    return ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.LinkingAlreadyExist), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath);
                case Constants.Application.InvalidPartnerName:
                    return ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidPartnerName), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath);
                case Constants.Application.LinkingNotExist:
                    return ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.LinkingNotExist), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath);
                default:
                    return ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.MiscellaneousTechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath);
            }
        }
    }
}
