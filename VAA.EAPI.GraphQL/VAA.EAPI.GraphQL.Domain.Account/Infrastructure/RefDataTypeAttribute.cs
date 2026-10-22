namespace VAA.EAPI.GraphQL.Domain.Account.Infrastructure
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class RefDataTypeAttribute : Attribute
    {
        public string CacheKey { get; }
        public string UrlSuffix { get; }
        public string FilterField { get; }

        public RefDataTypeAttribute() { }

        public RefDataTypeAttribute(string urlSuffix, string cacheName) : this(urlSuffix, cacheName, string.Empty) { }
        public RefDataTypeAttribute(string urlSuffix, string cacheName, string filterField)
        {
            UrlSuffix = urlSuffix;
            CacheKey = cacheName;
            FilterField = filterField;
        }
    }
}
