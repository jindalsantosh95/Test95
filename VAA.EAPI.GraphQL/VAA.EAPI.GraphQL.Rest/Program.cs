using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Rest.FunctionHost;

namespace VAA.EAPI.GraphQL.Rest
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            var hostBuilder = new HostBuilder();

            hostBuilder.Properties.Add("args", args);

            var hostBuilderInitialiser = (HostBuilderInitialiser)hostBuilder;

            var builder = hostBuilderInitialiser.Configure().Build();

            // we do this so we can output in the console the routes that have been registered by the router,
            // which is done in its constructor, and to ensure that the router is instantiated before the host
            // starts accepting requests
            _ = builder.Services.GetRequiredService(typeof(Router));

            return builder.RunAsync();
        }
    }
}