using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Interface;
using VAA.EAPI.GraphQL.Domain.Retailing.Models.MealServiceMutaion;

namespace VAA.EAPI.GraphQL.Domain.Retailing.Mutations
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public sealed class MealServiceMutation
    {
        private readonly ICateringProvider _cateringProvider;
        private readonly IMealServiceProvider _mealServiceProvider;
        private readonly ILogger<MealServiceMutation> _logger;

        public MealServiceMutation(IMealServiceProvider mealServiceProvider, ICateringProvider cateringProvider, ILogger<MealServiceMutation> logger)
        {
            _mealServiceProvider = mealServiceProvider ?? throw new ArgumentNullException(nameof(mealServiceProvider));
            _cateringProvider = cateringProvider ?? throw new ArgumentNullException(nameof(cateringProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [GraphQLName("addMeal")]
        [GraphQLDescription("Submits the meal request")]
        public async Task<MealOperationResponse> AddMeal(AddMealRequest request)
        {
            try
            {
                if (request != null && !string.IsNullOrWhiteSpace(request.RecordLocator) && request.Segments.Count() > 0)
                {
                    var addMealResponse = await _mealServiceProvider.AddMeal(request);
                    addMealResponse.OrderId = request.OrderId;
                    addMealResponse.OrderStatus = "Confirmed";

                    if (addMealResponse != null && addMealResponse.Segments.Any())
                    {
                        var mealOrders = addMealResponse.Segments
                            .SelectMany(segment => segment.Passengers.Select(pax => new MealOrders
                            {
                                MealOrderCode = addMealResponse.OrderId + "-" + addMealResponse.RecordLocator + "-" + Convert.ToString(segment.FlightNumber.Number).TrimStart('0') + "-" + request.Segments.Where(seg => seg.SegmentId == segment.SegmentId).FirstOrDefault().FlightDate.ToString("ddMMMyyyy"),
                                OrderID = addMealResponse.OrderId,
                                RecordLocator = addMealResponse.RecordLocator,
                                FlightNumber = Convert.ToString(segment.FlightNumber.Number).TrimStart('0'),                                
                                FlightDate = request.Segments.Where(seg => seg.SegmentId == segment.SegmentId).FirstOrDefault().FlightDate,                              
                                MealCode = string.Join(",", pax.Meals.Select(s => s.Code)),//pax.Meals.First().Code,// 
                                PaxId = pax.PassengerId,                                                              
                            }));

                        foreach (var mealOrder in mealOrders)
                        {
                            await _cateringProvider.AddMealOrder(mealOrder);
                        }
                    }

                    return addMealResponse;
                }
                else
                {
                    throw new GraphQLException(ErrorBuilder.New()
                       .SetMessage("RecordLocator and Segments info are required")
                       .SetExtension("code", 400)
                       .Build());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId}");

                throw;
            }
        }

        [GraphQLName("cancelMeal")]
        [GraphQLDescription("Submits the meal cancel request")]
        public async Task<MealOperationResponse> CancelMeal(CancelMealRequest request)
        {
            try
            {
                if (request != null && !string.IsNullOrWhiteSpace(request.RecordLocator) && request.Segments.Count() > 0)
                {

                    var cancelMealResponse = await _mealServiceProvider.CancelMeal(request);
                    cancelMealResponse.OrderId = request.OrderId;
                    cancelMealResponse.OrderStatus = "Cancelled";

                    if (cancelMealResponse != null && cancelMealResponse.Segments.Any())
                    {
                        var mealOrders = cancelMealResponse.Segments
                            .SelectMany(segment => segment.Passengers.Select(pax => new MealOrders
                            {
                                MealOrderCode = cancelMealResponse.OrderId +"-"+ cancelMealResponse.RecordLocator + "-" + Convert.ToString(segment.FlightNumber.Number).TrimStart('0') + "-" + request.Segments.Where(seg => seg.SegmentId == segment.SegmentId).FirstOrDefault().FlightDate.ToString("ddMMMyyyy"),
                                OrderID = cancelMealResponse.OrderId,
                                RecordLocator = cancelMealResponse.RecordLocator,
                                FlightNumber =  Convert.ToString(segment.FlightNumber.Number).TrimStart('0'),                                
                                FlightDate = request.Segments.Where(seg => seg.SegmentId == segment.SegmentId).FirstOrDefault().FlightDate,                           
                                MealCode = string.Join(",", pax.Meals.Select(s => s.Code)),
                                PaxId = pax.PassengerId,                                    
                            }));

                        foreach (var mealOrder in mealOrders)
                        {
                            await _cateringProvider.UpdateMealOrder(mealOrder);
                        }
                    }

                    return cancelMealResponse;
                }
                else
                {
                    throw new GraphQLException(ErrorBuilder.New()
                       .SetMessage("RecordLocator and Segments info are required")
                       .SetExtension("code", 400)
                       .Build());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId}");

                throw;
            }
        }

    }
}
