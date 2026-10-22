using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.CommonRefDB.Interface;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.CommonRefDB.Models;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.CommonRefDB.Models;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.CommonRefDB.Services
{
    public class CommonRefDBServices : ICommonRefDBServices
    {
        private readonly CommonReferenceDataContext _commonReferenceDataContext;
        private readonly ICacheService _iCacheService;

        public CommonRefDBServices(CommonReferenceDataContext commonReferenceDataContext, ICacheService iCacheService)
        {
            _commonReferenceDataContext = commonReferenceDataContext;
            _commonReferenceDataContext.ChangeTracker.AutoDetectChangesEnabled = false;
            _iCacheService = iCacheService;
        }

        public async Task<IEnumerable<TitleMaster>> GetAllTitlesAsync()
        {
            IEnumerable<TitleMaster> titleMasterData = await _iCacheService.GetCacheValue<IEnumerable<TitleMaster>>(Constants.Application.TitleMasterCacheKey);
            if (titleMasterData == null)
            {
                titleMasterData = _commonReferenceDataContext.TitleMaster.ToList();
                await _iCacheService.SetCacheValue(Constants.Application.TitleMasterCacheKey, titleMasterData, true);
            }
            return titleMasterData;

        }

        public async Task<IEnumerable<MediaCodes>> GetAllMediaCodesAsync()
        {
            IEnumerable<MediaCodes> mediaCodes = await _iCacheService.GetCacheValue<IEnumerable<MediaCodes>>(Constants.Application.MediaCodeCacheKey);
            if (mediaCodes == null)
            {
                mediaCodes = _commonReferenceDataContext.MediaCodes.ToList();
                await _iCacheService.SetCacheValue(Constants.Application.MediaCodeCacheKey, mediaCodes, true);
            }
            return mediaCodes;

        }

        public async Task<IEnumerable<ListOfValues>> GetAllCounryCodesAsync()
        {
            IEnumerable<ListOfValues> counryCodeData = await _iCacheService.GetCacheValue<IEnumerable<ListOfValues>>(Constants.Application.CountryCacheKey);
            if (counryCodeData == null)
            {
                counryCodeData = _commonReferenceDataContext.ListOfValues.Where(x => x.Type.Is("COUNTRY")).ToList();
                await _iCacheService.SetCacheValue(Constants.Application.CountryCacheKey, counryCodeData, true);
            }
            return counryCodeData;

        }

        public async Task<IEnumerable<PartnerProfile>> GetPartnerCodeAsync()
        {
            IEnumerable<PartnerProfile> partnerCodeData = await _iCacheService.GetCacheValue<IEnumerable<PartnerProfile>>(Constants.Application.PartnerCodeCacheKey);
            if (partnerCodeData == null)
            {
                partnerCodeData = _commonReferenceDataContext.PartnerProfile.ToList();
                await _iCacheService.SetCacheValue(Constants.Application.PartnerCodeCacheKey, partnerCodeData, true);
            }
            return partnerCodeData;

        }

        public async Task<IEnumerable<PartnerStatusCodes>> GetPartnerStatusCodeAsync()
        {
            IEnumerable<PartnerStatusCodes> partnerStatusCodeData = await _iCacheService.GetCacheValue<IEnumerable<PartnerStatusCodes>>(Constants.Application.PartnerStatusCacheKey);
            if (partnerStatusCodeData == null)
            {
                partnerStatusCodeData = _commonReferenceDataContext.PartnerStatusCodes.ToList();
                await _iCacheService.SetCacheValue(Constants.Application.PartnerStatusCacheKey, partnerStatusCodeData, true);
            }
            return partnerStatusCodeData;

        }

        public async Task<IEnumerable<ListOfValues>> GetMetroAreaAsync(string countryCode)
        {
            IEnumerable<ListOfValues> counryCodeData = await GetAllCounryCodesAsync();
            return counryCodeData.Where(x => x.Type.Is("COUNTRY") && x.Name == countryCode && x.SortOrder > 1000);

        }

        public async Task<ListOfValues> GetTitleValueAsync(string title)
        {
            IEnumerable<ListOfValues> titleValueList = await _iCacheService.GetCacheValue<IEnumerable<ListOfValues>>(Constants.Application.TitleValueCacheKey);
            if (titleValueList == null)
            {
                titleValueList = _commonReferenceDataContext.ListOfValues.Where(x => x.Type == "VA_TPF_TITLE").ToList();
                await _iCacheService.SetCacheValue(Constants.Application.TitleValueCacheKey, titleValueList, true);
            }
            return titleValueList.FirstOrDefault(x => string.Equals(x.Name, title, StringComparison.OrdinalIgnoreCase));

        }

    }
}
