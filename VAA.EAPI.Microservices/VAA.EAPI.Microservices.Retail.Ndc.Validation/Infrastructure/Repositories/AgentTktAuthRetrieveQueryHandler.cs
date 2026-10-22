using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Models;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Repositories
{
    public class AgentTktAuthRetrieveQueryHandler : IRequestHandler<AgentTktAuthRetrieveQuery, AgentTktAuthResponse>
    {
        private readonly IAgentTktAuthSvc _agentTktAuthSvc;
        private readonly IDistributedCacheService _distributedCacheService;
        private readonly IAgencyDataService _agencyDataService;

        public AgentTktAuthRetrieveQueryHandler(IAgentTktAuthSvc agentTktAuthService, IDistributedCacheService distributedCacheService, IAgencyDataService agencyDataService)
        {
            _agentTktAuthSvc = agentTktAuthService ?? throw new ArgumentNullException(nameof(agentTktAuthService));
            _distributedCacheService = distributedCacheService;
            _agencyDataService = agencyDataService;
        }

        public async Task<AgentTktAuthResponse> Handle(AgentTktAuthRetrieveQuery request, CancellationToken cancellationToken)
        {
            var absoluteExpirationTime = ConfigReader.Get<TimeSpan>(Constants.Environment.AgencyAbsoluteExpirationInDay);

            var agentTktAuthResponse = await _agentTktAuthSvc.UpdateAgentTktAuthResponse(request.RequestData?.Parties?.Participants?.First(x => x.PartyType == Participant.PartyTypeEnum.Seller).AgencyID, request.RequestData.RequestAgencyType, request.RequestData.Environment);

            if (string.IsNullOrEmpty(agentTktAuthResponse.Error))
            {
                var cacheKey = CommonHelper.GenerateCacheKey(request.RequestData);
                await _distributedCacheService.Remove(cacheKey);
                var agencyData = MapAgencyData(agentTktAuthResponse);

                if (agencyData != null && agencyData.Any())
                    await _distributedCacheService.Set(cacheKey, agencyData, false, TimeSpan.Zero, absoluteExpirationTime);
            }

            return agentTktAuthResponse;
        }

       

        private IEnumerable<AgencyData> MapAgencyData(AgentTktAuthResponse agentTktAuthResponse)
        {
            return agentTktAuthResponse.IATAObj.Select(ita => new AgencyData()
            {
                AccountCode = ita.AccountCode,
                AgencyId = ita.AgencyId,
                AgencyIdType = ita.AgencyIdType,
                AirlineTicketingAuthorityInd = ita.AirlineTicketingAuthorityInd,
                BspActiveStatusInd = ita.BspActiveStatusInd,
                BusinessType = ita.BusinessType,
                CardFopAllowedInd = ita.CardFopAllowedInd,
                CashFopAllowedInd = ita.CashFopAllowedInd,
                CompanyName = ita.CompanyName,
                CorporateAccountCode = ita.CorporateAccountCode,
                CorporateId = ita.CorporateId,
                CountryCode = ita.CountryCode,
                EasyPayFopAllowedInd = ita.EasyPayFopAllowedInd,
                Environment = ita.Environment,
                ExpiryDateKey = ita.ExpiryDateKey,
                ExpiryDateKey2 = ita.ExpiryDateKey2,
                KeyId = ita.KeyId,
                KeyStatus = ita.KeyStatus,
                KeyType = ita.KeyType,
                LastUpdate = ita.LastUpdate,
                LastUpdateIATA = ita.LastUpdateIATA,
                OrgId = ita.OrgId,
                ParentId = ita.ParentId,
                PciDssComplianceInd = ita.PciDssComplianceInd,
                RequestAgencyId = ita.RequestAgencyId,
                RequestAgencyIdType = ita.RequestAgencyIdType,
                RequestOrgName = ita.RequestOrgName,
                RequestOrgStatus = ita.RequestOrgStatus,
                RowId = ita.RowId,
                Status = ita.Status,
                SubscriberKey = ita.SubscriberKey,
                SubscriberKey2 = ita.SubscriberKey2,

            });
        }

    }
}
