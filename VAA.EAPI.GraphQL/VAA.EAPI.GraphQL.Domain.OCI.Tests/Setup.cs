using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Functions;

namespace VAA.EAPI.GraphQL.Domain.OCI.Tests
{
    public class Setup
    {
        public static void CreateTestEnvironmentSettings()
        {
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
            Dictionary<string, string> settings = new()
            {
                {"checkin:msurl","https://Testresources.virginatlantic.com/dev/checkinmicroservice"},
                {"checkin:ocpsubid","118c0bea17514fee8f3ab9c610da69a8"},
                { "reference:msurl", "https://Testresources.virginatlantic.com/dev/ReferenceMicroservice"},
                {"CheckIn:CheckInIdJwtKey", "dshfwq649821iuher98dwq6n4932uhdq7298u43wad31754d96932q546d32" },
                {"CheckIn:RequirementIdExpiryTimeinMin", "20"},
                {"CheckIn:TravelPartyIdExpiryTimeinMin", "20"},
                { "baggage:msurl", "https://Testresources.virginatlantic.com/dev/baggage"},
                {"Baggage:OcpSubID", "TestKey" },
                { "analytics:url","https://Testresources.virginatlantic.com/dev/analyticsMicroservice/track"},
                {"analytics:ocpsubid","a55fc84b25a843c7ab58c04195a22b2d" },
                {"BasketMS:Url", "https://testresources.virginatlantic.com/dev/basketmicroservice/basket/" },
                {"BasketMSOcpKey", "TestKey" },
                {"CheckIn:BasketIdExpiryTimeinMin", "20"},
                {"Baggage:OCITripType", "OLCI"},
                { "PaymentMS:Url", "https://Testresources.virginatlantic.com/dev/orderpaymentmicroservice/order/payment/"},
                { "TRIPMS:MSUrl", "https://Testresources.virginatlantic.com/DEV/digital/Trip"},
                { "TRIPMS:OcpSubID", "7ec41bf046024f0084c3b42f66ea4e3f"},
                {"book:paymentms:ocpsubid", "testocpkey" },
                { "tripms:bookingidjwtkey", "dshfwq649821iuher98dwq6n4932uhdq7298u43wad31754d96932q546d32"},
                { "CheckIn:PaxOpsJwtTokenExpiryTimeinDays", "1" },
                { "Identity:MsUrl", "https://Testresources.virginatlantic.com/dev/identityMicroservice" },
                {"MyAccount:IdentityMSURLForLoginTokenValidation","https://Testresources.virginatlantic.com/dev/identitymicroservice/ValidateAuthToken"},
                {"IdentityMS:AppId","testAppid"},
                {"OrderDataProvider:MsUrl","https://Testresources.virginatlantic.com/dev/orderdata"},
                {"identity:ocpsubid","ocpkeyvalue" },
                {"eapigraph:appid","testappids" },
                {"eapigraph:clientsecret","testsecret" },
                {"aad:authority","https://login.microsoftonline.com/" },
                {"aad:tenantid","00000000-4f89-11d3-9a0c-0305e82c3301" },
                {"Myaccount:WalletGeneratePassURL", "https://Testresources.virginatlantic.com/dev/walletMicroService/"},
                {"CommonEAPIOcpAPIMKey", "testocpkey" }
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);
        }
    }
}
