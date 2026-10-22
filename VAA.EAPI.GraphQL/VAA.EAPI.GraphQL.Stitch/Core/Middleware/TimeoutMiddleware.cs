using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Execution.Options;
using static System.Threading.CancellationTokenSource;

namespace VAA.EAPI.GraphQL.Stitch.Core.Middleware
{
    public sealed class TimeoutMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TimeSpan _timeout;

        public TimeoutMiddleware(RequestDelegate next, IRequestExecutorOptionsAccessor options)
        {
            if (options is null) throw new ArgumentNullException(nameof(options));

            _next = next ?? throw new ArgumentNullException(nameof(next));
            _timeout = options.ExecutionTimeout;
        }

        public async ValueTask InvokeAsync(IRequestContext context)
        {
            using var timeout = new CancellationTokenSource(_timeout);

            // We do not dispose the combined token in this middleware at all times.
            // The dispose is handled in the finally block.
            var combined = CreateLinkedTokenSource(context.RequestAborted, timeout.Token);

            try
            {
                // Replace the request abort cancellation token with the newly created combined
                // token. That now tracks our request timeout as well as the outer request
                // cancellation token.
                context.RequestAborted = combined.Token;

                await _next(context).ConfigureAwait(false);

                if (timeout.IsCancellationRequested)
                    context.Result = ErrorHelper.RequestTimeout(_timeout);
            }
            catch (OperationCanceledException)
            {
                // if its not the timeout that canceled we will let somebody else handle this.
                if (!timeout.IsCancellationRequested)
                    throw;

                context.Result = ErrorHelper.RequestTimeout(_timeout);
            }
            finally
            {
                // If we return a stream we want to keep the combined token alive until
                // the stream is completed. By doing so we allow the initial cancellation token
                // to still signal to resolvers that the request was canceled.
                //
                // In the case of a stream it is still ok that we disposed the timeout since the
                // timeout is meant for the request processing itself which is finished at this
                // stage.
                if (context.Result is IResponseStream stream)
                    stream.RegisterForCleanup(combined);
                else
                    // if the result is not a stream than we can safely dispose.
                    combined.Dispose();
            }
        }
    }

    internal static class ErrorHelper
    {
        public const string ErrorHelper_RequestTimeout = "The request exceeded the configured timeout of `{0}`.";

        public static IQueryResult RequestTimeout(TimeSpan timeout)
        {
            Console.WriteLine("RequestTimeout was called");

            return QueryResultBuilder.CreateError(new Error(string.Format(ErrorHelper_RequestTimeout, timeout), ErrorCodes.Execution.Timeout));
        }
    }
}