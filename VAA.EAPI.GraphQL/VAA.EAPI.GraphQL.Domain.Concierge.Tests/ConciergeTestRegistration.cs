using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.FunctionHost;
using VAA.EAPI.GraphQL.Infrastructure.FunctionHost;

namespace VAA.EAPI.GraphQL.Domain.Concierge.Tests
{
    internal static class ConciergeTestRegistration
    {
        public static IServiceCollection AddConcierge(this IServiceCollection services)
        {
            IStartup startup = new Startup();
            var builder = services.AddGraphQLServer().ModifyRequestOptions(o => o.IncludeExceptionDetails = true);
            builder.RegisterDomainSchema(false,
                [startup.RegisterQueries], [startup.RegisterMutations], [startup.RegisterSubscriptions]);
            startup.ConfigureServices(services);
            return services;
        }
    }
}
