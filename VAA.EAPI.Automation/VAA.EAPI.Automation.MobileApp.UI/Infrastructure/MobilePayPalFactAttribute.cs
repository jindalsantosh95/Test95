using Microsoft.Extensions.Configuration;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.UI.Factory;

namespace VAA.EAPI.Automation.MobileApp.UI.Infrastructure;

public sealed class MobilePayPalFactAttribute : FactAttribute
{
    public MobilePayPalFactAttribute()
    {
        IConfiguration configuration = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json").AddJsonFile("local.settings.local.json", optional: true)
            .AddEnvironmentVariables().Build();
        if (!CanRun(configuration))
            Skip = "PayPal readiness requires explicit local SIT opt-in and confirmed credential-safe driver logging.";
    }

    internal static bool CanRun(IConfiguration configuration)
        => configuration["RunMode"].Is("Local") && configuration["Platform"].Is("iOS")
            && IosProcessArguments.ResolveConfiguredBuildProfile(configuration) == "sit_standard"
            && configuration.GetValue<bool>("PayPal:Enabled")
            && configuration.GetValue<bool>("PayPal:AllowSandboxPayment")
            && configuration.GetValue<bool>("PayPal:CredentialSafeLoggingConfirmed")
            && !configuration.GetValue<bool>("FaultInjection:Enabled");
}
