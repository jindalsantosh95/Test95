using System.Reflection;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Account.Mutations.Secure;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests;

public sealed class AccountLinkingAuthorizationContractTests
{
    [Fact]
    public void RedeemProof_ShouldRequireConfiguredApplicationRole()
    {
        var authorize = Assert.Single(typeof(AccountLinkingMutation)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .SelectMany(method => method.GetCustomAttributesData()),
            attribute => attribute.AttributeType.FullName == "HotChocolate.AspNetCore.Authorization.AuthorizeAttribute");
        var roles = authorize.NamedArguments
            .Where(argument => argument.MemberName == "Roles")
            .SelectMany(argument => argument.TypedValue.Value as IReadOnlyCollection<CustomAttributeTypedArgument> ?? [])
            .Select(argument => argument.Value?.ToString())
            .Where(role => !role.IsNullOrWhiteSpace());

        Assert.Equal([VAA.EAPI.GraphQL.Domain.Account.Constants.Application.AccountLinkingProofRedeemRole], roles);
    }
}
