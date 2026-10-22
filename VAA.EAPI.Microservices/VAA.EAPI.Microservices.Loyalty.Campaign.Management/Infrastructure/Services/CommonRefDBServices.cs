using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignSearch;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignTypeRetrieval;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionCampaignSearch;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionSearch;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Models;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services.Interface;
using SqlParameter = Microsoft.Data.SqlClient.SqlParameter;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services
{
    public class CommonRefDBServices : ICommonRefDBServices
    {
        private readonly CommonReferenceDataContext _commonReferenceDataContext;
        private readonly ILogger<CommonRefDBServices> _logger;

        public CommonRefDBServices(CommonReferenceDataContext commonReferenceDataContext, ILogger<CommonRefDBServices> logger)
        {
            _commonReferenceDataContext = commonReferenceDataContext ?? throw new ArgumentNullException(nameof(commonReferenceDataContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Models.Campaign> GetAllCampaignDetails(string campaignId)
        {
            try
            {
                return await _commonReferenceDataContext.Campaigns.FirstOrDefaultAsync(x => x.RowId == campaignId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetAllCampaignDetails failed due to Exception");
                throw;
            }
        }
        public async Task<string> GetPromotionsDetail(string campaignId)
        {
            try
            {
                return (await _commonReferenceDataContext.CampaignsPromotionsMapping.FirstOrDefaultAsync(x => x.CampaignId == campaignId))?.PromotionId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetPromotionsDetail failed due to Exception");
                throw;
            }
        }
        public async Task<string> UpdateCampaignDetails(string campaignId, string campaignCode, string startDate, string endDate, string campaignName, string description, string linkToTemplate, bool registrationRequiredFlag, string status, string type, string initiatedBy)
        {
            try
            {
                string sql = Constants.Application.CampaignUpdateQuery;
                List<SqlParameter> sqlParameters = new List<SqlParameter>
            {
                new SqlParameter { ParameterName = "@CampaignId", Value = campaignId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 60 },
                new SqlParameter { ParameterName = "@CampaignName", Value = campaignName ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 200 },
                new SqlParameter { ParameterName = "@StartDate", Value = startDate ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.DateTime },
                new SqlParameter { ParameterName = "@EndDate", Value = endDate ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.DateTime},
                new SqlParameter { ParameterName = "@CampaignCode", Value = campaignCode ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 60 },
                new SqlParameter { ParameterName = "@Status", Value = status ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 60 },
                new SqlParameter { ParameterName = "@Type", Value = type ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 200 },
                new SqlParameter { ParameterName = "@Description", Value = description ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 550 },
                new SqlParameter { ParameterName = "@RegistrationRequiredFlag", Value = registrationRequiredFlag, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size =1 },
                new SqlParameter { ParameterName = "@LinkToTemplate", Value = linkToTemplate ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 4000 },
                new SqlParameter { ParameterName = "@InitiatedBy", Value = initiatedBy ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 50 },
                new SqlParameter { ParameterName = "@Result", Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int },
             };
                await _commonReferenceDataContext.Database.ExecuteSqlRawAsync(sql, sqlParameters.ToArray());
                return (int)sqlParameters[Constants.Application.UpdateResultNumber].Value == 1 ? Constants.Application.Success : ((int)sqlParameters[Constants.Application.UpdateResultNumber].Value == 0 ? Constants.Application.InvalidCampaignCode : Constants.Application.CampaignCodeAlreadyUsed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"UpdateCampaignDetails failed due to Exception");
                throw;
            }
        }

        public async Task<string> CreateNewCampaign(string campaignCode, string startDate, string endDate, string campaignName, string description, string linkToTemplate, bool registrationRequiredFlag, string status, string type, string initiatedBy)
        {
            try
            {
                string sql = Constants.Application.CampaignCreateQuery;
                List<SqlParameter> sqlParameters = new List<SqlParameter>
            {
                new SqlParameter { ParameterName = "@CampaignName", Value = campaignName ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 200 },
                new SqlParameter { ParameterName = "@StartDate", Value = startDate ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.DateTime },
                new SqlParameter { ParameterName = "@EndDate", Value = endDate ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.DateTime},
                new SqlParameter { ParameterName = "@CampaignCode", Value = campaignCode ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 60 },
                new SqlParameter { ParameterName = "@Status", Value = status ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 60 },
                new SqlParameter { ParameterName = "@Type", Value = type ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 200 },
                new SqlParameter { ParameterName = "@Description", Value = description ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 550 },
                new SqlParameter { ParameterName = "@RegistrationRequiredFlag", Value = registrationRequiredFlag, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size =1 },
                new SqlParameter { ParameterName = "@LinkToTemplate", Value = linkToTemplate ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 4000 },
                new SqlParameter { ParameterName = "@InitiatedBy", Value = initiatedBy ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 50 },
                new SqlParameter { ParameterName = "@Result", Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int },
             };
                await _commonReferenceDataContext.Database.ExecuteSqlRawAsync(sql, sqlParameters.ToArray());
                return (int)sqlParameters[Constants.Application.CreateResultNumber].Value == 1 ? Constants.Application.CreatedSuccessfully : ((int)sqlParameters[Constants.Application.CreateResultNumber].Value == 0 ? Constants.Application.DuplicateCampaign : ((int)sqlParameters[Constants.Application.CreateResultNumber].Value == 2 ? Constants.Application.InvalidCampaignType : Constants.Application.CreateFailed));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"CreateNewCampaign failed due to Exception");
                throw;
            }
        }
        public async Task<string> InsertCampaignPromtionLink(string campaignId, string promotionId, string createdBy)
        {
            try
            {
                string sql = Constants.Application.CampaignPromotionLinkQuery;
                List<SqlParameter> sqlParameters = new List<SqlParameter>
            {
                new SqlParameter { ParameterName = "@CampaignId", Value = campaignId ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 50 },
                new SqlParameter { ParameterName = "@PromotionId", Value = promotionId ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 50 },
                new SqlParameter { ParameterName = "@CreatedBy", Value = createdBy ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 100 },
                new SqlParameter { ParameterName = "@LastUpdatedBy", Value = createdBy ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 100},
                new SqlParameter { ParameterName = "@Result", Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int }
             };
                await _commonReferenceDataContext.Database.ExecuteSqlRawAsync(sql, sqlParameters.ToArray());
                return (int)sqlParameters[4].Value == 1 ? Constants.Application.Success : (int)sqlParameters[4].Value == 2 ? Constants.Application.InvalidPromotionId : (int)sqlParameters[4].Value == 3 ? Constants.Application.InvalidCampaignCode : Constants.Application.LinkingAlreadyExist;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"InsertCampaignPromtionLink failed due to Exception");
                throw;
            }
        }

        public async Task<Result<Response<CampaignTypeResponse>>> GetCampaignTypeList()
        {
            try
            {
                var campaignTypeList = await _commonReferenceDataContext.ListOfValues?.Where(x => x.Type == Constants.Application.CampaignType && x.SubType.ToUpper() == Constants.Application.CampaignSubType.ToUpper() && x.Active == Constants.Application.ActiveList).ToListAsync();
                if (campaignTypeList != null)
                {
                    CampaignTypeResponse campaignTypeResponse = new CampaignTypeResponse();
                    campaignTypeResponse.CampaignTypes = new List<string>();

                    foreach (var type in campaignTypeList)
                    {
                        if (type.Value != null)
                        {
                            campaignTypeResponse.CampaignTypes.Add(type.Value);
                        }

                    }

                    _logger.LogInformation("Fecth Campaign Type from Reference DB is Success");
                    return new Response<CampaignTypeResponse>
                    {
                        Status = Status.OK,
                        Value = campaignTypeResponse
                    };
                }
                else
                {
                    _logger.LogInformation("Fecth Campaign Type from Reference DB is Failed");
                    return Result.Failure<Response<CampaignTypeResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetCampaignTypeList failed due to Exception");
                throw;
            }

        }
        public async Task<string> UpdatePromotionDetails(string promotionId, string promotionName, string partnerName, string startDate, string endDate, string active, string campaignLinked, string initiatedBy)
        {
            try
            {
                string sql = Constants.Application.PromotionUpdateQuery;
                List<SqlParameter> sqlParameters = new List<SqlParameter>
            {
                new SqlParameter { ParameterName = "@PromotionId", Value = promotionId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 60 },
                new SqlParameter { ParameterName = "@PromotionName", Value = promotionName ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 200 },
                new SqlParameter { ParameterName = "@StartDate", Value = startDate ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.DateTime },
                new SqlParameter { ParameterName = "@EndDate", Value = endDate ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.DateTime},
                new SqlParameter { ParameterName = "@Active", Value = active ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 60 },
                new SqlParameter { ParameterName = "@PartnerName", Value = partnerName ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 60 },
                new SqlParameter { ParameterName = "@InitiatedBy", Value = initiatedBy ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 50 },
                new SqlParameter { ParameterName = "@Result", Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int },
             };
                await _commonReferenceDataContext.Database.ExecuteSqlRawAsync(sql, sqlParameters.ToArray());
                return (int)sqlParameters[Constants.Application.PromotionUpdateResultNumber].Value == 1 ? Constants.Application.Success : ((int)sqlParameters[Constants.Application.PromotionUpdateResultNumber].Value == 0 ? Constants.Application.InvalidPromotionId : ((int)sqlParameters[Constants.Application.PromotionUpdateResultNumber].Value == 2 ? Constants.Application.PromotionNameAlreadyUsed : Constants.Application.CreateFailed));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"UpdatePromotionDetails failed due to Exception");
                throw;
            }
        }

        public async Task<Result<Response<PromotionSearchResponse>>> GetPromotionSearchList(string PromotionId, string PromotionName, string StartDateF, string EndDateF, string StartDateT, string EndDateT, string PartnerName, string Active)
        {
            try
            {
                string sql = Constants.Application.PromotionSearchQuery;
                List<SqlParameter> sqlParameters = new List<SqlParameter>
            {
                new SqlParameter { ParameterName = "@PromotionId", Value = string.IsNullOrEmpty(PromotionId) ? DBNull.Value : PromotionId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 60 },
                new SqlParameter { ParameterName = "@PromotionName", Value = string.IsNullOrEmpty(PromotionName) ? DBNull.Value : PromotionName, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 60 },
               new SqlParameter { ParameterName = "@StartDateF", Value = string.IsNullOrEmpty(StartDateF) ? DBNull.Value : StartDateF, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.DateTime},
                new SqlParameter { ParameterName = "@EndDateF", Value = string.IsNullOrEmpty(EndDateF) ? DBNull.Value : EndDateF, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.DateTime},
                new SqlParameter { ParameterName = "@StartDateT", Value = string.IsNullOrEmpty(StartDateT) ? DBNull.Value : StartDateT, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.DateTime},
                new SqlParameter { ParameterName = "@EndDateT", Value = string.IsNullOrEmpty(EndDateT) ? DBNull.Value : EndDateT, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.DateTime},
                new SqlParameter { ParameterName = "@PartnerName", Value = string.IsNullOrEmpty(PartnerName) ? DBNull.Value : PartnerName, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 60 },
                new SqlParameter { ParameterName = "@Active", Value = string.IsNullOrEmpty(Active) ? DBNull.Value :  Active , Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 60 },
             };


                var promotionList = await _commonReferenceDataContext.Promotions.FromSqlRaw(sql, sqlParameters.ToArray()).ToListAsync();
                if (promotionList.Count > 0)
                {
                    PromotionSearchResponse PromotionResponse = new PromotionSearchResponse();
                    PromotionResponse.Promotions = promotionList;
                    return new Response<PromotionSearchResponse>
                    {
                        Status = Status.OK,
                        Value = PromotionResponse
                    };
                }
                else
                {
                    _logger.LogInformation("Fecth promotions from Reference DB is Failed");
                    return Result.Failure<Response<PromotionSearchResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.NotFound, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetPromotionSearchList failed due to Exception");
                throw;
            }
        }
        public async Task<string> DeleteCampaignPromtionLink(string campaignId, string promotionId)
        {
            try
            {
                string sql = Constants.Application.CampaignPromotionDelinkQuery;
                List<SqlParameter> sqlParameters = new List<SqlParameter>
            {
                new SqlParameter { ParameterName = "@CampaignId", Value = campaignId ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 50 },
                new SqlParameter { ParameterName = "@PromotionId", Value = promotionId ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 50 },
                new SqlParameter { ParameterName = "@Result", Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int }
             };
                await _commonReferenceDataContext.Database.ExecuteSqlRawAsync(sql, sqlParameters.ToArray());
                return (int)sqlParameters.LastOrDefault().Value == 1 ? Constants.Application.Success : (int)sqlParameters.LastOrDefault().Value == 2 ? Constants.Application.InvalidPromotionId : (int)sqlParameters.LastOrDefault().Value == 3 ? Constants.Application.InvalidCampaignCode : Constants.Application.LinkingNotExist;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"DeleteCampaignPromtionLink failed due to Exception");
                throw;
            }
        }
        public async Task<string> CreateNewPromotion(string promotionName, string partnerName, string startDate, string endDate, string active, string initiatedBy)
        {
            try
            {
                string sql = Constants.Application.PromotionCreateQuery;
                List<SqlParameter> sqlParameters = new List<SqlParameter>
                {
                    new SqlParameter { ParameterName = "@PromotionName", Value = promotionName ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 200 },
                    new SqlParameter { ParameterName = "@StartDate", Value = startDate ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.DateTime },
                    new SqlParameter { ParameterName = "@EndDate", Value = endDate ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.DateTime},
                    new SqlParameter { ParameterName = "@Active", Value = active ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 60 },
                    new SqlParameter { ParameterName = "@PartnerName", Value = partnerName ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 60 },
                    new SqlParameter { ParameterName = "@InitiatedBy", Value = initiatedBy ?? (object) DBNull.Value, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 50 },
                    new SqlParameter { ParameterName = "@Result", Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int },
                 };
                await _commonReferenceDataContext.Database.ExecuteSqlRawAsync(sql, sqlParameters.ToArray());
                return (int)sqlParameters[Constants.Application.PromotionCreateResultNumber].Value == 1 ? Constants.Application.Success : ((int)sqlParameters[Constants.Application.PromotionCreateResultNumber].Value == 0 ? Constants.Application.PromotionNameAlreadyUsed : ((int)sqlParameters[Constants.Application.PromotionCreateResultNumber].Value == 2 ? Constants.Application.InvalidPartnerName : Constants.Application.CreateFailed));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"CreateNewPromotion failed due to Exception");
                throw;
            }
        }
        public async Task<Result<Response<PromotionCampaignSearchResponse>>> GetPromotionCampaignSearchList(string PromotionId)
        {
            try
            {
                string sql = Constants.Application.PromotionCampaignSearchQuery;
                List<SqlParameter> sqlParameters = new List<SqlParameter>
                {
                new SqlParameter { ParameterName = "@PromotionId", Value = PromotionId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 60 },
                };


                var promotionList = await _commonReferenceDataContext.PromotionCampaigns.FromSqlRaw(sql, sqlParameters.ToArray()).ToListAsync();
                if (promotionList.Count > 0)
                {
                    PromotionCampaignSearchResponse PromotionCampaignResponse = new PromotionCampaignSearchResponse();
                    PromotionCampaignResponse.PromotionCampaigns = promotionList;
                    return new Response<PromotionCampaignSearchResponse>
                    {
                        Status = Status.OK,
                        Value = PromotionCampaignResponse
                    };
                }
                else
                {
                    _logger.LogInformation("Fecth campaings from Reference DB is Failed");
                    return Result.Failure<Response<PromotionCampaignSearchResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.NotFound, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetPromotionCampaignSearchList failed due to Exception");
                throw;
            }
        }
        public async Task<List<PromotionsDownload>> GetPromotionslist()
        {
            try
            {
                var product = await _commonReferenceDataContext.PromotionsDownload.FromSqlRaw(Constants.Application.PromotionsList).ToListAsync();

                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"PromotionsListDownload failed due to Exception");
                throw;
            }
        }

        public async Task<Result<Response<CampaignSearchResponse>>> GetCampaignSearchList(string campaignId, string campaignName, string startDateF, string endDateF, string startDateT, string endDateT, string campaignCode, string status, string type)
        {
            try
            {
                string sql = Constants.Application.CampaignSearchQuery;
                List<SqlParameter> sqlParameters = new List<SqlParameter>
                {
                    new SqlParameter { ParameterName = "@Name", Value = string.IsNullOrEmpty(campaignName) ? DBNull.Value : campaignName, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 60 },
                    new SqlParameter { ParameterName = "@StartDateF", Value = string.IsNullOrEmpty(startDateF) ? DBNull.Value : startDateF, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.DateTime},
                    new SqlParameter { ParameterName = "@EndDateF", Value = string.IsNullOrEmpty(endDateF) ? DBNull.Value : endDateF, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.DateTime},
                    new SqlParameter { ParameterName = "@StartDateT", Value = string.IsNullOrEmpty(startDateT) ? DBNull.Value : startDateT, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.DateTime},
                    new SqlParameter { ParameterName = "@EndDateT", Value = string.IsNullOrEmpty(endDateT) ? DBNull.Value : endDateT, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.DateTime},
                    new SqlParameter { ParameterName = "@CampaignCode", Value = string.IsNullOrEmpty(campaignCode) ? DBNull.Value : campaignCode, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 60 },
                    new SqlParameter { ParameterName = "@Status", Value = string.IsNullOrEmpty(status) ? DBNull.Value :  status , Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 60 },
                    new SqlParameter { ParameterName = "@Type", Value = string.IsNullOrEmpty(type) ? DBNull.Value : type, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 60 },
                    new SqlParameter { ParameterName = "@CampaignId", Value = string.IsNullOrEmpty(campaignId) ? DBNull.Value : campaignId, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 60 }
                };


                var campaignList = await _commonReferenceDataContext.CampaignSearchData.FromSqlRaw(sql, sqlParameters.ToArray()).ToListAsync();
                if (campaignList.Count > 0)
                {
                    CampaignSearchResponse campaignResponse = new CampaignSearchResponse() { Campaigns = new List<CampaignData>() };

                    foreach (var camp in campaignList)
                    {

                        campaignResponse.Campaigns.Add(new CampaignData()
                        {
                            CampaignId = camp.CampaignId ?? string.Empty,
                            Name = camp.Name ?? string.Empty,
                            StartDate = camp.StartDate,
                            EndDate = camp.EndDate,
                            CampaignCode = camp.CampaignCode ?? string.Empty,
                            Status = camp.Status ?? string.Empty,
                            Type = camp.Type ?? string.Empty,
                            Description = camp.Description ?? string.Empty,
                            CampaignPurpose = camp.CampaignPurpose ?? string.Empty,
                            RegistrationRequired = camp.RegistrationRequired ?? string.Empty,
                            LinkToTemplate = camp.LinkToTemplate ?? string.Empty,
                            Promotion = new PromotionData()
                            {
                                PromotionId = camp.Promotion_Code ?? string.Empty,
                                PromotionName = camp.Promotion_Name ?? string.Empty,
                                StartDate = camp.Promotion_StartDate,
                                EndDate = camp.Promotion_EndDate,
                                PartnerName = camp.Partner_Name ?? string.Empty,
                                LastModifiedBy = camp.Promotion_LastUpdBy ?? string.Empty
                            },
                        });
                    }

                    return new Response<CampaignSearchResponse>
                    {
                        Status = Status.OK,
                        Value = campaignResponse,
                        IgnoreNullValues = false
                    };
                }
                else
                {
                    _logger.LogInformation("Fecth campaigns from Reference DB is Failed");
                    return Result.Failure<Response<CampaignSearchResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.NotFound, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fecth campaigns from Reference DB failed due to Exception");
                throw;
            }
        }
    }
}
