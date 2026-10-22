using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Functions;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Tests;

public static class Setup
{
    public static void CreateTestEnvironmentSettings()
    {
        AsyncContext.Current = new AsyncContext { Context = new TestFunctionContext() };

        Dictionary<string, string> settings = new()
        {
            { "StoreLocator:MsUrl", "https://Testresources.virginatlantic.com/dev/storelocator" },
            { "StoreLocator:OcpSubID", "test-ocp-key" }
        };

        foreach (var item in settings)
        {
            ConfigReader.Set(item.Key, item.Value);
        }
    }
}
