using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.FlightOperations.Catering.Application.Interface;
using VAA.EAPI.Microservices.FlightOperations.Catering.Application.Mapper;
using VAA.EAPI.Microservices.FlightOperations.Catering.Application.Models;
using VAA.EAPI.Microservices.FlightOperations.Catering.Infrastructure.Models.Entity;
using VAA.EAPI.Microservices.FlightOperations.Catering.Services;

namespace VAA.EAPI.Microservices.FlightOperations.Catering.Application.Service
{
    public class CateringService : ICateringService
    {
        private IPopsDbService _dbService;
        private readonly ILogger<CateringService> _logger;

        public CateringService(IPopsDbService dbService, ILogger<CateringService> logger)
        {
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService)); ;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<IEnumerable<Meal>>>> GetFlightMealsAsync(MealsRequest request)
        {
            return await _dbService.GetMealData(request);
        }

        public async Task<Result<Response<string>>> AddMealOrderAsync(MealOrders mealOrder)
        {
            int addMealOrderResponse = await _dbService.AddMealOrderAsync(mealOrder);

            if (addMealOrderResponse.Equals(1))
            {
                return new Response<string>
                {
                    Status = Status.OK,
                    Value = Constants.Application.MealOrderCreateSuccessMessage
                };
            }
            else
            {
                _logger.LogError("Meal order creation did not persist an order for {OrderId}.", mealOrder?.OrderID);
                return Result.Failure<Response<string>>(Constants.Application.MealOrderCreateFailureMessage);
            }

        }

        public async Task<Result<Response<string>>> UpdateMealOrderAsync(MealOrders mealOrder)
        {
            var statusUpdate = await _dbService.UpdateMealOrderAsync(mealOrder);

            if (statusUpdate.Equals(1))
            {
                return new Response<string>
                {
                    Status = Status.OK,
                    Value = Constants.Application.MealOrderUpdateSuccessMessage
                };
            }
            else
            {
                _logger.LogError("Meal order update did not find a confirmed order for {OrderId}.", mealOrder?.OrderID);
                return Result.Failure<Response<string>>(Constants.Application.MealOrderUpdateFailureMessage);
            }

        }

        public Result<Response<BookingInfo>> ValidateSegmentsMealEligibility(BookingInfo bookingInfo)
        {
            try
            {
                foreach (var segment in bookingInfo.Segments)
                {

                    if (string.IsNullOrWhiteSpace(segment?.Cabin) ||
                        segment?.Departure == null ||
                        string.IsNullOrWhiteSpace(segment?.OperatingFlight?.AirlineCode) ||
                        segment?.OperatingFlight == null ||
                        segment.OperatingFlight.Number == 0 ||
                        string.IsNullOrWhiteSpace(segment?.Origin) ||
                        string.IsNullOrWhiteSpace(segment?.Destination))
                    {
                        segment.Eligibility = EligibilityType.None;
                        segment.PopsNonEligibleReason = Constants.Application.DataNotFoundMessage;
                    }
                    else
                    {
                        segment.Cabin = CabinMapper.MapToCabin(segment.Cabin);

                        if (segment.Cabin == "J" || segment.Cabin == "W" || (segment.Cabin == "Y" && bookingInfo.Passengers.Any(pax => !string.IsNullOrEmpty(pax.FrequentFlyerNumber))))
                        {
                            var result = _dbService.CheckPopsEligibility(segment.Cabin, segment.Departure, segment.OperatingFlight.AirlineCode, Convert.ToString(segment.OperatingFlight.Number), segment.Origin, segment.Destination);

                            SetEligibilityWithReason(result.Result, segment);
                        }
                        else
                        {
                            segment.Eligibility = EligibilityType.DutyFree;
                            segment.PopsNonEligibleReason = Constants.Application.CabinNonEligibleMessage;
                        }
                    }

                }
                var response = new Response<BookingInfo>
                {
                    Status = Status.OK,
                    Value = bookingInfo
                };

                return Result.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Segment meal eligibility validation failed for {RecordLocator}.", bookingInfo?.RecordLocator);
                return Result.Failure<Response<BookingInfo>>(Constants.Application.ValidateSegmentErrorMessage);
            }
        }

        private Segment SetEligibilityWithReason(PopsEligibility result, Segment segment)
        {
            if (result == null)
            {
                segment.Eligibility = EligibilityType.DutyFree;
                segment.PopsNonEligibleReason = Constants.Application.FltNotFoundMessage;
            }
            else
            {
                //min date to order meal
                var minDate = DateTime.UtcNow.AddHours(result.MinBookingHour);
                //max date to order meal
                var maxDate = DateTime.UtcNow.AddHours(result.MaxBookingHour);

                if (result.DepartureDateUTC < minDate)
                {
                    segment.Eligibility = EligibilityType.DutyFree;
                    segment.PopsNonEligibleReason = Constants.Application.ToLateOrderMessage;
                }
                else if (result.DepartureDateUTC > maxDate)
                {
                    segment.Eligibility = EligibilityType.DutyFree;
                    segment.PopsNonEligibleReason = Constants.Application.ToEarlyOrderMessage;
                }
                else
                {
                    segment.Eligibility = EligibilityType.Both;
                    segment.PopsNonEligibleReason = null;
                }
            }

            return segment;
        }
    }
}
