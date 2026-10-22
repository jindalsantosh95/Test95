using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Core.Xunit.Fixtures
{
    public sealed class KeyVaultFixture : IDisposable
    {
        public KeyVault Current { get; }

        public KeyVaultFixture()
        {
            Current = KeyVault.Default;
        }

        public void Dispose()
        {
        }
    }
}