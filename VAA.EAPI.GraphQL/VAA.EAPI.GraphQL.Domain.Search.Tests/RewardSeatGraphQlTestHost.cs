using System;
using System.Threading.Tasks;
using HotChocolate.Execution;
using HotChocolate.Execution.Configuration;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.FunctionHost;
using VAA.EAPI.GraphQL.Infrastructure.FunctionHost;

namespace VAA.EAPI.GraphQL.Domain.Search.Tests
{
    internal static class RewardSeatGraphQlTestHost
    {
        internal static async Task<(ServiceProvider Provider, IRequestExecutor Executor)> CreateAsync(
            bool isSecure = false,
            Action<IServiceCollection>? configureServices = null,
            Action<IRequestExecutorBuilder>? configureSchema = null)
        {
            var services = new ServiceCollection();
            services.AddSingleton<INamingConventions>(new EnumNamingConvention());
            IStartup startup = new Startup();
            var builder = services.AddGraphQLServer()
                .UseDefaultPipeline()
                .AddFiltering()
                .AddSorting()
                .AddType<HotChocolate.Types.NodaTime.LocalDateTimeType>()
                .AddType<HotChocolate.Types.NodaTime.LocalTimeType>()
                .AddType<HotChocolate.Types.NodaTime.LocalDateType>();
            builder.TryAddType<VersionQuery>();
            configureSchema?.Invoke(builder);

            builder.RegisterDomainSchema(isSecure,
                [startup.RegisterQueries], [startup.RegisterMutations], [startup.RegisterSubscriptions]);
            startup.ConfigureServices(services);
            configureServices?.Invoke(services);

            var provider = services.BuildServiceProvider();
            try
            {
                return (provider, await provider.GetRequestExecutorAsync());
            }
            catch
            {
                provider.Dispose();
                throw;
            }
        }
    }
}
