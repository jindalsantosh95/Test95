using HotChocolate.Execution.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Mutations;
using VAA.EAPI.GraphQL.Domain.Book.Mutations.Basket;
using VAA.EAPI.GraphQL.Domain.Book.Mutations.CoBrand;
using VAA.EAPI.GraphQL.Domain.Book.Queries;
using VAA.EAPI.GraphQL.Domain.Book.Services;
using VAA.EAPI.GraphQL.Infrastructure.FunctionHost;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services.Analytics;

namespace VAA.EAPI.GraphQL.Domain.Book
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
            services.AddSingleton<IOrderMutationProvider, OrderMutationProvider>()
                    .AddSingleton<IOfferServiceProvider, OfferServiceProvider>()
                    .AddSingleton<IBasketServiceProvider, BasketServiceProvider>()
                    .AddSingleton<IPaymentServiceProvider, PaymentServiceProvider>()
                    .AddSingleton<IRefServiceProvider, ReferenceServiceProvider>()
                    .AddSingleton<ICustomerProvider, CustomerServiceProvider>()
                    .AddSingleton<ISeatServiceProvider, SeatServiceProvider>()
                    .AddSingleton<ILOPAServiceProvider, LOPAServiceProvider>()
                    .AddSingleton<IVRedServiceProvider, VRedServiceProvider>()
                    .AddSingleton<IVoucherRedemptionProvider, VoucherRedemptionProvider>()
                    .AddSingleton<ILoyaltySyncServiceProvider, LoyaltySyncServiceProvider>()
                    .AddSingleton<IInRuleServiceProvider, InRuleServiceProvider>()
                    .AddSingleton(typeof(ITokenB2CProvider<>), typeof(TokenB2CProvider<>))
                    .AddSingleton<IInsuranceServiceProvider, InsuranceServiceProvider>()
                    .AddSingleton<ICustomerServiceProvider, CustomerServiceProvider>()
                    .AddSingleton<IBasketEarnPointsCalculatorService, BasketEarnPointsCalculatorService>();

        }

        public void RegisterMutations(IRequestExecutorBuilder builder, bool isSecure)
        {
            if (isSecure || System.Diagnostics.Debugger.IsAttached)
            {

            }

            if (!isSecure || System.Diagnostics.Debugger.IsAttached)
            {
                builder.TryAddType<OrderMutation>()
                       //.TryAddType<BasketMutation>()
                       .TryAddType<ContactDetailMutation>()
                       .TryAddType<PassengerMutations>()
                       .TryAddType<TnCMutation>()
                       .TryAddType<PaymentMutation>()
                       .TryAddType<UpdatePaymentStatusMutation>()
                       .TryAddType<UpdateSeatMutation>()
                       .TryAddType<UpdateInsuranceDecisionMutation>()
                       .TryAddType<VoucherRedemptionMutations>()
                       .TryAddType<SynchronyTokenMutation>();
            }
        }

        public void RegisterQueries(IRequestExecutorBuilder builder, bool isSecure)
        {
            if (isSecure || System.Diagnostics.Debugger.IsAttached)
            {
                builder.TryAddType<SeatMapQuery>();
            }

            if (!isSecure || System.Diagnostics.Debugger.IsAttached)
            {
                builder.TryAddType<RetrieveBasketQuery>()
                       .TryAddType<OrderResponseQuery>()
                       .TryAddType<PaymentDetailsResponseQuery>()
                       .TryAddType<RetrieveSeatMapQuery>()
                       .TryAddType<CountriesQuery>()
                       .TryAddType<SeatMapQuery>()
                       .TryAddType<SummaryResponseQuery>()
                       .TryAddType<ConvertPointsToCurrencyQuery>()
                       .TryAddType<InsuranceQuoteQuery>()
                       .TryAddType<CoBrandApplicationStatusQuery>();
            }
        }

        public void RegisterHealthChecks(IHealthChecksBuilder builder)
        {

        }
    }
}
