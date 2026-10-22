using System.Collections.Concurrent;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.Core
{
    /// <summary>
    /// Allows the reading of secrets from the Key vault by passing in the tenantId, clientId, clientSecret
    /// Example: var vault = new Vault(url, tenantId, clientId, clientSecret);
    /// and to get all key values from the vault example: var secretNames = vault.GetAllSecretNames();
    /// and to retrieve secrets for a given secret name example: var fromVault = vault.GetSecretValue("TheSecretName");
    /// </summary>
    public sealed class Vault
    {
        private static object @lock = new();
        private static Vault @default;
        private readonly static ConcurrentDictionary<string, Response<KeyVaultSecret>> cache = new();
        private readonly static ConcurrentDictionary<string, string> failures = new(StringComparer.OrdinalIgnoreCase);

        public static string LastFailure { get; private set; } = string.Empty;

        public static string GetLastFailure(string secretName) =>
            failures.TryGetValue(secretName, out string failure) ? failure : string.Empty;


        public static Vault Default
        {
            get
            {
                if (@default == null)
                    lock (@lock)
                    {
                        string keyVaultUri = ConfigReader.Get(Constants.Environment.KeyVaultUrl);
                        string keyVaultTenantID = ConfigReader.Get(Constants.Environment.KeyVaultTenantId);
                        string keyVaultClientID = ConfigReader.Get(Constants.Environment.KeyVaultClientId);
                        string keyVaultClientSecret = ConfigReader.Get(Constants.Environment.KeyVaultClientSecret);

                        bool useConfig = !keyVaultTenantID.IsNullOrWhiteSpace() && !keyVaultClientID.IsNullOrWhiteSpace() && !keyVaultClientSecret.IsNullOrWhiteSpace();

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

        public Vault() { }
        public Vault(string url) : this() => Connect(url);
        public Vault(string url, string tenantId, string clientId, string clientSecret) : this() => Connect(url, tenantId, clientId, clientSecret);
        public Vault(string url, TokenCredential credential) : this() => Connect(url, credential);

        public void Connect(string url, SecretClientOptions secretClientOptions = null) => secretClient = new SecretClient(new Uri(url), new DefaultAzureCredential(), secretClientOptions ?? SecretClientOptions);
        public void Connect(string url, string tenantId, string clientId, string clientSecret, SecretClientOptions secretClientOptions = null) => Connect(url, new ClientSecretCredential(tenantId, clientId, clientSecret), secretClientOptions ?? SecretClientOptions);
        public void Connect(string url, TokenCredential credential, SecretClientOptions secretClientOptions = null) => secretClient = new SecretClient(new Uri(url), credential, secretClientOptions ?? SecretClientOptions);

        public Response<KeyVaultSecret> SetSecret(string name, string value, CancellationToken cancellationToken = default, ITestOutputHelper testOutputHelper = null)
        {
            if (name.IsNullOrWhiteSpace())
                return null;

            Response<KeyVaultSecret> response = null;

            try
            {
                response = secretClient.SetSecret(name, value, cancellationToken);
            }
            catch (Exception ex)
            {
                WriteSafeFailure("set", name, ex, testOutputHelper);
            }

            return response;
        }

        public Response<KeyVaultSecret> GetSecret(string name, string version = null, CancellationToken cancellationToken = default, ITestOutputHelper testOutputHelper = null)
        {
            if (name.IsNullOrWhiteSpace())
                return null;

            string key = $"{name}/{version}";
            Response<KeyVaultSecret> response = null;

            try
            {
                if (cache.ContainsKey(key))
                    return cache[key];

                response = secretClient.GetSecret(name, version, cancellationToken);

                if (response is null)
                    return null;

                while (!cache.ContainsKey(key) && !cache.TryAdd(key, response))
                    if (!Thread.Yield())
                        Thread.Sleep(0);
            }
            catch (Exception ex)
            {
                WriteSafeFailure("get", name, ex, testOutputHelper);
            }

            return response;
        }

        public string GetSecretValue(string name, string version = null, CancellationToken cancellationToken = default, ITestOutputHelper testOutputHelper = null) => GetSecret(name, version, cancellationToken, testOutputHelper)?.Value?.Value ?? null;

        private static void WriteSafeFailure(
            string operation,
            string secretName,
            Exception exception,
            ITestOutputHelper testOutputHelper)
        {
            List<string> exceptionTypes = new();
            RequestFailedException requestFailure = null;
            for (Exception current = exception; current != null; current = current.InnerException)
            {
                exceptionTypes.Add(current.GetType().Name);
                requestFailure ??= current as RequestFailedException;
            }

            string status = requestFailure == null ? "unavailable" : requestFailure.Status.ToString();
            string errorCode = requestFailure?.ErrorCode;
            if (errorCode.IsNullOrWhiteSpace())
            {
                errorCode = exceptionTypes
                    .Select((_, index) => GetExceptionAtDepth(exception, index))
                    .Select(current => current?.GetType().GetProperty("ErrorCode")?.GetValue(current)?.ToString())
                    .FirstOrDefault(code => !code.IsNullOrWhiteSpace()) ?? "unavailable";
            }
            string diagnostic =
                $"Key Vault {operation} failed for secret '{secretName}'. " +
                $"Status: {status}; ErrorCode: {errorCode}; " +
                $"ExceptionTypes: {string.Join(" -> ", exceptionTypes)}. " +
                "Secret values and credentials are not logged.";
            LastFailure = diagnostic;
            failures[secretName] = diagnostic;

            if (testOutputHelper == null)
            {
                Console.Error.WriteLine(diagnostic);
            }
            else
            {
                testOutputHelper.WriteLine(diagnostic);
            }
        }

        private static Exception GetExceptionAtDepth(Exception exception, int depth)
        {
            Exception current = exception;
            for (int index = 0; index < depth && current != null; index++)
            {
                current = current.InnerException;
            }

            return current;
        }

        public string[] GetAllSecretNames(CancellationToken cancellationToken = default, ITestOutputHelper testOutputHelper = null)
        {
            string[] response = null;

            try
            {
                response = secretClient.GetPropertiesOfSecrets(cancellationToken).Select(x => x.Name).ToArray();
            }
            catch (Exception ex)
            {
                testOutputHelper?.WriteLine(ex.ToString());
            }

            return response;
        }
    }
}
