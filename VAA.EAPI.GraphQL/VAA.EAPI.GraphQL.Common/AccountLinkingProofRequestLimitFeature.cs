namespace VAA.EAPI.GraphQL.Common
{
    /// <summary>Marks a request too large for proof-specific transport inspection. Other operations remain unaffected.</summary>
    public sealed class AccountLinkingProofRequestLimitFeature
    {
        public const int MaximumInspectionBytes = 65536;
    }
}
