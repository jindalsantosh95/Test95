using HotChocolate;
using HotChocolate.Types;
using Newtonsoft.Json.Linq;
using VAA.EAPI.GraphQL.Domain.Account.Models.AccountLinking;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests;

public sealed class AccountLinkingSchemaContractTests
{
    [Fact]
    public void PublishedTypesMatchTheAgreedContract()
    {
        var schema = SchemaBuilder.New()
            .AddQueryType(descriptor => descriptor.Name("Query").Field("test").Type<StringType>().Resolve("test"))
            .AddType<InputObjectType<CreateLinkingProofInput>>()
            .AddType<InputObjectType<RedeemLinkingProofRequest>>()
            .AddType<ObjectType<CreateLinkingProofResponse>>()
            .AddType<ObjectType<RedeemLinkingProofResponse>>()
            .Create();

        var creation = schema.GetType<InputObjectType>("AccountCreateLinkingProofInput");
        Assert.Equal(new[] { "clientId", "resource", "scope", "transactionId" }, creation.Fields.Select(x => x.Name.ToString()).Order());
        Assert.IsType<IdType>(Assert.IsType<NonNullType>(creation.Fields["transactionId"].Type).Type);
        Assert.All(creation.Fields, field => Assert.IsType<NonNullType>(field.Type));
        var redemption = schema.GetType<InputObjectType>("LoyaltyRedeemLinkingProofInput");
        Assert.Equal(new[] { "clientId", "proof", "resource", "transactionId" }, redemption.Fields.Select(x => x.Name.ToString()).Order());
        Assert.All(redemption.Fields, field => Assert.IsType<NonNullType>(field.Type));
        Assert.Equal(new[] { "expiresAtUtc", "proof" }, schema.GetType<ObjectType>("AccountCreateLinkingProofPayload").Fields
            .Where(x => !x.Name.ToString().StartsWith("__")).Select(x => x.Name.ToString()).Order());
        Assert.Equal(new[] { "consentSubject", "flyingClubAccountNumber" }, schema.GetType<ObjectType>("LoyaltyRedeemLinkingProofPayload").Fields
            .Where(x => !x.Name.ToString().StartsWith("__")).Select(x => x.Name.ToString()).Order());
    }

    [Fact]
    public void SignedLinkedTokenOperationIsNotPublished()
    {
        var assembly = typeof(Startup).Assembly;
        Assert.Null(assembly.GetType("VAA.EAPI.GraphQL.Domain.Account.Mutations.Secure.LinkedFlyingClubBrokerTokenMutation"));
        Assert.DoesNotContain(typeof(Interface.IFlyingClubTokenProvider).GetMethods(),
            method => method.Name is "IssueLinkedAsync" or "ValidateLinkedAsync");
        Assert.Contains(typeof(Interface.IFlyingClubTokenProvider).GetMethods(), method => method.Name == "IssueAsync");
        Assert.Contains(typeof(Interface.IFlyingClubTokenProvider).GetMethods(), method => method.Name == "ValidateAsync");
    }

    [Fact]
    public void JsonUsesTheSameNamesAsGraphQLWithoutLegacyAliases()
    {
        var json = JObject.Parse(VAA.EAPI.GraphQL.Common.Serialisation.Json.Serialize(new CreateLinkingProofResponse
        {
            Proof = "synthetic",
            ExpiresAtUtc = new DateTime(2026, 10, 2, 12, 2, 0, DateTimeKind.Utc)
        }));
        Assert.NotNull(json["proof"]);
        Assert.NotNull(json["expiresAtUtc"]);
        Assert.Null(json["linkingProof"]);
        Assert.Null(json["expiresIn"]);
        var redemption = JObject.Parse(VAA.EAPI.GraphQL.Common.Serialisation.Json.Serialize(new RedeemLinkingProofResponse
        {
            FlyingClubAccountNumber = "1234567890",
            ConsentSubject = "synthetic"
        }));
        Assert.NotNull(redemption["consentSubject"]);
        Assert.Null(redemption["consentReference"]);
    }
}
