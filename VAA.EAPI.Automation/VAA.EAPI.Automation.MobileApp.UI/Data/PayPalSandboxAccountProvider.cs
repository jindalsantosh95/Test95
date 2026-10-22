using Microsoft.Extensions.Configuration;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.TestDataConfig.APMPayment;
using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;

namespace VAA.EAPI.Automation.MobileApp.UI.Data;

internal static class PayPalSandboxAccountProvider
{
    internal static APMDetails Retrieve(IConfiguration configuration)
    {
        if (!MobilePayPalFactAttribute.CanRun(configuration))
            throw new InvalidOperationException("PayPal local SIT privacy prerequisites are not satisfied.");
        string url = configuration["Core:KeyVaultUrl"] ?? string.Empty;
        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? vaultUri) || vaultUri.Scheme != "https" ||
            !vaultUri.Host.EndsWith(".vault.azure.net", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Configure Core:KeyVaultUrl for the approved SIT vault.");
        string tenant = configuration["Core:KeyVaultTenantId"] ?? string.Empty;
        string client = configuration["Core:KeyVaultClientId"] ?? string.Empty;
        string secret = configuration["Core:KeyVaultClientSecret"] ?? string.Empty;
        bool configuredIdentity = new[] { tenant, client, secret }.All(value => !value.IsNullOrWhiteSpace());
        if (!configuredIdentity && new[] { tenant, client, secret }.Any(value => !value.IsNullOrWhiteSpace()))
            throw new InvalidOperationException("Core Key Vault service-principal settings must be all set or all empty.");
        Vault vault = configuredIdentity ? new Vault(url, tenant, client, secret) : new Vault(url);
        // Reuse the Core connector and the same secret names as Core's web PayPal data provider.
        APMDetails account = new()
        {
            Email = vault.GetSecretValue("PayPal-Test-Email"),
            Password = vault.GetSecretValue("PayPal-Test-Password"),
        };
        if (account.Email.IsNullOrWhiteSpace() || account.Password.IsNullOrWhiteSpace())
            throw new InvalidOperationException("Required PayPal sandbox secrets were unavailable; values omitted.");
        return account;
    }
}
