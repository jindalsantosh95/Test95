using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using VAA.EAPI.Infrastructure.Middleware;

namespace VAA.EAPI.Infrastructure.Extentions
{
    public static class IFunctionsWorkerApplicationBuilderExtension
    {
        public static IFunctionsWorkerApplicationBuilder AddDefaultMiddleware(this IFunctionsWorkerApplicationBuilder worker)
        {
            worker.UseMiddleware<FunctionContextAccessorMiddleware>();
            worker.UseMiddleware<HttpCompressionMiddleware>();
            worker.UseMiddleware<ExceptionHandlerMiddleware>();

            return worker;
        }
    }
}
