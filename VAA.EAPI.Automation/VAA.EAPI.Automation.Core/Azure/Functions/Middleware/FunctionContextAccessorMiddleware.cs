using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace VAA.EAPI.Automation.Core.Azure.Functions.Middleware
{
    public sealed class FunctionContextAccessorMiddleware : IFunctionsWorkerMiddleware
    {
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            AsyncContext.Current = new AsyncContext { Context = context };

            await next(context);
        }
    }
}
