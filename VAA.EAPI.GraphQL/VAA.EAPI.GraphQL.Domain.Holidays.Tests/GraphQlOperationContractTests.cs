using System.Reflection;
using HotChocolate;
using HotChocolate.Language;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.Holidays.Constants;
using VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;
using VAA.EAPI.GraphQL.Domain.Holidays.Mutations;
using VAA.EAPI.GraphQL.Domain.Holidays.Queries;
using Xunit;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Tests;

public class GraphQlOperationContractTests
{
    private const string GraphQlNameAttributeFullName = "HotChocolate.GraphQLNameAttribute";

    [Fact]
    public void Queries_ShouldMatchConfiguredOperationNameConstants()
    {
        var expected = GetConstantValues(typeof(GraphQlOperationNames.Queries));
        var actual = GetGraphQlFieldNames(
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
            typeof(NearbyStoresQuery),
            typeof(StoreDetailsQuery),
            typeof(SlotAvailabilityQuery),
            typeof(StoreCalendarAvailabilityQuery));

        Assert.Equal(expected.OrderBy(x => x), actual.OrderBy(x => x));
    }

    [Fact]
    public void Mutations_ShouldMatchConfiguredOperationNameConstants()
    {
        var expected = GetConstantValues(typeof(GraphQlOperationNames.Mutations));
        var actual = GetGraphQlFieldNames(
            typeof(CancelBookingMutation),
            typeof(CreateClosureMutation),
            typeof(DeleteClosureMutation),
            typeof(ReplaceOpeningHoursMutation),
            typeof(ReplaceSlotRulesMutation),
            typeof(SaveAppointmentBookingMutation),
            typeof(UpdateAdminStoreMutation),
            typeof(UpdateSlotAllocationStatusMutation),
            typeof(UpdateSlotStatusMutation),
            typeof(UpdateStoreMutation));

        Assert.Equal(expected.OrderBy(x => x), actual.OrderBy(x => x));
    }

    [Fact]
    public void OperationNameConstants_ShouldBeUniquePerRootType()
    {
        var queryNames = GetConstantValues(typeof(GraphQlOperationNames.Queries)).ToArray();
        var mutationNames = GetConstantValues(typeof(GraphQlOperationNames.Mutations)).ToArray();

        Assert.Equal(queryNames.Length, queryNames.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(mutationNames.Length, mutationNames.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void BookingCancellationReason_ShouldExposeStoreClosureEnumValue()
    {
        var enumMember = typeof(BookingCancellationReason)
            .GetMember(nameof(BookingCancellationReason.StoreClosure))
            .Single();
        var graphQlName = enumMember
            .GetCustomAttributesData()
            .Single(attribute => attribute.AttributeType.FullName == GraphQlNameAttributeFullName)
            .ConstructorArguments
            .Single()
            .Value as string;

        Assert.Equal("STORE_CLOSURE", graphQlName);
    }

    [Fact]
    public void BookingCancellationReason_ShouldParseFrontendEnumLiteral()
    {
        var schema = CreateSchemaWithEnumType<BookingCancellationReasonType>();
        var enumType = schema.GetType<EnumType>("BookingCancellationReason");

        var parsed = enumType.ParseLiteral(new EnumValueNode("STORE_CLOSURE"));

        Assert.Equal(BookingCancellationReason.StoreClosure, parsed);
    }

    [Theory]
    [InlineData("BOOKED", StoreManagerBookingStatus.Booked)]
    [InlineData("CANCELLED", StoreManagerBookingStatus.Cancelled)]
    public void StoreManagerBookingStatus_ShouldParseFrontendEnumLiteral(
        string literal,
        StoreManagerBookingStatus expected)
    {
        var schema = CreateSchemaWithEnumType<StoreManagerBookingStatusType>();
        var enumType = schema.GetType<EnumType>("StoreManagerBookingStatus");

        var parsed = enumType.ParseLiteral(new EnumValueNode(literal));

        Assert.Equal(expected, parsed);
    }

    private static ISchema CreateSchemaWithEnumType<TEnumType>()
        where TEnumType : INamedType
    {
        var queryType = new ObjectType(descriptor => descriptor
            .Name("Query")
            .Field("ping")
            .Resolve("pong"));

        return SchemaBuilder.New()
            .AddRootType(queryType, OperationType.Query)
            .AddType(typeof(TEnumType))
            .Create();
    }

    private static IEnumerable<string> GetConstantValues(Type containerType)
    {
        return containerType
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field is { IsLiteral: true, IsInitOnly: false } && field.FieldType == typeof(string))
            .Select(field => (string)field.GetRawConstantValue()!)
            .Where(value => !string.IsNullOrWhiteSpace(value));
    }

    private static IEnumerable<string> GetGraphQlFieldNames(params Type[] resolverTypes)
    {
        return resolverTypes
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Select(method => method
                .GetCustomAttributesData()
                .FirstOrDefault(attr => attr.AttributeType.FullName == GraphQlNameAttributeFullName)?
                .ConstructorArguments
                .FirstOrDefault()
                .Value as string)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!);
    }
}
