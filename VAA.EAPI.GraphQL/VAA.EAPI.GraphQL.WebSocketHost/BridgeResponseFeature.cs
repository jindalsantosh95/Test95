using Microsoft.AspNetCore.Http.Features;
using System.IO.Pipelines;

namespace VAA.EAPI.GraphQL.WebSocketHost
{
    internal sealed class BridgeResponseFeature : HttpResponseFeature, IHttpResponseBodyFeature, IAsyncDisposable
    {
        private readonly Stack<(Func<object, Task> Callback, object State)> starting = new();
        private readonly Stack<(Func<object, Task> Callback, object State)> completed = new();
        private PipeWriter? writer;
        private bool started;
        private bool disposed;

        public BridgeResponseFeature()
        {
            Buffer = new BridgeResponseStream(() => StartAsync());
            Body = Buffer;
        }

        public BridgeResponseStream Buffer { get; }
        public override bool HasStarted => started;
        public Stream Stream => Buffer;
        public PipeWriter Writer => writer ??= PipeWriter.Create(Buffer, new StreamPipeWriterOptions(leaveOpen: true));
        public override void OnStarting(Func<object, Task> callback, object state)
        {
            if (started) { throw new InvalidOperationException("Response has already started."); }
            starting.Push((callback, state));
        }
        public override void OnCompleted(Func<object, Task> callback, object state) => completed.Push((callback, state));
        public void DisableBuffering() { }
        public Task SendFileAsync(string path, long offset, long? count, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Internal bridge responses must be JSON, not files.");

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (started) { return; }
            while (starting.TryPop(out var callback)) { await callback.Callback(callback.State); }
            started = true;
        }

        public async Task CompleteAsync()
        {
            if (writer is not null) { await writer.FlushAsync(); }
            await StartAsync();
        }

        public async ValueTask DisposeAsync()
        {
            if (disposed) { return; }
            disposed = true;
            Exception? failure = null;
            try
            {
                if (writer is not null) { await writer.CompleteAsync(); }
            }
            catch (Exception exception) { failure = exception; }
            // Includes the Functions host's RequestServicesFeature disposal callback.
            while (completed.TryPop(out var callback))
            {
                try { await callback.Callback(callback.State); }
                catch (Exception exception) { failure ??= exception; }
            }
            await Buffer.DisposeAsync();
            if (failure is not null) { System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw(); }
        }
    }
}
