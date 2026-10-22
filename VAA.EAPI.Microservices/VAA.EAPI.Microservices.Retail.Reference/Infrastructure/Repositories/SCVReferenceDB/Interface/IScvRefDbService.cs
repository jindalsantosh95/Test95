using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Reference.Domain;
using VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Models;

namespace VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Interface
{
    public interface IScvRefDbService
    {
        public Task<IEnumerable<PartnerProfile>> GetPartnerCodeAsync();
        public Task<IEnumerable<Product>> GetProductsAsync();        
        public Task<IEnumerable<ListOfValues>> GetAllCountryCodesAsync();
        public Task<IEnumerable<MemberAccountReferenceData>> GetTierThresholdAsync();
        public Task<IEnumerable<DistanceDetails>> GetDistanceAsync(Distance distanceDetails);
        public Task<IEnumerable<PartnerBooking>> GetPartnerBookingAsync(FlownDetails flownDetails);
        public Task<IEnumerable<PartnerBookingV2>> GetPartnerBookingV2Async(FlownDetails flownDetails);
        public Task<NonVSRetroCabinClassDetails> GetCabinClassNonVSRetroAsync(NonVSRetroFlightDetails flightDetails);
        public Task<DistanceAndCountryCodeResponse> GetDistanceWithCountryCode(Distance distance);
    }
}
