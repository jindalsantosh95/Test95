using Azure.Security.KeyVault.Keys.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.FlyingClubToken;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Identity.Triggers.Http;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests;

[Collection("EnvironmentDependentTests")]
public sealed class FlyingClubTokenControllerTests
{
    [Fact]
    public void OnlyOrdinaryTokenEndpointsRemain()
    {
        Assert.Null(typeof(AccountLinkingController).GetMethod("IssueLinkedTokenAsync"));
        Assert.Null(typeof(FlyingClubTokenController).GetMethod("ValidateLinkedAsync"));
        Assert.NotNull(typeof(FlyingClubTokenController).GetMethod("IssueAsync"));
        Assert.NotNull(typeof(FlyingClubTokenController).GetMethod("ValidateAsync"));
    }

    [Theory]
    [InlineData(false, HttpStatusCode.OK)]
    [InlineData(true, HttpStatusCode.Unauthorized)]
    public async Task ValidationEndpointEnforcesItsTokenPurpose(bool linkedToken, HttpStatusCode expected)
    {
        const string keyId = "https://synthetic.vault.azure.net/keys/test/version";
        ConfigReader.Set(Constants.Environment.FlyingClubTokenKeyId, keyId);
        ConfigReader.Set(Constants.Environment.FlyingClubTokenAuthorizedParty, "graphql-app");
        ConfigReader.Set(Constants.Environment.FlyingClubTokenIssuer, "https://identity.example.test");
        ConfigReader.Set(Constants.Environment.FlyingClubTokenAudience, "summary-test");
        ConfigReader.Set(Constants.Environment.FlyingClubTokenClaimType, "flyingClubId");
        using var rsa = RSA.Create(2048);
        var client = new CryptographyClient(new Azure.Security.KeyVault.Keys.JsonWebKey(rsa) { Id = keyId });
        var validatorConstructor = typeof(KeyVaultFlyingClubTokenValidator).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic,
            null, [typeof(CryptographyClient), typeof(TimeProvider)], null)!;
        using var services = new ServiceCollection().AddLogging().AddSingleton(TimeProvider.System)
            .AddSingleton<IFlyingClubTokenValidator>(_ => (KeyVaultFlyingClubTokenValidator)validatorConstructor.Invoke([client, TimeProvider.System]))
            .AddSingleton<IFlyingClubTokenValidationHandler, FlyingClubTokenValidationHandler>()
            .AddSingleton<IFlyingClubTokenSigner, KeyVaultFlyingClubTokenSigner>()
            .AddSingleton<IFlyingClubTokenHandler, FlyingClubTokenHandler>()
            .AddSingleton<FlyingClubTokenController>().BuildServiceProvider();
        var claims = new List<Claim> { new("azp", "graphql-app"), new("flyingClubId", "1234567890") };
        if (linkedToken) claims.Add(new Claim("token_use", "linked"));
        var token = new JwtSecurityToken("https://identity.example.test", "summary-test", claims, DateTime.UtcNow.AddSeconds(-1),
            DateTime.UtcNow.AddMinutes(2), new SigningCredentials(new RsaSecurityKey(rsa) { KeyId = keyId }, SecurityAlgorithms.RsaSha256));
        var body = Newtonsoft.Json.JsonConvert.SerializeObject(new { accessToken = new JwtSecurityTokenHandler().WriteToken(token) });
        var context = new TestFunctionContext();
        context.Features.Set(new JwtPrincipalFeature(new ClaimsPrincipal(new ClaimsIdentity([new Claim("azp", "graphql-app")], "test")), null!));
        var request = new TestHttpRequestData(context, new Uri("https://identity.example.test/validate"), new MemoryStream(Encoding.UTF8.GetBytes(body)), "POST");
        var controller = services.GetRequiredService<FlyingClubTokenController>();
        var response = await controller.ValidateAsync(request, context, TestContext.Current.CancellationToken);
        Assert.Equal(expected, response.StatusCode);
    }
}
