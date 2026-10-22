using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CommonReferenceDetails;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IRepository.CommonRefDB;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.CommonRefDB.Interface;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.CommonRefDB.Mapper;


namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.CommonRefDB.Repository
{
    public class CommonRefDBRepository : ICommonRefDBRepository
    {
        private readonly ICommonRefDBServices _commonRefDBServices;
        private readonly ILogger<CommonRefDBRepository> _logger;

        public CommonRefDBRepository(ICommonRefDBServices commonRefDBServices, ILogger<CommonRefDBRepository> logger)
        {
            _logger = logger;
            _commonRefDBServices = commonRefDBServices;
        }
        public async Task<List<TitleDetails>> GetTitleDetails()
        {
            _logger.LogInformation($"Title data fetched successfully");
            return TitleDataMapper.MapTitleData(await _commonRefDBServices.GetAllTitlesAsync());
        }
        public async Task<List<MediaCodeDetails>> GetMediaCodeDetails()
        {
            _logger.LogInformation($"Media code data fetched successfully");
            return MediaCodeDataMapper.MapMediaCodeData(await _commonRefDBServices.GetAllMediaCodesAsync());

        }
        public async Task<List<CountryDetails>> GetCountryCodeDetails()
        {


            _logger.LogInformation($"Country code data fetched successfully");
            return CountryDataMapper.MapCountryCodeData(await _commonRefDBServices.GetAllCounryCodesAsync());

        }
        public async Task<List<PartnerProfileDetails>> GetPartnerCodeDetails()
        {
            _logger.LogInformation($"Partner Code and Id datas Fetched successfully");
            return PartnerCodeMapper.MapPartnerCodeData(await _commonRefDBServices.GetPartnerCodeAsync());

        }
        public async Task<List<PartnerStatusCodeDetails>> GetPartnerStatusCodeDetails()
        {
            _logger.LogInformation($"Partner Status Code data Fetched successfully");
            return PartnerStatusCodeMapper.MapPartnerStatusCodeData(await _commonRefDBServices.GetPartnerStatusCodeAsync());

        }
        public async Task<MetroAreaDetails> GetMetroArea(string countryCode)
        {
            _logger.LogInformation($"Metro area data Fetched successfully");
            return MetroAreaMapper.MapCountryCodeData(await _commonRefDBServices.GetMetroAreaAsync(countryCode), countryCode);

        }
        public async Task<string> GetTitleValue(string title)
        {
            var result = await _commonRefDBServices.GetTitleValueAsync(title);
            return result?.Value;
        }
    }
}
