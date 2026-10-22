using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.FunctionHost;

namespace VAA.EAPI.GraphQL
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            var hostBuilder = new HostBuilder();

            hostBuilder.Properties.Add("args", args);

            var hostBuilderInitialiser = (HostBuilderInitialiser)hostBuilder;

            return hostBuilderInitialiser.Configure().Build().RunAsync();
        }
    }
}
