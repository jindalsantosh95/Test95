using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.CommonRefDB.Models;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.CommonRefDB.Interface
{
    public interface ICommonRefDBServices
    {

        public Task<IEnumerable<TitleMaster>> GetAllTitlesAsync();
        public Task<IEnumerable<MediaCodes>> GetAllMediaCodesAsync();
        public Task<IEnumerable<ListOfValues>> GetAllCounryCodesAsync();
        public Task<IEnumerable<PartnerProfile>> GetPartnerCodeAsync();
        public Task<IEnumerable<PartnerStatusCodes>> GetPartnerStatusCodeAsync();
        public Task<IEnumerable<ListOfValues>> GetMetroAreaAsync(string countryCode);
        public Task<ListOfValues> GetTitleValueAsync(string title);
    }
}
