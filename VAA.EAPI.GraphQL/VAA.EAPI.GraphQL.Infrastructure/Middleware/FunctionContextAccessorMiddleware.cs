using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common;

namespace VAA.EAPI.GraphQL.Infrastructure.Middleware
{
    public sealed class FunctionContextAccessorMiddleware : IFunctionsWorkerMiddleware
    {
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var previous = AsyncContext.Current;
            var current = await AsyncContext.CreateAsync(context);
            AsyncContext.Current = current;

            try
            {
                await next(context);
            }
            finally
            {
                current.Complete();
                AsyncContext.Current = previous;
            }
        }
    }
}
