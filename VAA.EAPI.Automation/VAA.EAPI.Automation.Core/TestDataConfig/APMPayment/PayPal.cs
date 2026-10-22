using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.Core.TestDataConfig.APMPayment
{
    public class PayPal : APMDetails
    {
        public PayPal()
        {
            if (Config.Environment.Is("prod"))
            {
                Email = string.Empty;
                Password = string.Empty;
                return;
            }

            Email = Vault.Default.GetSecretValue("PayPal-Test-Email");
            Password = Vault.Default.GetSecretValue("PayPal-Test-Password");
        }
    }
}
