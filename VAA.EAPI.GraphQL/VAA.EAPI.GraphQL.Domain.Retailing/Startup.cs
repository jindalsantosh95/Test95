using HotChocolate.Execution.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Interface;
using VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Services;
using VAA.EAPI.GraphQL.Domain.Retailing.Mutations;
using VAA.EAPI.GraphQL.Domain.Retailing.Queries;
using VAA.EAPI.GraphQL.Infrastructure.FunctionHost;

namespace VAA.EAPI.GraphQL.Domain.Retailing
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
            services.AddSingleton(typeof(IOrderDataProvider), typeof(OrderDataProvider))
                    .AddSingleton(typeof(IOrderSearchProvider), typeof(OrderSearchProvider))
                    .AddSingleton(typeof(ICateringProvider), typeof(CateringProvider))
                    .AddSingleton(typeof(IMealServiceProvider), typeof(MealServiceProvider));
        }

        public void RegisterMutations(IRequestExecutorBuilder builder, bool isSecure)
        {
            if (isSecure || System.Diagnostics.Debugger.IsAttached)
            {
                builder.TryAddType<OrderMutations>();
                builder.TryAddType<MealServiceMutation>();
            }

            if (!isSecure || System.Diagnostics.Debugger.IsAttached)
            {
            }
        }

        public void RegisterQueries(IRequestExecutorBuilder builder, bool isSecure)
        {
            if (isSecure || System.Diagnostics.Debugger.IsAttached)
            {
                builder.TryAddType<BookingsQueries>();
                builder.TryAddType<CateringQueries>();
            }

            if (!isSecure || System.Diagnostics.Debugger.IsAttached)
            {
            }
        }

        public void RegisterHealthChecks(IHealthChecksBuilder builder)
        {
        }
    }
}
