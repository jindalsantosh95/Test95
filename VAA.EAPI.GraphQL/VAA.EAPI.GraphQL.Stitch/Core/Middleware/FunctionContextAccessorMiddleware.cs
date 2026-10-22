using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace VAA.EAPI.GraphQL.Stitch.Core.Middleware
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
