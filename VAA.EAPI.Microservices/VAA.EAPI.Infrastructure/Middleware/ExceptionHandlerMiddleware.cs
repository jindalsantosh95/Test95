using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions;

namespace VAA.EAPI.Infrastructure.Middleware
{
    public sealed class ExceptionHandlerMiddleware : IFunctionsWorkerMiddleware
    {
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                var logger = context.GetLogger<ExceptionHandlerMiddleware>();

                if (ex is AggregateException)
                {
                    foreach (var innerEx in ((AggregateException)ex).InnerExceptions)
                        logger.LogError(innerEx, $"Error Captured in ExceptionHandlerMiddleware - CorrelationId: {AsyncContext.Current?.CorrelationId} - {ex.Message}\n", innerEx.StackTrace);
                }
                else
                    logger.LogError(ex, $"Error Captured in ExceptionHandlerMiddleware - CorrelationId: {AsyncContext.Current?.CorrelationId} - {ex.Message}\n", ex.StackTrace);

                if (context.FunctionDefinition.InputBindings.Any(x => x.Value.Type.Is("httpTrigger")))
                {
                    var response = (await context.GetHttpRequestDataAsync()).CreateResponse(HttpStatusCode.InternalServerError);
                    await context.InvokeResult(response, logger, ex);
                }
                else throw;
            }
        }
    }
}
