using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Interface;
using VAA.EAPI.GraphQL.Domain.Retailing.Models;

namespace VAA.EAPI.GraphQL.Domain.Retailing.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class CateringQueries(ICateringProvider cateringProvider, ILogger<CateringQueries> logger)
    {
        private readonly ICateringProvider _cateringProvider = cateringProvider ?? throw new ArgumentNullException(nameof(cateringProvider));
        private readonly ILogger<CateringQueries> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [GraphQLName("isEligible")]
        [GraphQLDescription("Retrieves eligibility status.")]
        public async Task<PNR> IsEligible(PNR bookingInfo)
        {
            try
            {

                if (bookingInfo == null
                    || !bookingInfo.Segments.Any()
                    || bookingInfo.Segments.All(x => string.IsNullOrWhiteSpace(x.Cabin))
                    || !bookingInfo.Passengers.Any()
                    || bookingInfo.BookingDate == default
                    || string.IsNullOrWhiteSpace(bookingInfo.RecordLocator))
                {
                    throw new GraphQLException(ErrorBuilder.New()
                        .SetMessage("FlightDate, RecordLocator, Cabin, Passengers info are required fields")
                        .SetExtension("code", 400)
                        .Build());
                }

                return await _cateringProvider.IsEligible(bookingInfo);
            }
            catch (GraphQLException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new GraphQLException(ErrorBuilder.New()
                    .SetMessage("Unhandled exception")
                    .SetExtension("code", 500)
                    .Build());
            }
        }

        [GraphQLName("retrieveFlightMeals")]
        [GraphQLDescription("Retrieves the meals available for selection for the selected flight and cabin.")]
        public async Task<IEnumerable<Meal>> RetrieveFlightMeals(string flightDate, string carrier, int flightNumber, string cabin)
        {
            try
            {
                if (string.IsNullOrEmpty(flightDate) || string.IsNullOrEmpty(carrier) || flightNumber <= 0 || string.IsNullOrEmpty(cabin))
                {
                    throw new GraphQLException(ErrorBuilder.New()
                        .SetMessage("Flight Date, Carrier, Flight Number and Cabin are required fields")
                        .SetExtension("code", 400)
                        .Build());
                }

                var mealList = await _cateringProvider.GetFlightMeals(DateOnly.Parse(flightDate), carrier, flightNumber, cabin);

                if (mealList == null || !mealList.Any())
                {
                    throw new GraphQLException(ErrorBuilder.New()
                        .SetMessage("No meal options available for this flight / cabin combination.")
                        .SetExtension("code", "NO_SSRS_FOUND")
                        .Build());
                }

                return mealList;
            }           
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId}");

                throw;
            }
        }
        [GraphQLName("MealOrderingContext")]
        [GraphQLDescription("Resolves the booking and passenger context required to initialize the Meal Selection page for a customer, using the CacheId provided in the deep link.")]
        public async Task<MealOrderingContext> MealOrderingContext(string cacheId)
        {
            try
            {
                if (string.IsNullOrEmpty(cacheId))
                {
                    throw new GraphQLException(ErrorBuilder.New()
                        .SetMessage("CacheId is required field")
                        .SetExtension("code", 400)
                        .Build());
                }
                // implementation to be completed 
                throw new NotImplementedException();
            }
            catch (GraphQLException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new GraphQLException(ErrorBuilder.New()
                    .SetMessage("Unhandled exception")
                    .SetExtension("code", 500)
                    .Build());
            }
        }
    }
}
