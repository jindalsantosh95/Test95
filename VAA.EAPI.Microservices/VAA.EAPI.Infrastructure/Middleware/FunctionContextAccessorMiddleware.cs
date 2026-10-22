using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging;

namespace VAA.EAPI.Infrastructure.Middleware
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
                RestoreServiceBusTraceContext(context, current);
                await next(context);
            }
            finally
            {
                current.Complete();
                AsyncContext.Current = previous;
            }
        }

        private static void RestoreServiceBusTraceContext(FunctionContext functionContext, AsyncContext asyncContext)
        {
            if (!functionContext.BindingContext.BindingData.TryGetValue("ApplicationProperties", out var applicationProperties))
                return;

            switch (applicationProperties)
            {
                case IReadOnlyDictionary<string, object> properties:
                    properties.RestoreEapiTraceContext(asyncContext);
                    break;
                case IDictionary<string, object> properties:
                    new Dictionary<string, object>(properties).RestoreEapiTraceContext(asyncContext);
                    break;
                case string serializedProperties:
                    serializedProperties.RestoreEapiTraceContext(asyncContext);
                    break;
            }
        }
    }
}
