using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Domain.Trips.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SSRMS;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Enum;
using VAA.EAPI.GraphQL.Domain.Trips.Models.SSR;

namespace VAA.EAPI.GraphQL.Domain.Trips.Mutations
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public class SelectMealMutation
    {
        private readonly ISSRDetailsProvider _ssrDetailsProvider;
        private readonly IReferenceServiceProvider _referenceServiceProvider;
        private readonly ILogger<SelectMealMutation> _logger;

        public SelectMealMutation(ISSRDetailsProvider ssrDetailsProvider, IReferenceServiceProvider referenceServiceProvider, ILogger<SelectMealMutation> logger)
        {
            _ssrDetailsProvider = ssrDetailsProvider ?? throw new ArgumentNullException(nameof(ssrDetailsProvider));
            _referenceServiceProvider = referenceServiceProvider ?? throw new ArgumentNullException(nameof(referenceServiceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [GraphQLName("selectMealMutation")]
        [GraphQLDescription("Select meal per passenger for requested segment")]
        public async Task<PassengerResponse> SelectMeal(string tripReservationId, string segmentId, string passengerId, string ssrCode, bool applyToAll)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(tripReservationId);
                ArgumentNullException.ThrowIfNull(segmentId);
                ArgumentNullException.ThrowIfNull(passengerId);
                ArgumentNullException.ThrowIfNull(ssrCode);
                ArgumentNullException.ThrowIfNull(applyToAll);

                IEnumerable<Models.ReferenceData.SSRMealRefData> refData = Enumerable.Empty<Models.ReferenceData.SSRMealRefData>();
                PassengerSSRResponse msResponse = new();
                var tripResourceId = await BookingIdHelper.GetMSBookingId(tripReservationId);

                Parallel.Invoke(
                         new Action(() => refData = _referenceServiceProvider.RetrieveSSRMealDataAsync().GetAwaiter().GetResult()),
                         new Action(() => msResponse = _ssrDetailsProvider.QuerySSRMealDetailsAsync(tripResourceId).GetAwaiter().GetResult())
                       );

                var eligibleSegments = SSRServiceHelpers.GetEligibleSegments(msResponse, passengerId, [segmentId], applyToAll, Common.Constants.Application.SpecialMealCategory);
                var action = SSRServiceHelpers.GetServiceAction(eligibleSegments, ssrCode, [segmentId], refData, Common.Constants.Application.SpecialMealCategory, Common.Constants.Application.StandardMealCategoryChoice);
                var updateResponse = await _ssrDetailsProvider.ManageSSRAsync((SSRUpsertRequest)(eligibleSegments, applyToAll, action), tripResourceId);

                return (PassengerResponse)(updateResponse, PassengerConversionType.MEAL, refData, null);

            }
            catch (Exception ex)
            {
                var msError = await (ex.InnerException as HttpResponseException)?.Response?.Content?.ReadAsStringAsync();
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} | Booking reference - {tripReservationId} | Error message - {msError}");

                throw new Exception(Constants.ErrorCodes.SelectSpecialMealErrorCode, ex);
            }
        }
    }
}