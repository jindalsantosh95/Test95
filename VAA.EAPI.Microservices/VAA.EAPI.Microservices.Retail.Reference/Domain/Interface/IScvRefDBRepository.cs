using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Models;

namespace VAA.EAPI.Microservices.Retail.Reference.Domain.Interface
{
    public interface IScvRefDBRepository
    {
        public Task<IEnumerable<PartnerProfileDetails>> GetPartnerCodeDetails();
        public Task<IEnumerable<ProductDetails>> GetProductsDetails();        
        public Task<IEnumerable<CountryDetails>> GetCountryCodeDetails();
        public Task<IEnumerable<TierThresholdValues>> GetTierThresholdDetails();
        public Task<int> GetDistanceDetails(Distance distance);
        public Task<IEnumerable<BookingDetails>> GetBookingDetails(FlownDetails flownDetails);
        public Task<PreValidationPartnerData> GetBookingDetailsV2(FlownDetails flownDetails);
        public Task<NonVSRetroCabinClassDetails> GetCabinClassForNonVSRetro(NonVSRetroFlightDetails flightDetails);
        public Task<DistanceAndCountryCodeResponse> GetDistanceWithCountryCode(Distance distance);

    }
}
