using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Account.Mutations;
using VAA.EAPI.GraphQL.Domain.Account.Mutations.Secure;
using VAA.EAPI.GraphQL.Domain.Account.Queries;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests;

[Collection("EnvironmentDependentTests")]
public sealed class AccountLinkingRegistrationTests
{
    [Theory]
    [InlineData("Public", false, false)]
    [InlineData("Secure", true, true)]
    [InlineData("Public|Secure", false, true)]
    [InlineData("Public | secure", false, true)]
    [InlineData("Public|Insecure", false, false)]
    public void RedemptionRegistrationRecognizesSecureModeWithoutChangingLegacyRegistration(string mode, bool hostIsSecure, bool expected)
    {
        const string key = "OperationsVisibilityMode";
        var previous = ConfigReader.Try(key);
        try
        {
            ConfigReader.Set(key, mode);
            var services = new ServiceCollection().AddSingleton<Startup>();
            var builder = services.AddGraphQL();
            using var serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetRequiredService<Startup>().RegisterMutations(builder, hostIsSecure);
            Assert.Equal(expected, services.Any(service => service.ServiceType == typeof(AccountLinkingMutation)));
            Assert.Equal(hostIsSecure, services.Any(service => service.ServiceType == typeof(FlyingClubTokenMutation)));
        }
        finally { ConfigReader.Set(key, previous!); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AddressLookupV3RegistrationIsAvailableOnThePublicHostWithoutSecureHostRegistration(bool hostIsSecure)
    {
        var services = new ServiceCollection().AddSingleton<Startup>();
        var builder = services.AddGraphQL();
        using var serviceProvider = services.BuildServiceProvider();
        var startup = serviceProvider.GetRequiredService<Startup>();

        startup.RegisterQueries(builder, hostIsSecure);
        startup.RegisterMutations(builder, hostIsSecure);

        Assert.Equal(!hostIsSecure, services.Any(service => service.ServiceType == typeof(AddressLookupV3Query)));
        Assert.Equal(!hostIsSecure, services.Any(service => service.ServiceType == typeof(AddressLookupV3Mutation)));
    }
}
