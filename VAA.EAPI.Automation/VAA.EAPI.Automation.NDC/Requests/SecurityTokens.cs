using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;

namespace VAA.EAPI.Automation.NDC.Requests
{
    public static class SecurityTokens
    {
        public const string Oauth2Token = "/6bdfd5a8-531f-4f14-9af1-dc7b444c3ad8/oauth2/token";

        public static void AddTokenForAirShoppingConfig(this TokenFixture source, string name) => source.AuthoriseFromConfig(name, Config.MicrosoftOnline.GetUrl(Oauth2Token), WebRequest.MultipartFormData.Post, Constants.Environment.NdcClientId, Constants.Environment.NdcClientSecret, Constants.Environment.NdcResource, null, Constants.Environment.NdcScope, Constants.Environment.NdcContentType);

        public static void AddTokenForOrderCreateFromConfig(this TokenFixture source, string name) => source.AuthoriseFromConfig(name, Config.MicrosoftOnline.GetUrl(Oauth2Token), WebRequest.MultipartFormData.Post, Constants.Environment.NdcClientId, Constants.Environment.NdcClientSecret, Constants.Environment.NdcResourceOrderCreate, null, Constants.Environment.NdcScope, Constants.Environment.NdcContentType);
    }
}
