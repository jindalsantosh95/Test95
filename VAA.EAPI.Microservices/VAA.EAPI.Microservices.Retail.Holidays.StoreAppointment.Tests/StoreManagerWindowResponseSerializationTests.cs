using System;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Constants;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.StoreManager;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Tests;

public sealed class StoreManagerWindowResponseSerializationTests
{
    [Fact]
    public void WindowResponse_ShouldSerializeAvailableStatus()
    {
        var response = new WindowResponse
        {
            Id = "slot-1",
            StoreId = "1043",
            StartAt = DateTimeOffset.Parse("2026-06-26T09:00:00+01:00"),
            EndAt = DateTimeOffset.Parse("2026-06-26T10:00:00+01:00"),
            Capacity = 1,
            Status = SlotStatus.Available
        };

        var json = Json.Serialize(response);

        Assert.Contains("\"status\":\"AVAILABLE\"", json);
    }
}
