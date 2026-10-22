using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Microservices.Retail.Reference.Domain;
using VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Interface;
using VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Models;

namespace VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Services
{
    public class ScvRefDbService : IScvRefDbService
    {
        private readonly ScvReferenceDbContext _refDbContext;
        private readonly ICacheService _iCacheService;

        public ScvRefDbService(ScvReferenceDbContext refDbContext, ICacheService iCacheService)
        {
            _refDbContext = refDbContext;
            _refDbContext.ChangeTracker.AutoDetectChangesEnabled = false;
            _iCacheService = iCacheService;
        }

        public async Task<IEnumerable<PartnerProfile>> GetPartnerCodeAsync()
        {
            IEnumerable<PartnerProfile> partnerCodeData = await _iCacheService.GetCacheValue<IEnumerable<PartnerProfile>>(Constants.Application.PartnerCodeCacheKey);

            if (partnerCodeData == null)
            {
                partnerCodeData = _refDbContext.PartnerProfile.ToList();
                await _iCacheService.SetCacheValue(Constants.Application.PartnerCodeCacheKey, partnerCodeData, true);
            }

            return partnerCodeData;
        }

        public async Task<IEnumerable<Product>> GetProductsAsync()
        {
            IEnumerable<Product> productData = await _iCacheService.GetCacheValue<IEnumerable<Product>>(Constants.Application.ProductCacheKey);

            if (productData == null)
            {
                productData = _refDbContext.Product.ToList();
                await _iCacheService.SetCacheValue(Constants.Application.ProductCacheKey, productData, true);
            }

            return productData;
        }

        public async Task<IEnumerable<MemberAccountReferenceData>> GetTierThresholdAsync()
        {
            IEnumerable<MemberAccountReferenceData> tierThresholdData = await _iCacheService.GetCacheValue<IEnumerable<MemberAccountReferenceData>>(Constants.Application.TierThresholdCacheKey);

            if (tierThresholdData == null)
            {
                tierThresholdData = _refDbContext.MemberAccountReferenceData.FromSqlRaw(Constants.Application.SPQueryinRefDB).ToList();
                await _iCacheService.SetCacheValue(Constants.Application.TierThresholdCacheKey, tierThresholdData, true);
            }

            return tierThresholdData;
        }

        public Task<IEnumerable<DistanceDetails>> GetDistanceAsync(Distance distance)
        {
            SqlParameter[] parameters = new SqlParameter[2];

            parameters[0] = new SqlParameter("@OriginCountryCode", SqlDbType.NVarChar, 30);
            parameters[0].Value = distance.OriginCountryCode;

            parameters[1] = new SqlParameter("@DestinationCountryCode", SqlDbType.NVarChar, 30);
            parameters[1].Value = distance.DestinationCountryCode;

            return Task.FromResult<IEnumerable<DistanceDetails>>(_refDbContext.Distance.FromSqlRaw(Constants.Application.DistanceSPQueryinRefDB, parameters).ToList());
        }

        public Task<IEnumerable<PartnerBooking>> GetPartnerBookingAsync(FlownDetails flownDetails)
        {
            SqlParameter[] parameters = new SqlParameter[9];

            parameters[0] = new SqlParameter("@BookingClass", SqlDbType.NVarChar, 30);
            parameters[0].Value = flownDetails.OriginalBookedClass;

            parameters[1] = new SqlParameter("@FlownClass", SqlDbType.NVarChar, 30);
            parameters[1].Value = flownDetails.FlownUpgradedClass;

            parameters[2] = new SqlParameter("@OperatedAirlineCode", SqlDbType.NVarChar, 30);
            parameters[2].Value = flownDetails.OperatingAirlineCode;

            parameters[3] = new SqlParameter("@ActivityDate", SqlDbType.DateTime);
            parameters[3].Value = flownDetails.DepartureDate;

            parameters[4] = new SqlParameter("@ListFareValue", SqlDbType.NVarChar, 200);
            parameters[4].Value = (object)flownDetails.FareBasis ?? DBNull.Value;

            parameters[5] = new SqlParameter("@ListTicketValue", SqlDbType.NVarChar, 200);
            parameters[5].Value = (object)flownDetails.TicketDesignator ?? DBNull.Value;

            parameters[6] = new SqlParameter("@ListPaxValue", SqlDbType.NVarChar, 200);
            parameters[6].Value = (object)flownDetails.PassengerList ?? DBNull.Value;

            parameters[7] = new SqlParameter("@Origin", SqlDbType.NVarChar, 30);
            parameters[7].Value = flownDetails.OriginCountryCode;

            parameters[8] = new SqlParameter("@Destination", SqlDbType.NVarChar, 30);
            parameters[8].Value = flownDetails.DestinationCountryCode;

            return Task.FromResult<IEnumerable<PartnerBooking>>(_refDbContext.PartnerBooking.FromSqlRaw(Constants.Application.GetBookingClass, parameters).ToList());
        }        

        public async Task<IEnumerable<ListOfValues>> GetAllCountryCodesAsync()
        {
            IEnumerable<ListOfValues> counryCodeData = await _iCacheService.GetCacheValue<IEnumerable<ListOfValues>>(Constants.Application.CountryCodeCacheKey);
            if (counryCodeData == null)
            {
                counryCodeData = _refDbContext.ListOfValues.Where(x => x.Type == "COUNTRY").ToList();
                await _iCacheService.SetCacheValue(Constants.Application.CountryCodeCacheKey, counryCodeData, true);
            }
            return counryCodeData;

        }

        public Task<IEnumerable<PartnerBookingV2>> GetPartnerBookingV2Async(FlownDetails flownDetails)
        {
            SqlParameter[] parameters = new SqlParameter[6];
            parameters[0] = new SqlParameter("@BookingClass", SqlDbType.NVarChar, 30) { Value = flownDetails.OriginalBookedClass };
            parameters[1] = new SqlParameter("@FlownClass", SqlDbType.NVarChar, 30) { Value = flownDetails.FlownUpgradedClass };
            parameters[2] = new SqlParameter("@OperatedAirlineCode", SqlDbType.NVarChar, 30) { Value = flownDetails.OperatingAirlineCode };
            parameters[3] = new SqlParameter("@ActivityDate", SqlDbType.DateTime) { Value = flownDetails.DepartureDate };
            parameters[4] = new SqlParameter("@Origin", SqlDbType.NVarChar, 30) { Value = flownDetails.OriginCountryCode };
            parameters[5] = new SqlParameter("@Destination", SqlDbType.NVarChar, 30) { Value = flownDetails.DestinationCountryCode };

            return Task.FromResult<IEnumerable<PartnerBookingV2>>(_refDbContext.PartnerBookingV2.FromSqlRaw(Constants.Application.GetBookingClassNew, parameters).ToList());
        }

        public Task<NonVSRetroCabinClassDetails> GetCabinClassNonVSRetroAsync(NonVSRetroFlightDetails flightDetails)
        {
            SqlParameter[] parameters = new SqlParameter[9];

            parameters[0] = new SqlParameter("@operatedAirlineCode", SqlDbType.NVarChar, 30);
            parameters[0].Value = flightDetails.OperatedAirlineCode;

            parameters[1] = new SqlParameter("@flightActivityDate", SqlDbType.NVarChar, 30);
            parameters[1].Value = flightDetails.FlightActivityDate;

            parameters[2] = new SqlParameter("@operatingBookingClass", SqlDbType.NVarChar, 30);
            parameters[2].Value = flightDetails.OperatedbookingClass;

            parameters[3] = new SqlParameter("@marketingBookingClass", SqlDbType.NVarChar, 30);
            parameters[3].Value = flightDetails.MarketingBookingClass;

            parameters[4] = new SqlParameter("@originAirportCode", SqlDbType.NVarChar, 30);
            parameters[4].Value = (object)flightDetails.OriginAirportCode ?? DBNull.Value;

            parameters[5] = new SqlParameter("@destinationAirportCode", SqlDbType.NVarChar, 30);
            parameters[5].Value = (object)flightDetails.DestinationAirportCode ?? DBNull.Value;

            return Task.FromResult<NonVSRetroCabinClassDetails>(_refDbContext.NonVSRetroCabinClass.FromSqlRaw(Constants.Application.GetCabinClassNonVSRetro, parameters).AsEnumerable().FirstOrDefault());
        }

        public Task<DistanceAndCountryCodeResponse> GetDistanceWithCountryCode(Distance distance)
        {
            SqlParameter[] parameters = new SqlParameter[2];

            parameters[0] = new SqlParameter("@OriginCountryCode", SqlDbType.NVarChar, 30);
            parameters[0].Value = distance.OriginCountryCode;

            parameters[1] = new SqlParameter("@DestinationCountryCode", SqlDbType.NVarChar, 30);
            parameters[1].Value = distance.DestinationCountryCode;

            return Task.FromResult<DistanceAndCountryCodeResponse>(_refDbContext.DistanceAndCountryCodeResponse.FromSqlRaw(Constants.Application.DistanceSPInRuleRefDB, parameters).AsEnumerable().FirstOrDefault());
        }
    }
}
