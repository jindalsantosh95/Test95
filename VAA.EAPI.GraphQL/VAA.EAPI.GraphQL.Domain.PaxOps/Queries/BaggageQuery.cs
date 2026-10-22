using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.Baggage;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.Basket;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class BaggageQuery
    {
        private IBaggageServiceProvider _baggageServiceProvider;
        private ICheckInServiceProvider _checkInServiceProvider;
        private readonly ITelemetryService _telemetryServiceProvider;
        private IBasketServiceProvider _basketServiceProvider;
        private readonly ITokenB2CProvider<string> _loginTokenProvider;
        private readonly ILogger<BaggageQuery> _logger;

        public BaggageQuery(IBaggageServiceProvider baggageServiceProvider, ICheckInServiceProvider checkInServiceProvider, ITelemetryService telemetryServiceProvider, ITokenB2CProvider<string> loginTokenProvider, IBasketServiceProvider basketServiceProvider, ILogger<BaggageQuery> logger)
        {
            _baggageServiceProvider = baggageServiceProvider ?? throw new ArgumentNullException(nameof(baggageServiceProvider));
            _checkInServiceProvider = checkInServiceProvider ?? throw new ArgumentNullException(nameof(checkInServiceProvider));
            _telemetryServiceProvider = telemetryServiceProvider ?? throw new ArgumentNullException(nameof(telemetryServiceProvider));
            _loginTokenProvider = loginTokenProvider ?? throw new ArgumentNullException(nameof(loginTokenProvider));
            _basketServiceProvider = basketServiceProvider ?? throw new ArgumentNullException(nameof(basketServiceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [GraphQLName("BaggageAllowance")]
        [GraphQLDescription("Get baggage allowance for all passengers")]
        public async Task<BaggageResponse> GetBaggageAllowance(string checkInId, bool? hidePaidBaggage, IResolverContext context)
        {
            if (checkInId.IsNullOrEmpty())
            {
                context.ReportError(string.Format(Application.MissingFieldMessage, nameof(checkInId)));
                return null;
            }

            string? emailAddress = null;
            CheckedBaggage? savedBaggages = null;

            try
            {
                Basket? basketResponse = null;

                var basketId = await CheckInIdHelper.GetBasketId(checkInId);

                if (!basketId.IsNullOrEmpty())
                {
                    basketResponse = await _basketServiceProvider.RetrieveBasketAsync(basketId);
                }

                if (basketResponse != null)
                {
                    savedBaggages = basketResponse.CheckedBaggageDetails;
                    emailAddress = basketResponse.ContactDetails?.EmailAddress;
                }

                if (emailAddress.IsNullOrEmpty())
                {
                    var jwtPrincipalFeature = AsyncContext.Current?.Context?.GetJwtPrincipalFeature();

                    if (!(jwtPrincipalFeature?.AccessToken?.Token.IsNullOrWhiteSpace() ?? true))
                    {
                        var loginTokenResponse = await _loginTokenProvider.ValidateTokenAsync(jwtPrincipalFeature.AccessToken.Token);

                        TokenB2CResponse? claim = null;

                        try
                        {
                            claim = Json.Deserialize<TokenB2CResponse>(loginTokenResponse);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to deserialize token response.");
                        }

                        if (claim == null || claim.EmailAddress.IsNullOrEmpty())
                        {
                            _logger.LogWarning("No email address linked to logged-in passenger.");
                        }
                        else
                        {
                            emailAddress = claim.EmailAddress;
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                string? pnr = null;
                try { pnr = await CheckInIdHelper.GetPnrLocator(checkInId); } catch { }
                _logger.LogError(ex, "An error occurred while retrieving member's email address for pnr : {pnr}", pnr);
            }

            try
            {
                checkInId = await CheckInIdHelper.ExtendSessions(_checkInServiceProvider, checkInId);

                _ = _telemetryServiceProvider.TrackTelemetry((TelemetryEvent)(LoggingCheckinEvent.BAGGAGE_OPTIONS_REQUESTED, Data: await TelemetryHelper.BaggageOffersRetrievalInitiated(checkInId)));

                BaggageResponse baggageResponse = await BaggageResponse.BaggageResponseAsync(await _baggageServiceProvider.GetBaggageAsync(checkInId, hidePaidBaggage), checkInId);

                if(baggageResponse.ShowBaggageAllowance)
                    _ = _telemetryServiceProvider.TrackTelemetry((TelemetryEvent)(LoggingCheckinEvent.BAGGAGE_OPTIONS_RECEIVED, Data: await TelemetryHelper.BaggageOptionsRequested(checkInId, baggageResponse)));
                else
                     _ = _telemetryServiceProvider.TrackTelemetry((TelemetryEvent)(LoggingCheckinEvent.BAGGAGE_DETAILS_FAILED, Data: await TelemetryHelper.BaggageDetailsFailed(checkInId, null, LoggingFailureContext.BAGGAGE_OFFERS_RETRIEVAL)));

                if (savedBaggages?.PassengerBaggage != null && baggageResponse.Passengers != null)
                {
                    foreach (var passenger in baggageResponse.Passengers)
                    {
                        var baggage = savedBaggages.PassengerBaggage.FirstOrDefault(x => x.PassengerNiN.Is(passenger.PassengerNiN));

                        if (baggage?.BaggageCount is int baggageCount)
                        {
                            passenger.NumSelectedBags = baggageCount;
                        }
                    }
                }

                baggageResponse.EmailAddress = emailAddress;

                return baggageResponse;
            }

            catch (Exception ex)
            {
                return await BaggageFailureHandler.HandleAsync<BaggageResponse>(
                    context,
                    ex,
                    new BaggageFailureContext
                    {
                        CheckInId = checkInId,
                        LoggingFailureContext = LoggingFailureContext.BAGGAGE_OFFERS_RETRIEVAL,
                        Operation = "retrieve baggage offers"
                    },
                    _telemetryServiceProvider,
                    _logger);
            }            
        }
    }
}
