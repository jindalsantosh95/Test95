using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Diagnostics.CodeAnalysis;
using VAA.EAPI.Common.Serialisation;

namespace VAA.Facade.Test.Framework
{
    [ExcludeFromCodeCoverage]
    public static class TestHelpers
    {
        public static ClaimsPrincipal TestUser()
        {
            var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Name, "john.doe@somewhere.com"),
                    new Claim(ClaimTypes.NameIdentifier, "JDOE0001"),
                    new Claim("name", "John Doe"),
                };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            return claimsPrincipal;
        }



        public static class TestDataProvider
        {
            public static T GetTestData<T>(string dataKey)
            {
                // Load the test data file 
                string testDataFileContent = File.ReadAllText($"TestDataSamples/{dataKey.Replace('\\', '/')}.json");

                // Convert the test data into the required object
                T response;
                if (typeof(T) == typeof(string))
                {
                    response = (T)Convert.ChangeType(testDataFileContent, typeof(T));
                }
                else
                {
                    response = Json.Deserialize<T>(testDataFileContent);
                }

                return response;
            }
        }
    }
}
