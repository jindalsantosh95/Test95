using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.CommonReferenceDetails;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Interface.IRepository.CommonRefDB
{
    public interface ICommonRefDBRepository
    {
        public Task<List<TitleDetails>> GetTitleDetails();
        public Task<List<MediaCodeDetails>> GetMediaCodeDetails();
        public Task<List<CountryDetails>> GetCountryCodeDetails();
        public Task<List<PartnerProfileDetails>> GetPartnerCodeDetails();
        public Task<List<PartnerStatusCodeDetails>> GetPartnerStatusCodeDetails();
        public Task<MetroAreaDetails> GetMetroArea(string countryCode);
        public Task<string> GetTitleValue(string title);
    }
}
