using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System;
using System.Linq;
using System.Threading;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Infrastructure.Services
{
    public sealed class KeyVault : IKeyVault
    {
        private static object @lock = new();
        private static KeyVault @default;

        public static KeyVault Default
        {
            get
            {
                if (@default == null)
                    lock (@lock)
                    {
                        var keyVaultUri = ConfigReader.Get(Common.Constants.Application.KeyVaultUri);
                        var keyVaultTenantID = ConfigReader.Try(Common.Constants.Application.KeyVaultTenantID);
                        var keyVaultClientID = ConfigReader.Try(Common.Constants.Application.KeyVaultClientID);
                        var keyVaultClientSecret = ConfigReader.Try(Common.Constants.Application.KeyVaultClientSecret);

                        var useConfig = System.Diagnostics.Debugger.IsAttached && !string.IsNullOrWhiteSpace(keyVaultTenantID) && !string.IsNullOrWhiteSpace(keyVaultClientID) && !string.IsNullOrWhiteSpace(keyVaultClientSecret);

                        @default ??= useConfig ? new(keyVaultUri, keyVaultTenantID, clientId: keyVaultClientID, clientSecret: keyVaultClientSecret) : new(keyVaultUri);
                    }

                return @default;
            }
        }

        public static readonly SecretClientOptions SecretClientOptions = new()
        {
            Retry =
            {
                Delay= TimeSpan.FromSeconds(2),
                MaxDelay = TimeSpan.FromSeconds(16),
                MaxRetries = 5,
                Mode = RetryMode.Exponential
            }
        };

        private SecretClient secretClient;

        public KeyVault() { }
        public KeyVault(string url) : this() => Connect(url);
        public KeyVault(string url, string tenantId, string clientId, string clientSecret) : this() => Connect(url, tenantId, clientId, clientSecret);
        public KeyVault(string url, TokenCredential credential) : this() => Connect(url, credential);

        public void Connect(string url, SecretClientOptions secretClientOptions = null) => secretClient = new SecretClient(new Uri(url), new DefaultAzureCredential(), secretClientOptions ?? SecretClientOptions);
        public void Connect(string url, string tenantId, string clientId, string clientSecret, SecretClientOptions secretClientOptions = null) => Connect(url, new ClientSecretCredential(tenantId, clientId, clientSecret), secretClientOptions ?? SecretClientOptions);
        public void Connect(string url, TokenCredential credential, SecretClientOptions secretClientOptions = null) => secretClient = new SecretClient(new Uri(url), credential, secretClientOptions ?? SecretClientOptions);

        public Response<KeyVaultSecret> SetSecret(string name, string value, CancellationToken cancellationToken = default) => secretClient.SetSecret(name, value, cancellationToken);

        public Response<KeyVaultSecret> GetSecret(string name, string version = null, CancellationToken cancellationToken = default)
        {
            try
            {
                return secretClient.GetSecret(name, version, cancellationToken);
            }
            catch (Exception ex)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debug.WriteLine(ex.ToString());

                return null;
            }
        }

        public string GetSecretValue(string name, string version = null, CancellationToken cancellationToken = default) => GetSecret(name, version, cancellationToken)?.Value?.Value ?? null;

        public string[] GetAllSecretNames(CancellationToken cancellationToken = default) => secretClient.GetPropertiesOfSecrets(cancellationToken).Select(x => x.Name).ToArray();
    }
}
