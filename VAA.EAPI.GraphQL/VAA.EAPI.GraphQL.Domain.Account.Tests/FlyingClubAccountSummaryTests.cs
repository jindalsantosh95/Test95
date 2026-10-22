using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Types;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Domain.Account.Queries;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests;

[Collection("EnvironmentDependentTests")]
public sealed class FlyingClubAccountSummaryTests
{
    [Fact]
    public async Task MissingTokenReturnsFaultedTaskWithoutCallingProvider()
    {
        var previous = AsyncContext.Current;
        try
        {
            AsyncContext.Current = new AsyncContext { Context = new TestFunctionContext([]) };
            using var services = FlyingClubSummaryTestServices.Create();
            var query = services.GetRequiredService<FlyingClubAccountSummaryQuery>();
            var task = query.GetAsync(CancellationToken.None);

            var error = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => task);
            Assert.Equal("Unauthorized", error.Message);
            var httpFactory = (VAA.EAPI.GraphQL.Core.Xunit.Http.HttpClientFactory)services.GetRequiredService<IHttpClientFactory>();
            Assert.Empty(httpFactory.GetResponses(FlyingClubSummaryTestServices.Root));
        }
        finally
        {
            AsyncContext.Current = previous;
        }
    }

    [Theory]
    [InlineData("unauthorized")]
    [InlineData("missing-account")]
    public async Task InvalidTokenUsesSharedUnauthorizedContract(string scenario)
    {
        using var services = FlyingClubSummaryTestServices.Create(FlyingClubSummaryTestServices.Root + "/" + scenario);
        var provider = services.GetRequiredService<IFlyingClubAccountSummaryProvider>();
        var error = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => provider.GetAsync("synthetic-invalid-token"));
        Assert.Equal("Unauthorized", error.Message);
        var httpFactory = (VAA.EAPI.GraphQL.Core.Xunit.Http.HttpClientFactory)services.GetRequiredService<IHttpClientFactory>();
        Assert.Single(httpFactory.GetResponses(FlyingClubSummaryTestServices.Root));
    }

    [Fact]
    public async Task DependencyFailureUsesTechnicalErrorContract()
    {
        using var services = FlyingClubSummaryTestServices.Create(FlyingClubSummaryTestServices.Root + "/forbidden");
        var provider = services.GetRequiredService<IFlyingClubAccountSummaryProvider>();
        var error = await Assert.ThrowsAsync<Exception>(() => provider.GetAsync("synthetic-token"));
        Assert.Equal("ACCOUNT_SUMMARY_DEPENDENCY_UNAVAILABLE", error.Message);
    }

    [Fact]
    public void ResponseContractContainsOnlyLeastDataFields()
    {
        var schema = SchemaBuilder.New()
            .AddQueryType(descriptor => descriptor.Name("Query").Field("summary")
                .Type<ObjectType<FlyingClubAccountSummary>>().Resolve(new FlyingClubAccountSummary()))
            .Create();
        var properties = schema.GetType<ObjectType>("FlyingClubAccountSummary").Fields
            .Where(field => !field.Name.ToString().StartsWith("__", StringComparison.Ordinal))
            .Select(field => field.Name.ToString())
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal([
            "tier",
            "tierPoints",
            "tierPointsResetDate",
            "virginPoints"
        ], properties);
    }

    [Fact]
    public async Task QueryRequiresDedicatedSummaryRole()
    {
        using var services = new ServiceCollection().AddGraphQLServer()
            .AddQueryType(descriptor => descriptor.Name("Query"))
            .AddTypeExtension<FlyingClubAccountSummaryQuery>()
            .AddAuthorizeDirectiveType()
            .Services.BuildServiceProvider();
        var executor = await services.GetRequiredService<IRequestExecutorResolver>().GetRequestExecutorAsync();
        var field = executor.Schema.QueryType.Fields["loyaltyFlyingClubAccountSummary"];
        var authorization = Assert.Single(field.Directives, directive => directive.Type.Name == "authorize")
            .ToObject<HotChocolate.AspNetCore.Authorization.AuthorizeDirective>();
        Assert.Equal(["Loyalty.AccountSummary.Read"], authorization.Roles);
    }
}
