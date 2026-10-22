using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests
{
    [CollectionDefinition("EnvironmentDependentTests", DisableParallelization = true)]
    public class EnvironmentDependentTestsCollection
    {
    }

    public class IdentityTestsCommon
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new()
            {
                {"ApplicationId", "ND"},
                {"DebugOptions", "Y"},
                {"TestLab", "TVTH" },
                {"PoolType", "STAMPING/VAA/NEW/DIST/CAPABILITY"},
                {"AgentCityCode", "LON"},
                {"RoleIndicator", "RESNDCSALES"},
                {"SecurityId", "D027954"},
                {"OptionNumberForDisplayRequest", "1"},
                {"ProvideResidualValue", "N"},
                {"TrustedSource", "Y"},
                {"SignInEmailAddress", "emailAddress"},
                {"SignInUserName", "username"},
                {"TenantId", "42db763d-91b9-4d05-9811-6280695a5cae"},
                {"ClientId", "275687af-116a-4172-82a1-36a633983a58"},
                {"ClientSecret", "qbe8Q~dO0mqGg54nz8h7jeNCth1GsfbV1ob3IbyS"},
                {"GraphAPIUrl", "https://tempuri.org/"},
                {"GraphApiUrlVersion", "v1.0"},
                {"graphapiurlproxyaddressversion", "beta"},
                {"B2CDomain", "avateunloyaltyfcb2c.onmicrosoft.com"},
                {"AuthenticationUrl", "https://login.microsoftonline.com/"},
                {"ReferenceDataMicroServiceDBKey", "TestDBKey" },
                {"OTPGenerateTableName", "OTPRequest"},
                {"RandomDigitMinValue", "100000"},
                {"RandomDigitMaxValue", "1000000"},
                {"ContinuationTokenRandomDigitMinValue", "10000000"},
                {"ContinuationTokenRandomDigitMaxValue", "100000000"},
                {"OTPDigitLength", "6"},
                {"OtpEntraDigitLength", "8"},
                {"OTPExpiryTimeLimit", "10"},
                {"OTPGenerationLimit", "5" },
                {"b2cdomain", "TestDomain" },
                {"emailupdatetemplate", "Testtemplate" },
                { "OtpLimit", "5" },
                { "validationjwtkey", "TestkeySRECDSADSADSAREWEF£76545$£==+" },
                {"otptokenexpirytime", "10" },
                {"IdentityActivationTokenIssuer", "test-issuer" },
                {"MyAccountActivationTokenIssuer", "test-issuer" },
                {"EntraAPISubDomain", "avateunloyaltyfcb2c" },
                {"EntraClientId", "3457e949-4162-4ada-8374-ee2e7c0d6a9c" },
                {"EntraTenantId", "42db763d-91b9-4d05-9811-6280695a5cad" },
                {"EntraAPIurlDomain", "avateunloyaltyfcb2c.ciamlogin.com/avateunloyaltyfcb2c.onmicrosoft.com" },
                {"ExtensionPrefix", "extension_b4e6641f44c644ea8b9a899e786c7579_" },
                {"CustomMemberStatusAttribute", "memberStatus" },
                {"Identities", "identities" },
                {"entraapiscope", "test" },
                {"WelcomeEmailTemplate", "vaaMyAccRePlatWC" },
                {"AllowTestEmail","N" },
                {"EmailTriggerTopicName", "test" },
                {"CustomerSyncTopicName","test" },
                {"RetryCounter","2" },
                {"VredMarketingFeatureFlag","Y" },
                {"AccountEnabled","AccountEnabled" },
                {"Surname","Surname" },
                {"DisplayName","DisplayName" },
                {"GivenName","GivenName" },
                {"PasswordProfile","PasswordProfile" },
                {"PasswordPolicies","PasswordPolicies" },
                {"UserPrincipalName","UserPrincipalName" },                
                {"CustomMemberTypeAttribute","MemberType" },                
                {"CustomCustomerIdAttribute","CustomerId" },                
                {"EntraLogging", "true" }
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);;
        }
    }
}
