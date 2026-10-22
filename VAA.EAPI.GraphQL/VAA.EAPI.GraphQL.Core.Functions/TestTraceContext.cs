using Microsoft.Azure.Functions.Worker;

namespace VAA.EAPI.GraphQL.Core.Functions
{
    public sealed class TestTraceContext : TraceContext
    {
        public override string TraceParent => $"00-{Guid.NewGuid():N}-abcdef1234567890-01";
        public override string TraceState => "state";
    }
}
