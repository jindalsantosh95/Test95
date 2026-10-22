using Azure.Data.Tables;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using Xunit;

namespace VAA.EAPI.Automation.Core.Tests;

public sealed class StorageAccountHelperTests
{
    [Fact]
    public void CreateNdcPnrEntity_PreservesBookingMetadata()
    {
        NdcPnrTableEntity entity = StorageAccountHelper.CreateNdcPnrEntity(
            "ABC123",
            "UAT",
            "Hold",
            "Visa",
            "12A",
            "2026-09-18T15:00:00.0000000+00:00");

        Assert.Equal("uat", entity.PartitionKey);
        Assert.Equal("ABC123", entity.RowKey);
        Assert.Equal("UAT", entity.Environment);
        Assert.Equal("Hold", entity.BookingType);
        Assert.Equal("Visa", entity.PaymentType);
        Assert.Equal("12A", entity.SeatType);
        Assert.Equal("2026-09-18T15:00:00.0000000+00:00", entity.CreatedUtc);
    }

    [Fact]
    public void CreateCleanupBatches_GroupsRowsByPartition()
    {
        List<NdcPnrTableEntity> entities = new()
        {
            new NdcPnrTableEntity { PartitionKey = "uat", RowKey = "UAT001" },
            new NdcPnrTableEntity { PartitionKey = "prod", RowKey = "PROD001" },
            new NdcPnrTableEntity { PartitionKey = "uat", RowKey = "UAT002" }
        };

        List<List<TableTransactionAction>> batches = StorageAccountHelper.CreateCleanupBatches(entities)
            .Select(batch => batch.ToList())
            .ToList();

        Assert.Equal(2, batches.Count);
        Assert.All(batches, batch => Assert.Single(batch.Select(action => action.Entity.PartitionKey).Distinct()));
        Assert.Contains(batches, batch => batch.Any(action => action.Entity.RowKey == "UAT001"));
        Assert.Contains(batches, batch => batch.Any(action => action.Entity.RowKey == "PROD001"));
    }
}
