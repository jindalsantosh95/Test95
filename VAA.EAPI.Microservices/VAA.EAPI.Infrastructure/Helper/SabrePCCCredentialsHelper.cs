using VAA.EAPI.Infrastructure.Services;

namespace VAA.EAPI.Infrastructure.Helper
{
    public static class SabrePCCCredentialsHelper
    {
        public static string GetPCCUserName(string pcc)
        {
            return KeyVault.Default.GetSecretValue($"{pcc}Username");
        }

        public static string GetPCCPassword(string pcc)
        {
            return KeyVault.Default.GetSecretValue($"{pcc}Password");
        }
    }
}
