using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.GraphQL.Common.Http
{
    internal sealed class CompletionTrackingHttpContent : HttpContent
    {
        private readonly HttpContent content;
        private readonly Action<long> onCompleted;
        private readonly long? contentLength;
        private int completionSignalled;
        private int disposed;

        public CompletionTrackingHttpContent(HttpContent content, Action<long> onCompleted)
        {
            this.content = content ?? throw new ArgumentNullException(nameof(content));
            this.onCompleted = onCompleted ?? throw new ArgumentNullException(nameof(onCompleted));
            contentLength = content.Headers.ContentLength;

            foreach (var header in content.Headers)
                Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            => SerializeToStreamAsync(stream, context, CancellationToken.None);

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context, CancellationToken cancellationToken)
        {
            await using var source = new CompletionTrackingStream(
                await content.ReadAsStreamAsync(cancellationToken),
                Complete);

            await source.CopyToAsync(stream, cancellationToken);
        }

        protected override async Task<Stream> CreateContentReadStreamAsync()
            => new CompletionTrackingStream(await content.ReadAsStreamAsync(), Complete);

        protected override async Task<Stream> CreateContentReadStreamAsync(CancellationToken cancellationToken)
            => new CompletionTrackingStream(await content.ReadAsStreamAsync(cancellationToken), Complete);

        protected override bool TryComputeLength(out long length)
        {
            if (contentLength is long knownLength)
            {
                length = knownLength;
                return true;
            }

            length = 0;
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && Interlocked.Exchange(ref disposed, 1) == 0)
            {
                Complete(contentLength ?? 0);
                content.Dispose();
            }

            base.Dispose(disposing);
        }

        private void Complete(long contentLength)
        {
            if (Interlocked.Exchange(ref completionSignalled, 1) == 0)
                onCompleted(contentLength);
        }

        private sealed class CompletionTrackingStream : Stream
        {
            private readonly Stream stream;
            private readonly Action<long> onCompleted;
            private long bytesRead;
            private int completionSignalled;

            public CompletionTrackingStream(Stream stream, Action<long> onCompleted)
            {
                this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
                this.onCompleted = onCompleted ?? throw new ArgumentNullException(nameof(onCompleted));
            }

            public override bool CanRead => stream.CanRead;
            public override bool CanSeek => stream.CanSeek;
            public override bool CanWrite => false;
            public override long Length => stream.Length;
            public override long Position
            {
                get => stream.Position;
                set => stream.Position = value;
            }

            public override void Flush() => stream.Flush();
            public override Task FlushAsync(CancellationToken cancellationToken) => stream.FlushAsync(cancellationToken);
            public override long Seek(long offset, SeekOrigin origin) => stream.Seek(offset, origin);
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

            public override int Read(byte[] buffer, int offset, int count)
            {
                var countRead = stream.Read(buffer, offset, count);
                Track(countRead);
                return countRead;
            }

            public override int Read(Span<byte> buffer)
            {
                var countRead = stream.Read(buffer);
                Track(countRead);
                return countRead;
            }

            public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                var countRead = await stream.ReadAsync(buffer, offset, count, cancellationToken);
                Track(countRead);
                return countRead;
            }

            public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            {
                var countRead = await stream.ReadAsync(buffer, cancellationToken);
                Track(countRead);
                return countRead;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    Complete();
                    stream.Dispose();
                }

                base.Dispose(disposing);
            }

            public override async ValueTask DisposeAsync()
            {
                Complete();
                await stream.DisposeAsync();
                GC.SuppressFinalize(this);
            }

            private void Track(int countRead)
            {
                if (countRead == 0)
                    Complete();
                else
                    bytesRead += countRead;
            }

            private void Complete()
            {
                if (Interlocked.Exchange(ref completionSignalled, 1) == 0)
                    onCompleted(bytesRead);
            }
        }
    }
}
