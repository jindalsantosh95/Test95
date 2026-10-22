namespace VAA.EAPI.GraphQL.WebSocketHost
{
    internal sealed class BridgeResponseStream : MemoryStream
    {
        private const int MaximumLength = 2 * 1024 * 1024;
        private readonly Func<Task> start;

        public BridgeResponseStream(Func<Task> start)
        {
            this.start = start;
        }

        private void ValidateLength(long length)
        {
            if (length > MaximumLength) { throw new InvalidDataException("Internal bridge response exceeded its limit."); }
        }

        public override void SetLength(long value)
        {
            ValidateLength(value);
            base.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            ValidateLength(Position + count);
            start().GetAwaiter().GetResult();
            base.Write(buffer, offset, count);
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            ValidateLength(Position + buffer.Length);
            start().GetAwaiter().GetResult();
            base.Write(buffer);
        }

        public override void WriteByte(byte value)
        {
            ValidateLength(Position + 1);
            start().GetAwaiter().GetResult();
            base.WriteByte(value);
        }

        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            ValidateLength(Position + buffer.Length);
            await start();
            base.Write(buffer.Span);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken token) =>
            WriteAsync(buffer.AsMemory(offset, count), token).AsTask();

        public override void Flush() => start().GetAwaiter().GetResult();

        public override async Task FlushAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            await start();
        }
    }
}
