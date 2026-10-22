using Azure;
using Azure.Security.KeyVault.Secrets;
using System.Threading;

namespace VAA.EAPI.GraphQL.Infrastructure.Interfaces
{
    public interface IKeyVault
    {
        void Connect(string url, SecretClientOptions secretClientOptions = null);
        void Connect(string url, string tenantId, string clientId, string clientSecret, SecretClientOptions secretClientOptions = null);

        Response<KeyVaultSecret> SetSecret(string name, string value, CancellationToken cancellationToken = default);

        Response<KeyVaultSecret> GetSecret(string name, string version = null, CancellationToken cancellationToken = default);

        string GetSecretValue(string name, string version = null, CancellationToken cancellationToken = default);

        string[] GetAllSecretNames(CancellationToken cancellationToken = default);
    }
}
