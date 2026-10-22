using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.AccountLinking;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests;

[Collection("EnvironmentDependentTests")]
public sealed class LinkingTransactionValidatorTests : IDisposable
{
    private readonly ServiceProvider _services;
    private readonly string? _client;
    private readonly string? _resource;

    public LinkingTransactionValidatorTests()
    {
        _client = ConfigReader.Try(Constants.Environment.AccountLinkingTransactionClientId);
        _resource = ConfigReader.Try(Constants.Environment.AccountLinkingTransactionResource);
        ConfigReader.Set(Constants.Environment.AccountLinkingTransactionClientId, "alexa-client");
        ConfigReader.Set(Constants.Environment.AccountLinkingTransactionResource, "vaa-flying-club");
        _services = new ServiceCollection().AddSingleton(TimeProvider.System)
            .AddSingleton<ILinkingTransactionValidator, LinkingTransactionValidator>().BuildServiceProvider();
    }

    [Fact]
    public void PlainIdentifiersProduceMatchingBindingWithoutSigningConfiguration()
    {
        var validator = _services.GetRequiredService<ILinkingTransactionValidator>();
        var created = validator.ValidateCreation(Create());
        var redeemed = validator.ValidateRedemption(new RedeemLinkingProofRequest
        {
            TransactionId = "transaction-1", ClientId = "alexa-client", Resource = "vaa-flying-club"
        });
        Assert.Equal(created.BindingHash, redeemed.BindingHash);
        Assert.InRange((created.ExpiresAtUtc - DateTime.UtcNow).TotalSeconds, 115, 120);
        Assert.NotEqual(created.ProofReplayId, validator.ValidateCreation(Create()).ProofReplayId);
    }

    [Theory]
    [InlineData("", "alexa-client", "vaa-flying-club", "account:read")]
    [InlineData("transaction-1", "other-client", "vaa-flying-club", "account:read")]
    [InlineData("transaction-1", "alexa-client", "other-resource", "account:read")]
    [InlineData("transaction-1", "alexa-client", "vaa-flying-club", "account:write")]
    [InlineData("transaction-1", "ALEXA-CLIENT", "vaa-flying-club", "account:read")]
    [InlineData("transaction\n1", "alexa-client", "vaa-flying-club", "account:read")]
    public void RejectsInvalidOrUnapprovedBinding(string transaction, string client, string resource, string scope)
    {
        var request = Create();
        request.TransactionId = transaction;
        request.ClientId = client;
        request.Resource = resource;
        request.Scope = scope;
        Assert.ThrowsAny<SecurityTokenException>(() =>
            _services.GetRequiredService<ILinkingTransactionValidator>().ValidateCreation(request));
    }

    [Fact]
    public void DifferentTransactionsHaveDifferentBindings()
    {
        var validator = _services.GetRequiredService<ILinkingTransactionValidator>();
        var first = validator.ValidateCreation(Create());
        var request = Create();
        request.TransactionId = "transaction-2";
        Assert.NotEqual(first.BindingHash, validator.ValidateCreation(request).BindingHash);
    }

    [Fact]
    public void MissingPolicyFailsClosed()
    {
        ConfigReader.Set(Constants.Environment.AccountLinkingTransactionResource, "");
        Assert.Throws<InvalidOperationException>(() =>
            _services.GetRequiredService<ILinkingTransactionValidator>().ValidateCreation(Create()));
    }

    private static CreateLinkingProofRequest Create() => new()
    {
        TransactionId = "transaction-1", ClientId = "alexa-client",
        Resource = "vaa-flying-club", Scope = "account:read"
    };

    public void Dispose()
    {
        ConfigReader.Set(Constants.Environment.AccountLinkingTransactionClientId, _client!);
        ConfigReader.Set(Constants.Environment.AccountLinkingTransactionResource, _resource!);
        _services.Dispose();
    }
}
