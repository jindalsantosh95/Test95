using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System.Net;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Book.Helpers;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using Summary = VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary.Summary;

namespace VAA.EAPI.GraphQL.Domain.Book.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class SummaryResponseQuery
    {
        protected readonly ILogger<SummaryResponseQuery> _logger;
        private readonly IBasketServiceProvider _basketServiceProvider;

        public SummaryResponseQuery(ILogger<SummaryResponseQuery> logger, IBasketServiceProvider basketServiceProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _basketServiceProvider = basketServiceProvider ?? throw new ArgumentNullException(nameof(basketServiceProvider));
        }

        [GraphQLName("summaryResponse")]
        [GraphQLDescription("Get Flight Summary Response From Basket")]
        public async Task<Summary> GetFlightSummaryResponseAsync(string basketId)
        {
            try
            {
                if (string.IsNullOrEmpty(basketId)) throw new ArgumentNullException(nameof(basketId));

                if (ConfigReader.Try<bool>(Constants.Environment.EnableFlightSummaryMockResponse, false))
                {
                    return Json.Deserialize<Summary>(File.ReadAllText($"{Common.Constants.Application.FunctionAppDirPath}/MockResponses/FlightSummaryResponse.json"));
                }

                var basket = await _basketServiceProvider.RetrieveBasketAsync(basketId);
                var summary = Json.Deserialize<Summary>(basket.FlightSummary);
                if (summary is null)
                {
                    return null;
                }

                VoucherRedemptionFeatureHelper.HydrateBasketFromPersistedSummary(basket, summary);
                summary.HasSufficientPoints = basket.HasSufficientPoints;
                // BasketMS can return an older root flag while the serialized summary
                // already contains the selected redemption state.
                summary.VoucherApplied = basket.VoucherApplied || summary.VoucherApplied;
                summary.VoucherPoints = basket.VoucherPoints;
                summary.PointsToPay = basket.PointsToPay;
                summary.VoucherDetails = basket.VoucherDetails;
                summary.VoucherRedemptionSelection = basket.VoucherRedemptionSelection;

                return VoucherRedemptionFeatureHelper.Normalize(summary);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {basketId} - Exception - {ex}");
                throw new GraphQLException(ErrorBuilder.New()
                             .SetMessage("Unhandled Exception in FlightSummaryResponseQuery.")
                             .SetExtension("Exception", ex.Message)
                             .SetCode(((int)HttpStatusCode.InternalServerError).ToString())
                             .Build());
            }
        }
    }
}
