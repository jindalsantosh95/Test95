using Newtonsoft.Json;
using NodaTime;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Serialization;
using DomainAvailabilityWindowResponse = VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager.AvailabilityWindowResponse;
using DomainBookingResponse = VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager.BookingResponse;
using DomainManageBookingResponse = VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager.ManageBookingResponse;
using DomainManageSlotAllocationBookingResponse = VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager.ManageSlotAllocationBookingResponse;
using DomainManageSlotAllocationResponse = VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager.ManageSlotAllocationResponse;
using DomainOpeningHoursEntryResponse = VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager.OpeningHoursEntryResponse;
using DomainOpeningHoursEntryRequest = VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager.OpeningHoursEntryRequest;
using DomainSlotRuleResponse = VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager.SlotRuleResponse;
using DomainSlotRuleRequest = VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager.SlotRuleRequest;
using DomainSlotStatusUpdateRequest = VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager.SlotStatusUpdateRequest;
using DomainStoreClosureRequest = VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager.StoreClosureRequest;
using DomainStoreClosureResponse = VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager.StoreClosureResponse;
using DomainStoreManagerWindowResponse = VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager.StoreManagerWindowResponse;
using InfrastructureAvailabilityWindowResponse = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager.AvailabilityWindowResponse;
using InfrastructureBookingResponse = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager.BookingResponse;
using InfrastructureManageBookingResponse = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager.ManageBookingResponse;
using InfrastructureManageSlotAllocationBookingResponse = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager.ManageSlotAllocationBookingResponse;
using InfrastructureManageSlotAllocationResponse = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager.ManageSlotAllocationResponse;
using InfrastructureOpeningHoursEntryResponse = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager.OpeningHoursEntryResponse;
using InfrastructureSlotRuleResponse = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager.SlotRuleResponse;
using InfrastructureStoreClosureRequest = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager.StoreClosureRequest;
using InfrastructureStoreClosureResponse = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager.StoreClosureResponse;
using InfrastructureStoreManagerWindowResponse = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager.StoreManagerWindowResponse;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Tests;

public sealed class StoreManagerTemporalContractTests
{
    [Fact]
    public void ClosureRequest_ShouldSerializeDateOnlyAsIsoDate()
    {
        var request = new InfrastructureStoreClosureRequest
        {
            ClosureDate = new DateOnly(2026, 12, 25),
            ClosureType = "FULL_DAY",
            Reason = "Christmas"
        };

        var json = Json.Serialize(request);

        Assert.Contains("\"closureDate\":\"2026-12-25\"", json);
    }

    [Fact]
    public void ClosureResponse_ShouldDeserializeAndMapIsoDate()
    {
        const string json = """
                            {
                              "id": "closure-1",
                              "closureDate": "2026-07-15",
                              "closureType": "FULL_DAY"
                            }
                            """;

        var infrastructureResponse = Json.Deserialize<InfrastructureStoreClosureResponse>(json);
        var response = (DomainStoreClosureResponse)infrastructureResponse;

        Assert.Equal(new DateOnly(2026, 7, 15), infrastructureResponse.ClosureDate);
        Assert.Equal(new LocalDate(2026, 7, 15), response.ClosureDate);
    }

    [Fact]
    public void DateOnlyConverter_ShouldRejectNonIsoDate()
    {
        var converter = new DateOnlyJsonConverter();
        var serializer = new JsonSerializer();
        using var stringReader = new StringReader("\"15/07/2026\"");
        using var reader = new JsonTextReader(stringReader);
        reader.Read();

        Assert.Throws<JsonSerializationException>(
            () => converter.ReadJson(reader, typeof(DateOnly), default, false, serializer));
    }

    [Fact]
    public void OpeningHoursRequest_ShouldPreserveClosedValue()
    {
        var request = new DomainOpeningHoursEntryRequest
        {
            DayOfWeek = 0,
            OpensAt = "closed",
            ClosesAt = "closed"
        };

        var json = Json.Serialize(request);

        Assert.Contains("\"opensAt\":\"closed\"", json);
        Assert.Contains("\"closesAt\":\"closed\"", json);
    }

    [Fact]
    public void ManageBookingResponse_ShouldDeserializeAndMapDateTimeOffsets()
    {
        const string json = """
                            {
                              "id": "booking-1",
                              "storeId": "store-1",
                              "storeSlotId": "slot-1",
                              "customerRef": "ABC123",
                              "status": "BOOKED",
                              "windowStartAt": "2026-07-01T09:00:00+01:00",
                              "windowEndAt": "2026-07-01T09:30:00+01:00",
                              "windowCapacity": 2,
                              "createdAt": "2026-06-01T10:00:00Z",
                              "updatedAt": "2026-06-01T11:00:00Z"
                            }
                            """;

        var infrastructureResponse = Json.Deserialize<InfrastructureManageBookingResponse>(json);
        var response = (DomainManageBookingResponse)infrastructureResponse;

        Assert.Equal("slot-1", response.StoreSlotId);
        Assert.Equal(TimeSpan.FromHours(1), response.WindowStartAt.Offset);
        Assert.Equal(DateTimeOffset.Parse("2026-07-01T09:30:00+01:00"), response.WindowEndAt);
        Assert.Equal(DateTimeOffset.Parse("2026-06-01T10:00:00Z"), response.CreatedAt);
        Assert.Equal(DateTimeOffset.Parse("2026-06-01T11:00:00Z"), response.UpdatedAt);
    }

    [Theory]
    [MemberData(nameof(StronglyTypedTemporalProperties))]
    public void TemporalProperty_ShouldUseExpectedType(Type modelType, string propertyName, Type expectedType)
    {
        Assert.Equal(expectedType, modelType.GetProperty(propertyName)?.PropertyType);
    }

    [Theory]
    [MemberData(nameof(StringContractProperties))]
    public void OpaqueOrFlexibleProperty_ShouldRemainString(Type modelType, string propertyName)
    {
        Assert.Equal(typeof(string), modelType.GetProperty(propertyName)?.PropertyType);
    }

    public static IEnumerable<object[]> StronglyTypedTemporalProperties()
    {
        var dateTimeOffsetProperties = new (Type Infrastructure, Type Domain, string[] Properties)[]
        {
            (typeof(InfrastructureAvailabilityWindowResponse), typeof(DomainAvailabilityWindowResponse), ["StartAt", "EndAt"]),
            (typeof(InfrastructureBookingResponse), typeof(DomainBookingResponse), ["CreatedAt", "UpdatedAt"]),
            (typeof(InfrastructureManageBookingResponse), typeof(DomainManageBookingResponse), ["WindowStartAt", "WindowEndAt", "CreatedAt", "UpdatedAt"]),
            (typeof(InfrastructureManageSlotAllocationBookingResponse), typeof(DomainManageSlotAllocationBookingResponse), ["CreatedAt", "UpdatedAt"]),
            (typeof(InfrastructureManageSlotAllocationResponse), typeof(DomainManageSlotAllocationResponse), ["StartAt", "EndAt"]),
            (typeof(InfrastructureStoreManagerWindowResponse), typeof(DomainStoreManagerWindowResponse), ["StartAt", "EndAt"])
        };

        foreach (var (infrastructure, domain, properties) in dateTimeOffsetProperties)
        {
            foreach (var property in properties)
            {
                yield return [infrastructure, property, typeof(DateTimeOffset)];
                yield return [domain, property, typeof(DateTimeOffset)];
            }
        }

        yield return [typeof(InfrastructureStoreClosureRequest), "ClosureDate", typeof(DateOnly)];
        yield return [typeof(InfrastructureStoreClosureResponse), "ClosureDate", typeof(DateOnly)];
        yield return [typeof(DomainStoreClosureRequest), "ClosureDate", typeof(LocalDate)];
        yield return [typeof(DomainStoreClosureResponse), "ClosureDate", typeof(LocalDate)];
        yield return [typeof(DomainSlotStatusUpdateRequest), "StartAt", typeof(DateTimeOffset)];
        yield return [typeof(DomainSlotStatusUpdateRequest), "EndAt", typeof(DateTimeOffset)];
    }

    public static IEnumerable<object[]> StringContractProperties()
    {
        yield return [typeof(InfrastructureAvailabilityWindowResponse), "Id"];
        yield return [typeof(InfrastructureAvailabilityWindowResponse), "StoreId"];
        yield return [typeof(InfrastructureBookingResponse), "Id"];
        yield return [typeof(InfrastructureBookingResponse), "WindowId"];
        yield return [typeof(InfrastructureManageBookingResponse), "StoreSlotId"];
        yield return [typeof(DomainManageBookingResponse), "StoreSlotId"];
        yield return [typeof(InfrastructureOpeningHoursEntryResponse), "OpensAt"];
        yield return [typeof(InfrastructureOpeningHoursEntryResponse), "ClosesAt"];
        yield return [typeof(DomainOpeningHoursEntryResponse), "OpensAt"];
        yield return [typeof(DomainOpeningHoursEntryResponse), "ClosesAt"];
        yield return [typeof(DomainOpeningHoursEntryRequest), "OpensAt"];
        yield return [typeof(DomainOpeningHoursEntryRequest), "ClosesAt"];
        yield return [typeof(InfrastructureSlotRuleResponse), "StartTime"];
        yield return [typeof(InfrastructureSlotRuleResponse), "EndTime"];
        yield return [typeof(DomainSlotRuleResponse), "StartTime"];
        yield return [typeof(DomainSlotRuleResponse), "EndTime"];
        yield return [typeof(DomainSlotRuleRequest), "StartTime"];
        yield return [typeof(DomainSlotRuleRequest), "EndTime"];
    }
}
