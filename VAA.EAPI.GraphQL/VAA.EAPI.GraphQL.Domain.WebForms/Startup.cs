using HotChocolate.Execution.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.WebForms.Application;
using VAA.EAPI.GraphQL.Domain.WebForms.Infrastructure.OrderDataMicroservice.Interfaces;
using VAA.EAPI.GraphQL.Domain.WebForms.Infrastructure.OrderDataMicroservice.Services;
using VAA.EAPI.GraphQL.Domain.WebForms.Infrastructure.RefundMicroservice.Interfaces;
using VAA.EAPI.GraphQL.Domain.WebForms.Infrastructure.RefundMicroservice.Services;
using VAA.EAPI.GraphQL.Domain.WebForms.Interfaces;
using VAA.EAPI.GraphQL.Domain.WebForms.Mutations;
using VAA.EAPI.GraphQL.Domain.WebForms.Queries;
using VAA.EAPI.GraphQL.Infrastructure.FunctionHost;

namespace VAA.EAPI.GraphQL.Domain.WebForms
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
            services.AddSingleton<IRefundRequestService, RefundRequestService>();
            services.AddSingleton<IBookingProvider, BookingProvider>();
            services.AddSingleton<IRefundProvider, RefundProvider>();
        }

        public void RegisterMutations(IRequestExecutorBuilder builder, bool isSecure)
        {
            if (isSecure || System.Diagnostics.Debugger.IsAttached)
            {
                builder.TryAddType<PaymentRequestFormMutation>();
            }

            if (!isSecure || System.Diagnostics.Debugger.IsAttached)
            {
                builder.TryAddType<RefundRequestMutation>();
            }
        }

        public void RegisterQueries(IRequestExecutorBuilder builder, bool isSecure)
        {
            if (isSecure || System.Diagnostics.Debugger.IsAttached)
            {
            }

            if (!isSecure || System.Diagnostics.Debugger.IsAttached)
            {
                builder.TryAddType<RefundFindMyBooking>();
            }
        }

        public void RegisterHealthChecks(IHealthChecksBuilder builder)
        {

        }
    }
}
