using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.FlightOperations.Catering.Application.Models;
using VAA.EAPI.Microservices.FlightOperations.Catering.Infrastructure.Models.Entity;

namespace VAA.EAPI.Microservices.FlightOperations.Catering.Services
{
    public class PopsDbService : IPopsDbService
    {
        private readonly PopsDataContext _popsDataContext;
        private readonly ILogger<PopsDbService> _logger;

        public PopsDbService(PopsDataContext popsDataContext, ILogger<PopsDbService> logger)
        {
            _popsDataContext = popsDataContext ?? throw new ArgumentNullException(nameof(popsDataContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<IEnumerable<Meal>>>> GetMealData(MealsRequest request)
        {
            try
            {
                var mealInfo = await (from menu in _popsDataContext.Menus
                                      join offer in _popsDataContext.PreSelectOffers
                                      on menu.Id equals offer.Menus_Id
                                      where ((request.FlightDate.ToDateTime(TimeOnly.MinValue) >= menu.Effective_From && request.FlightDate.ToDateTime(TimeOnly.MinValue) <= menu.Effective_To)
                                      && Convert.ToString(request.FlightNo) == offer.Flight_Codes_Number && request.Cabin == offer.Cabin_Code && request.Carrier == offer.Flight_Codes_Airline && offer.Enabled == true)
                                      select new Meal
                                      {
                                          Code = offer.Meal_Code,
                                          DishId = menu.Dish_Id,                                         
                                          Description = menu.Name                                          
                                      }).ToListAsync();


                if (mealInfo != null)
                {
                    return new Response<IEnumerable<Meal>>
                    {
                        Status = Status.OK,
                        IgnoreNullValues = false,
                        Value = mealInfo
                    };
                }
                else
                {
                    return Result.Failure<Response<IEnumerable<Meal>>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.NotFound, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Meal data lookup failed for {Carrier} flight {FlightNumber} on {FlightDate} in {Cabin}.", request?.Carrier, request?.FlightNo, request?.FlightDate, request?.Cabin);
                return Result.Failure<Response<IEnumerable<Meal>>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }

        public async Task<int> AddMealOrderAsync(MealOrders mealOrder)
        {
            try
            {
                await UpdateMealOrderAsync(mealOrder);

                mealOrder.OrderStatus = "Confirmed";
                mealOrder.ModifiedOn = DateTime.UtcNow;

                await _popsDataContext.MealOrders.AddAsync(mealOrder);
                return await _popsDataContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Meal order creation failed for {OrderId}.", mealOrder?.OrderID);
                throw;
            }
        }

        public async Task<int> UpdateMealOrderAsync(MealOrders mealOrder)
        {
            try
            {
                var mealInfo = _popsDataContext.MealOrders.FirstOrDefault(mo =>
                                   mo.OrderID == mealOrder.OrderID &&
                                   mo.RecordLocator == mealOrder.RecordLocator &&
                                   mo.FlightNumber == mealOrder.FlightNumber &&
                                   mo.FlightDate == mealOrder.FlightDate &&
                                   mo.PaxId == mealOrder.PaxId &&
                                   mo.OrderStatus.ToLower() == "confirmed");

                if (mealInfo != null)
                {
                    // Update fields
                    mealInfo.OrderStatus = "Cancelled";
                    mealInfo.ModifiedOn = DateTimeOffset.UtcNow;
                    return await _popsDataContext.SaveChangesAsync();
                }
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Meal order update failed for {OrderId}.", mealOrder?.OrderID);
                throw;
            }
        }

        public async Task<PopsEligibility> CheckPopsEligibility(string cabinCode, DateTime departureDate, string operatingAirline, string operatingFlightNum, string orgin, string destination)
        {
            try
            {              
                var eligibilityInfo = _popsDataContext.Flights
                    .Join(
                        _popsDataContext.FlightLegs,
                        flt => flt.FlightId,
                        leg => leg.FlightId,
                        (flt, leg) => new { flt, leg }
                        )
                    .Join(
                        _popsDataContext.PreSelectOffers,
                        x => new { Airline = x.flt.OperatingAirline, Number = x.flt.OperatingFlightNum },
                        ps => new { Airline = ps.Flight_Codes_Airline, Number = ps.Flight_Codes_Number },
                        (x, ps) => new { x.flt, x.leg, ps }
                        )
                    .Where(x =>
                         x.ps.Enabled &&
                         x.ps.Cabin_Code == cabinCode &&
                         x.leg.ScheDepTimestampLoc == departureDate &&
                         x.flt.OperatingAirline == operatingAirline &&
                         x.flt.OperatingFlightNum == operatingFlightNum &&
                         x.leg.Origin == orgin &&
                         x.leg.Destination == destination &&
                         x.ps.Offer_Effective_From <= x.leg.ScheDepTimestampUtc &&
                         x.ps.Offer_Effective_To >= x.leg.ScheDepTimestampUtc
                         )
                    .Select(x => new PopsEligibility
                    {
                         DepartureDateUTC = x.leg.ScheDepTimestampUtc,
                         MinBookingHour = (int)x.ps.Bkg_Win_LWatermark,
                         MaxBookingHour = (int)x.ps.Bkg_Win_HWatermark,
                         OfferCreatedDate = x.ps.Created
                    })
                    .Distinct()
                    .OrderByDescending(s => s.OfferCreatedDate)
                    .AsNoTracking()
                    .FirstOrDefault();

                return eligibilityInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "POPS eligibility lookup failed for {OperatingAirline} flight {OperatingFlightNumber} from {Origin} to {Destination} on {DepartureDate} in {Cabin}.", operatingAirline, operatingFlightNum, orgin, destination, departureDate, cabinCode);
                throw;
            }

        }
    }
}
