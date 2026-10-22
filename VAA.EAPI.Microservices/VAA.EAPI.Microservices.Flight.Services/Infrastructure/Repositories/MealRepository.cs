using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Flight.Services.Domain.PopsMeal;
using VAA.EAPI.Microservices.Flight.Services.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Flight.Services.Infrastructure.Services.Interfaces;


namespace VAA.EAPI.Microservices.Flight.Services.Infrastructure.Repositories
{
    public class MealRepository : IMealRepository
    {
        private readonly ISpecialServiceRequestV4 _specialServiceRequestV4;
        private readonly IPnrCommonV6 _pnrCommonV6;
        private readonly ILogger<MealRepository> _logger;

        public MealRepository(ISpecialServiceRequestV4 specialServiceRequestV4, IPnrCommonV6 pnrCommonV6, ILogger<MealRepository> logger)
        {
            _specialServiceRequestV4 = specialServiceRequestV4 ?? throw new ArgumentNullException(nameof(specialServiceRequestV4));
            _pnrCommonV6 = pnrCommonV6 ?? throw new ArgumentNullException(nameof(pnrCommonV6));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<MealOperationResponse>>> AddMealAsync(MealOrderRequest request)
        {
            if (request == null)
                return Result.Failure<Response<MealOperationResponse>>("Request cannot be null");

            string sessionId = null;
            var mealResponse = new MealOperationResponse { RecordLocator = request.RecordLocator };
            try
            {
                var setupRequest = MealRequestMapper.SetupContextMapper(request.RecordLocator);
                var setupResponse = _pnrCommonV6.Air4SetupContext(setupRequest);

                sessionId = setupResponse?.SessionId;

                if (request.Segments?.Any() == true)
                {
                    request = RemoveDuplicateMealCodeFromRequest(request, setupResponse);

                    //Remove pax which are not having meal code
                    request.Segments = request.Segments.Select(seg => new FlightSegment
                    {
                        SegmentId = seg.SegmentId,
                        FlightDate = seg.FlightDate,
                        FlightNumber = seg.FlightNumber,
                        Passengers = seg.Passengers?.Select(p => new Passenger
                        {
                            PassengerId = p.PassengerId,
                            Meals = p.Meals?.Where(m => !string.IsNullOrWhiteSpace(m.Code))
                        })
                        .Where(p => p.Meals != null && p.Meals.Any())
                    }).Where(seg => seg.Passengers != null && seg.Passengers.Any());

                    //Since override not available for Update meal, we are deleting and then adding the requested meal
                    bool isDelete = DeleteExistingMealForUpdate(request, setupResponse);

                    if (isDelete == true)
                    {
                        setupRequest = MealRequestMapper.SetupContextMapper(request.RecordLocator);
                        setupResponse = _pnrCommonV6.Air4SetupContext(setupRequest);

                        sessionId = setupResponse?.SessionId;
                    }

                    mealResponse.Segments = await Task.WhenAll(request.Segments?.Select(segment =>
                        ProcessSegmentMealsAsync(segment, sessionId)));
                    if (mealResponse.Segments.Sum(segment => segment.Passengers.Count()) > 0)
                    {
                        var teardownRequest = MealRequestMapper.TearDownMapper(sessionId);
                        _pnrCommonV6.Air4TeardownContext(teardownRequest);
                    }
                }

                return Result.Success(new Response<MealOperationResponse> { Value = mealResponse, Status = Status.OK });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Meal update processing failed.");
                return Result.Failure<Response<MealOperationResponse>>(ex.Message);
            }

        }

        private MealOrderRequest RemoveDuplicateMealCodeFromRequest(MealOrderRequest request, Proxy.PNRCommon.SetupContextResponseType setupResponse)
        {
            var requestInfo = request.Segments
                        .SelectMany(s => s?.Passengers ?? Enumerable.Empty<Passenger>(),
                            (s, p) => new
                            {
                                s.SegmentId,
                                PaxFirstNumber = p.PassengerId?.Split('.').First(),
                                PaxLastNumber = p.PassengerId?.Split('.').Last(),
                                MealCode = p.Meals?.FirstOrDefault()?.Code,
                                FlightNumber = s.FlightNumber.Number
                            });

            var removeReqMealInfo = (from req in requestInfo
                                     join existData in setupResponse.Pnr.SpecialServiceRequest
                                       on new { FirstNameNumber = req.PaxFirstNumber, LastNameNumber = req.PaxLastNumber, code = req.MealCode, FlightNumber = req.FlightNumber?.TrimStart('0') }
                                       equals new { existData.FirstNameNumber, existData.LastNameNumber, code = existData.RequestCode, FlightNumber = existData.FlightNumber?.TrimStart('0') }
                                     select req);


            foreach (var r in removeReqMealInfo)
            {
                foreach (var s in request.Segments ?? Enumerable.Empty<FlightSegment>())
                {
                    foreach (var p in s.Passengers ?? Enumerable.Empty<Passenger>())
                    {
                        if (!p.PassengerId.Is(r.PaxFirstNumber + "." + r.PaxLastNumber) || s.FlightNumber?.Number.TrimStart('0').Is(r.FlightNumber.TrimStart('0')) == false)
                            continue;

                        // Remove matching meals
                        p.Meals = (p.Meals ?? Enumerable.Empty<Meal>())
                            .Where(m => !m.Code.Is(r.MealCode));
                    }
                }
            }
            ;
            return request;
        }

        private bool DeleteExistingMealForUpdate(MealOrderRequest request, Proxy.PNRCommon.SetupContextResponseType setupResponse)
        {
            bool isDelete = false;
            var requestInfo = request.Segments
                                   .SelectMany(s => s?.Passengers ?? Enumerable.Empty<Passenger>(),
                                       (s, p) => new
                                       {
                                           s.SegmentId,
                                           p.PassengerId,
                                           PaxFirstNumber = p.PassengerId?.Split('.').First(),
                                           PaxLastNumber = p.PassengerId?.Split('.').Last(),
                                           MealCode = p.Meals?.FirstOrDefault()?.Code,
                                           FlightNumber = s.FlightNumber?.Number
                                       });


            var deleteExistingMealSegments = from a in requestInfo
                                             join b in setupResponse.Pnr.SpecialServiceRequest
                                               on new { firstNumber = a.PaxFirstNumber, lastNumber = a.PaxLastNumber, FlightNumber = a.FlightNumber?.TrimStart('0') }
                                               equals new { firstNumber = b.FirstNameNumber, lastNumber = b.LastNameNumber, FlightNumber = b.FlightNumber?.TrimStart('0') }
                                             where a.MealCode != b.RequestCode && b.RequestCode.EndsWith("ML")
                                             select new FlightSegment
                                             {
                                                 SegmentId = a.SegmentId,
                                                 FlightNumber = new FlightNumber { Number = a.FlightNumber, AirlineCode = "VS" },
                                                 Passengers = new List<Passenger>
                                              {
                                                  new Passenger {
                                              PassengerId = a.PassengerId,
                                              Meals = new List<Meal>{ new Meal{Code= b.RequestCode, SsrId= b.SsrNumber} }
                                              } }
                                             };

            if (deleteExistingMealSegments.Any() == true)
            {
                MealCancelRequest delRequest = new MealCancelRequest
                {
                    OrderId = request.OrderId,
                    RecordLocator = request.RecordLocator,
                    Segments = deleteExistingMealSegments
                };

                var mealResponseSegments = ProcessSegmentMealCancellation(deleteExistingMealSegments, setupResponse?.SessionId);

                if (mealResponseSegments.Sum(segment => segment.Passengers.Count()) > 0)
                {
                    var teardownRequest = MealRequestMapper.TearDownMapper(setupResponse?.SessionId);
                    _pnrCommonV6.Air4TeardownContext(teardownRequest);
                    isDelete = true;
                }
            }

            return isDelete;
        }

        private async Task<FlightSegment> ProcessSegmentMealsAsync(FlightSegment segment, string sessionId)
        {
            var passengers = new List<Passenger>();

            foreach (var pax in segment.Passengers)
            {
                var addRequest = MealRequestMapper.AddMealRequest(segment, pax.Meals.First().Code, [pax], sessionId);
                var response = _specialServiceRequestV4.AddSSRsRequest(addRequest);

                if (response?.Pnr?.SpecialServiceRequest != null)
                {
                    var ssrRequest = response.Pnr.SpecialServiceRequest.Where(x => x.RequestCode.EndsWith("ML") && x.FlightNumber.TrimStart('0') == segment.FlightNumber.Number.TrimStart('0')
                    && x.FirstNameNumber == pax.PassengerId.Split('.').First() && x.LastNameNumber == pax.PassengerId?.Split('.').Last());
                    foreach (var ssr in ssrRequest)
                    {
                        passengers.Add(new Passenger
                        {
                            PassengerId = $"{ssr.FirstNameNumber}.{ssr.LastNameNumber}",
                            Meals = [new Meal { Code = ssr.RequestCode }]
                        });
                    }
                }
            }

            return new FlightSegment
            {
                SegmentId = segment.SegmentId,
                FlightDate = segment.FlightDate,
                FlightNumber = segment.FlightNumber,
                Passengers = passengers
            };
        }

        public async Task<Result<Response<MealOperationResponse>>> CancelMealAsync(MealCancelRequest request)
        {
            if (request == null)
                return Result.Failure<Response<MealOperationResponse>>("Request cannot be null");

            string sessionId = null;
            var mealResponse = new MealOperationResponse { RecordLocator = request.RecordLocator };
            try
            {
                var setupRequest = MealRequestMapper.SetupContextMapper(request.RecordLocator);
                var setupResponse = _pnrCommonV6.Air4SetupContext(setupRequest);

                sessionId = setupResponse?.SessionId;
                var existingSSRs = setupResponse?.Pnr?.SpecialServiceRequest;

                foreach (var s in request.Segments)
                {
                    foreach (var p in s.Passengers)
                    {
                        var idParts = p.PassengerId?.Split('.');
                        if (idParts?.Length == 2)
                        {
                            var firstNameNumber = idParts[0];
                            var lastNameNumber = idParts[1];
                            var flightNum = s.FlightNumber.Number;

                            var updatedMeals = new List<Meal>();
                            foreach (var meal in p.Meals)
                            {
                                var matchingSSR = existingSSRs?.FirstOrDefault(x =>
                                    x.FlightNumber == flightNum.TrimStart('0') &&
                                    x.FirstNameNumber == firstNameNumber &&
                                    x.LastNameNumber == lastNameNumber &&
                                    x.RequestCode == meal.Code);
                                if (matchingSSR != null)
                                    updatedMeals.Add(new Meal
                                    {
                                        Code = meal.Code,
                                        SsrId = matchingSSR?.SsrNumber
                                    });
                            }
                            p.Meals = updatedMeals;
                        }
                    }
                }

                if (request.Segments?.Any() == true)
                {
                    mealResponse.Segments = ProcessSegmentMealCancellation(request.Segments, sessionId);
                    if (mealResponse.Segments.Any(segment => segment.Passengers.Any()))
                    {
                        var teardownRequest = MealRequestMapper.TearDownMapper(sessionId);
                        _pnrCommonV6.Air4TeardownContext(teardownRequest);
                    }
                }

                return Result.Success(new Response<MealOperationResponse> { Value = mealResponse, Status = Status.OK });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Meal cancellation processing failed.");
                return Result.Failure<Response<MealOperationResponse>>(ex.Message);
            }
        }

        private IEnumerable<FlightSegment> ProcessSegmentMealCancellation(IEnumerable<FlightSegment> segments, string sessionId)
        {
            var processedPassengers = new List<Passenger>();

            segments = segments
                .Select(seg => new FlightSegment
                {
                    SegmentId = seg.SegmentId,
                    FlightDate = seg.FlightDate,
                    FlightNumber = seg.FlightNumber,
                    Passengers = (seg.Passengers ?? Enumerable.Empty<Passenger>())
                        .Where(pax => (pax.Meals ?? Enumerable.Empty<Meal>())
                            .Any(m => !m.SsrId.IsNullOrWhiteSpace()))
                })
                .Where(seg => seg.Passengers.Count() > 0);

            if (segments.Any())
            {
                var deleteRequest = MealRequestMapper.DeleteMealRequest(segments, sessionId);
                _ = _specialServiceRequestV4.DeleteSSRsRequest(deleteRequest);
            }

            return segments;
        }

    }
}
