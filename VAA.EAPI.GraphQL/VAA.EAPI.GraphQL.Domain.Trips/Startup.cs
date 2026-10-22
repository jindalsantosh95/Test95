using HotChocolate.Execution.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Interface.Features;
using VAA.EAPI.GraphQL.Domain.Trips.Interface.LoyaltyProgramDetails;
using VAA.EAPI.GraphQL.Domain.Trips.Mutations;
using VAA.EAPI.GraphQL.Domain.Trips.Queries;
using VAA.EAPI.GraphQL.Domain.Trips.Queries.DeepLink;
using VAA.EAPI.GraphQL.Domain.Trips.Services;
using VAA.EAPI.GraphQL.Domain.Trips.Services.Features;
using VAA.EAPI.GraphQL.Domain.Trips.Services.LoyaltyProgramDetails;
using VAA.EAPI.GraphQL.Infrastructure.FunctionHost;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Trips
{
    public sealed class Startup : IStartup
    {
        public void ConfigureAppConfiguration(IConfigurationBuilder config)
        {
        }

        public void ConfigureFunctionsWorkerDefaults(IFunctionsWorkerApplicationBuilder worker)
        {
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IScvBookingProvider, ScvBookingProvider>();
            services.AddSingleton<ITripsDetailsQueryProvider, TripsDetailsQueryProvider>()
                    .AddSingleton<ITravelRequirementsQueryProvider, TravelRequirementsQueryProvider>()
                    .AddSingleton<IReferenceServiceProvider, ReferenceServiceProvider>()
                    .AddSingleton<ISSRDetailsProvider, SSRDetailsProvider>()
                    .AddSingleton<IBasketProvider, BasketProvider>()
                    .AddSingleton<ISeatProvider, SeatProvider>()
                    .AddSingleton<ILOPAProvider, LOPAProvider>()
                    .AddSingleton<IPassengerDetailsProvider, PassengerDetailsProvider>()
                    .AddSingleton<ILoyaltyActionHandler, LoyaltyActionHandler>()
                    .AddSingleton<ILoyaltyActionStrategy, LoyaltyActionStrategy>()
                    .AddSingleton<ILoyaltyMemberService, LoyaltyMemberService>()
                    .AddSingleton<ILoyaltyRequestValidator, LoyaltyRequestValidator>()
                    .AddSingleton<IPaymentServiceProvider, PaymentServiceProvider>()
                    .AddSingleton<IContentServiceProvider, ContentServiceProvider>()
                    .AddSingleton<IOrderDataServiceProvider, OrderDataServiceProvider>()
                    .AddSingleton<ISeatUpgradeHandler, SeatUpgradeHandler>()
                    .AddSingleton<IUpsellOfferDetailsProvider, UpsellOfferDetailsProvider>()
                    .AddSingleton<IOperationalScheduleProvider, OperationalScheduleProvider>()
                    .AddSingleton<IStarlinkEligibilityProvider, StarlinkEligibilityProvider>()
                    .AddSingleton<ITripAircraftEligibilityService, TripAircraftEligibilityService>()
                    .AddSingleton<ITripDetailsFeatureProvider, TripDetailsFeatureProvider>()
                    .AddSingleton(typeof(ITokenB2CProvider<>), typeof(TokenB2CProvider<>))
                    .AddSingleton<IOrderDataProvider, OrderDataProvider>()
                    .AddSingleton<IHolidayProvider, HolidayProvider>()
                    .AddSingleton<TripDetailsListQuery>();
        }

        public void RegisterMutations(IRequestExecutorBuilder builder, bool isSecure)
        {
            if (isSecure || System.Diagnostics.Debugger.IsAttached)
            {

            }

            if (!isSecure || System.Diagnostics.Debugger.IsAttached)
            {
                builder.TryAddType<SelectSeatMutation>()
                       .TryAddType<SelectMealMutation>()
                       .TryAddType<SelectSpecialAssistanceMutation>()
                       .TryAddType<SeatConfirmationMutation>()
                       .TryAddType<SeatPaymentMutation>()
                       .TryAddType<ManangeLoyaltyMemberMutation>()
                       .TryAddType<PaymentStatusMutation>()
                       .TryAddType<HolidayPaymentMutation>()
                       .TryAddType<ContentQuery>()
                       .TryAddType<UpsellOffersQuery>()
                       .TryAddType<SeatFulfilmentMutation>();
            }
        }

        public void RegisterQueries(IRequestExecutorBuilder builder, bool isSecure)
        {
            if (isSecure || System.Diagnostics.Debugger.IsAttached)
            {
                builder.TryAddType<TripsQuery>()
                       .TryAddType<ProxyDeeplinkQuery>();
            }

            if (!isSecure || System.Diagnostics.Debugger.IsAttached)
            {
                builder.TryAddType<TripsQuery>()
                       .TryAddType<TripsMessageQuery>()
                       .TryAddType<SSRQuery>()
                       .TryAddType<SeatsQuery>()
                       .TryAddType<LoyaltyProgramsQuery>()
                       .TryAddType<TripDetailsListQuery>()
                       .TryAddType<FindTripByFlightDateQuery>()
                       .TryAddType<DeeplinkUrlQuery>()
                       .TryAddType<HolidayQuery>();
            }
        }

        public void RegisterHealthChecks(IHealthChecksBuilder builder)
        {

        }
    }
}
