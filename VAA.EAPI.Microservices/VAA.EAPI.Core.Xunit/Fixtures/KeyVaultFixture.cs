using VAA.EAPI.Infrastructure.Services;

namespace VAA.EAPI.Core.Xunit.Fixtures
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