using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Web.Common.Extensions;
using VAA.EAPI.Web.Common.Infrastructure.Exceptions;

namespace VAA.EAPI.Web.Common.Middleware
{
    public class ExceptionHandlerMiddleware : IFunctionsWorkerMiddleware
    {
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                var log = context.GetLogger<ExceptionHandlerMiddleware>();

                if (ex is AggregateException)
                {
                    foreach (var innerEx in ((AggregateException)ex).InnerExceptions)
                    {
                        LogException(log, innerEx);
                    }
                }
                else
                {
                    LogException(log, ex);
                }


                if (context.FunctionDefinition.InputBindings.First().Value.Type.Equals("httptrigger", StringComparison.OrdinalIgnoreCase))
                {
                    await context.SetHttpResponseStatusCode(System.Net.HttpStatusCode.InternalServerError);
                }
                else
                {
                    throw;
                }

            }
        }

        private void LogException(ILogger<ExceptionHandlerMiddleware> log, Exception ex)
        {
            if (ex is GraphQLException)
            {
                var gqlErrors = string.Join("\n", ((GraphQLException)ex).Errors.Select(er => $"{er.Message}: {er.Extensions.Code} {er.Extensions.Reason}\nResponse: {er.Extensions.Response}\n"));
                log.LogError(ex, $"GraphQL Error Captured in ExceptionHandlerMiddleware\n{gqlErrors}");
            }
            else
            {
                log.LogError(ex, "Error Captured in ExceptionHandlerMiddleware\n", ex.StackTrace);
            }
        }
    }
}
