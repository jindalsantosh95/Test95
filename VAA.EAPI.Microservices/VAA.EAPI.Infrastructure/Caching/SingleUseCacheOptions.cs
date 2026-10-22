using System;

namespace VAA.EAPI.Infrastructure.Caching
{
    /// <summary>Per-call policy for entries sharing a fixed consumption window.</summary>
    public sealed class SingleUseCacheOptions
    {
        public string ScopeKey { get; init; }
        public string Binding { get; init; }
        public string Owner { get; init; }
        public string IssuanceId { get; init; }
        public TimeSpan ScopeLifetime { get; init; }
        public int MaximumReplacements { get; init; }
    }
}
