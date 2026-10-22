using HotChocolate.Execution.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Search.Helpers;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Account;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.BasketMS;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.BrandMS;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.InRuleEngine;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.OfferMs;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Services;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.SustainMS;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.VRedMS;
using VAA.EAPI.GraphQL.Domain.Search.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Mutations;
using VAA.EAPI.GraphQL.Domain.Search.Models.RewardSeatAvailability;
using VAA.EAPI.GraphQL.Domain.Search.Queries;
using VAA.EAPI.GraphQL.Domain.Search.Services;
using VAA.EAPI.GraphQL.Domain.Search.Validation;
using VAA.EAPI.GraphQL.Infrastructure.FunctionHost;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Search
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
            services.AddSingleton<IOfferSearchServices, OfferSearchSevices>()
                    .AddSingleton<IOfferPriceServices, OfferPriceServices>()
                    .AddSingleton<IBasketService, BasketService>()
                    .AddSingleton<IReferenceService, ReferenceService>()
                    .AddSingleton<ISustainService, SustainService>()
                    .AddSingleton<IBrandService, BrandService>()
                    .AddSingleton<IOfferRetrieveService, OfferRetrieveService>()
                    .AddSingleton(typeof(IRefDataService<>), typeof(RefDataService<>))
                    .AddSingleton(typeof(ITokenB2CProvider<>), typeof(TokenB2CProvider<>))
                    .AddSingleton<ITokenValidatorExtension, TokenValidatorExtension>()
                    .AddSingleton<IVRedServices, VredServices>()
                    .AddSingleton<IInRuleService, InRuleService>()
                    .AddSingleton<IVoucherRedemptionService, VoucherRedemptionService>()
                    .AddSingleton<IGraphqlHelper, GraphqlHelper>()
                    .AddSingleton(TimeProvider.System)
                    .AddSingleton(typeof(ICustomerProvider), typeof(CustomerService))
                    .AddSingleton(typeof(ICustomerTransactionProvider), typeof(CustomerTransactionService))
                    .AddSingleton<IContentProvider, ContentProvider>();
            services.AddSingleton<IRewardSeatAvailabilityQueryHandler, RewardSeatAvailabilityQueryHandler>();
        }

        public void RegisterMutations(IRequestExecutorBuilder builder, bool isSecure)
        {
            if (isSecure || System.Diagnostics.Debugger.IsAttached)
            {

            }

            if (!isSecure || System.Diagnostics.Debugger.IsAttached)
            {
            }
        }

        public void RegisterQueries(IRequestExecutorBuilder builder, bool isSecure)
        {
            if (isSecure || System.Diagnostics.Debugger.IsAttached)
            {
            }

            if (!isSecure || System.Diagnostics.Debugger.IsAttached)
            {
                builder.TryAddType<CarHireQuery>()
                       .TryAddType<HolidaysQuery>()
                       .TryAddType<HotelsQuery>()
                       .TryAddType<OfferSearchBFFQuery>()
                       .TryAddType<OfferSearchNextBFFQuery>()
                       .TryAddType<SummaryQueryBFF>()
                       .TryAddType<SummaryDeepLinkQuery>()
                       .TryAddType<BrandQueryBFF>()
                       .TryAddType<FareQuery>()
                       .TryAddType<SalesBannerQuery>()
                       .TryAddType<CarbonIndexQuery>();

                if (!isSecure &&
                    ConfigReader.Try<string>(Constants.Environment.EnableRewardSeatAvailability, string.Empty).Trim().Is("true"))
                {
                    builder.TryAddType<RewardSeatMonthType>();
                    builder.TryAddType<RewardSeatAvailabilityQuery>();
                    builder.AddValidationRule<RewardSeatOperationValidationRule>();
                }
            }
        }

        public void RegisterHealthChecks(IHealthChecksBuilder builder)
        {

        }
    }
}
