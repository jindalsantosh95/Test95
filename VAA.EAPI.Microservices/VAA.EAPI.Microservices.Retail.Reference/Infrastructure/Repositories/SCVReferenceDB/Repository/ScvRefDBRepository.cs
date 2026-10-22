using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Reference.Domain;
using VAA.EAPI.Microservices.Retail.Reference.Domain.Interface;
using VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Interface;
using VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Mapper;
using VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Models;

namespace VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Repository
{
    public class ScvRefDBRepository : IScvRefDBRepository
    {
        private readonly IScvRefDbService _scvRefDBServices;

        public ScvRefDBRepository(IScvRefDbService scvRefDBServices)
        {
            _scvRefDBServices = scvRefDBServices;
        }

        public async Task<IEnumerable<BookingDetails>> GetBookingDetails(FlownDetails flownDetails)
        {
            return BookingDetailsMapper.MapBookingDetailsData(await _scvRefDBServices.GetPartnerBookingAsync(flownDetails));
        }      

        public async Task<PreValidationPartnerData> GetBookingDetailsV2(FlownDetails flownDetails)
        {
            return BookingDetailsMapper.MapBookingDetailsDataV2(await _scvRefDBServices.GetPartnerBookingV2Async(flownDetails));
        }

        public async Task<IEnumerable<PartnerProfileDetails>> GetPartnerCodeDetails()
        {
            return PartnerCodeMapper.MapPartnerCodeData(await _scvRefDBServices.GetPartnerCodeAsync());
        }

        public async Task<IEnumerable<ProductDetails>> GetProductsDetails()
        {
            return ProductMapper.MapProductData(await _scvRefDBServices.GetProductsAsync());
        }

        public async Task<IEnumerable<TierThresholdValues>> GetTierThresholdDetails()
        {
            return ThresholdValueMapper.MapThresholdData(await _scvRefDBServices.GetTierThresholdAsync());
        }

        public async Task<int> GetDistanceDetails(Distance distance)
        {
            var response = await _scvRefDBServices.GetDistanceAsync(distance);
            return Convert.ToInt32(Convert.ToDecimal(response.FirstOrDefault()?.DistanceInMiles));
        }

        public async Task<NonVSRetroCabinClassDetails> GetCabinClassForNonVSRetro(NonVSRetroFlightDetails flightDetails)
        {
            return await _scvRefDBServices.GetCabinClassNonVSRetroAsync(flightDetails);
        }

        public async Task<IEnumerable<CountryDetails>> GetCountryCodeDetails()
        {
            return CountryDataMapper.MapCountryCodeData(await _scvRefDBServices.GetAllCountryCodesAsync());

        }

        public async Task<DistanceAndCountryCodeResponse> GetDistanceWithCountryCode(Distance distance)
        {
            return await _scvRefDBServices.GetDistanceWithCountryCode(distance);
        }
    }
}
