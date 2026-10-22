using System;
using System.Collections.Generic;

namespace VAA.EAPI.Infrastructure.PSS.Tests
{
    public class PSSTestsCommon
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new()
            {
                {"BrandAPITokenClientId", "123"},
                {"BrandAPITokenClientSecret","abc"},
                {"BrandAPITokenUrl","https://stage-pvt-apigateway.vs01.vs.air4.com/v1/oauth/token"},
                {"BrandAPIUrl","https://stage-pvt-apigateway.vs01.vs.air4.com/rm/shopping/v1/brands/products/segments/cos"},
                {"ApplicationID","ND"},
                {"HttpClientTimeOutInMilliSeconds", "5000" },
                {"ApplicationName", "TestName" },
                {"ApplicationVersion", "DC" },
                {"CreateUserID", "DC"},
                {"AuthorizationToSave", "Y"}
            };

            foreach (var item in settings)
            {
                Environment.SetEnvironmentVariable(item.Key, item.Value, EnvironmentVariableTarget.Process);
            }
        }
    }
}
