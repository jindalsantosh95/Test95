using VAA.EAPI.Automation.Core.Helpers;

namespace VAA.EAPI.Automation.MyAccount.UI.Constants
{
    public static class WifiPortal
    {
        public static readonly string BaseUrl = ConfigReader.Get("WifiPortalUrl") ?? "https://onboard.vaa-test1.virginatlantic.com/";
    }
}
