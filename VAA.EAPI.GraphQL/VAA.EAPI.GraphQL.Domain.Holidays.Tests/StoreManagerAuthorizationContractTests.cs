using System.Reflection;
using VAA.EAPI.GraphQL.Domain.Holidays.Constants;
using VAA.EAPI.GraphQL.Domain.Holidays.Mutations;
using VAA.EAPI.GraphQL.Domain.Holidays.Queries;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Tests;

public sealed class StoreManagerAuthorizationContractTests
{
    private const string AuthorizeAttributeFullName = "HotChocolate.AspNetCore.Authorization.AuthorizeAttribute";

    [Theory]
    [MemberData(nameof(StoreManagerResolvers))]
    public void StoreManagerResolver_ShouldDeclareAuthorization(Type resolverType)
    {
        Assert.True(
            HasAuthorizeAttribute(resolverType)
            || resolverType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Any(HasAuthorizeAttribute),
            $"{resolverType.Name} must declare an authorization attribute.");
    }

    [Theory]
    [InlineData(typeof(GetAdminStoresStoreManagerQuery))]
    [InlineData(typeof(GetAdminStoreByIdStoreManagerQuery))]
    [InlineData(typeof(UpdateAdminStoreMutation))]
    public void AdminNamedResolver_ShouldDeclareStoreAdminAndStoreManagerRolesTogether(Type resolverType)
    {
        var configuredRoleSets = GetAuthorizeAttributes(resolverType)
            .Select(attribute => GetConfiguredRoles(attribute).ToHashSet(StringComparer.Ordinal))
            .Where(roles => roles.Count > 0)
            .ToArray();

        var configuredRoles = Assert.Single(configuredRoleSets);
        Assert.Contains(Roles.StoreAdmin, configuredRoles);
        Assert.Contains(Roles.StoreManager, configuredRoles);
    }

    public static TheoryData<Type> StoreManagerResolvers =>
    [
        typeof(GetAdminStoreByIdStoreManagerQuery),
        typeof(GetAdminStoresStoreManagerQuery),
        typeof(GetAvailabilityStoreManagerQuery),
        typeof(GetAvailabilityRangeStoreManagerQuery),
        typeof(GetClosuresStoreManagerQuery),
        typeof(GetBookingsStoreManagerQuery),
        typeof(GetMeStoreManagerQuery),
        typeof(GetMyBookingsStoreManagerQuery),
        typeof(GetOpeningHoursStoreManagerQuery),
        typeof(GetSlotRulesStoreManagerQuery),
        typeof(GetSlotAllocationsStoreManagerQuery),
        typeof(GetSlotsStoreManagerQuery),
        typeof(GetStoreByIdStoreManagerQuery),
        typeof(CreateClosureMutation),
        typeof(CancelBookingMutation),
        typeof(DeleteClosureMutation),
        typeof(ReplaceOpeningHoursMutation),
        typeof(ReplaceSlotRulesMutation),
        typeof(UpdateAdminStoreMutation),
        typeof(UpdateSlotAllocationStatusMutation),
        typeof(UpdateSlotStatusMutation),
        typeof(UpdateStoreMutation)
    ];

    private static bool HasAuthorizeAttribute(MemberInfo member) =>
        member.GetCustomAttributesData().Any(attribute => attribute.AttributeType.FullName == AuthorizeAttributeFullName);

    private static IEnumerable<CustomAttributeData> GetAuthorizeAttributes(Type resolverType) =>
        resolverType.GetCustomAttributesData()
            .Concat(resolverType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .SelectMany(method => method.GetCustomAttributesData()))
            .Where(attribute => attribute.AttributeType.FullName == AuthorizeAttributeFullName);

    private static IEnumerable<string> GetConfiguredRoles(CustomAttributeData attribute)
    {
        return attribute.NamedArguments
            .Where(argument => argument.MemberName == "Roles")
            .SelectMany(argument => argument.TypedValue.Value as IReadOnlyCollection<CustomAttributeTypedArgument> ?? [])
            .Select(argument => argument.Value?.ToString())
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Select(role => role!);
    }
}
