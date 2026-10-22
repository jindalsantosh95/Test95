using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Helper;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.Entities;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.Store;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreAppointment;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Tests
{
    public class StoreRepositorySpecialHoursTests
    {
        [Fact]
        public async Task GetStoreByIdAsync_ShouldPreferDedicatedSpecialHoursColumn_WhenAvailable()
        {
            await using var dbContext = BuildDbContext(nameof(GetStoreByIdAsync_ShouldPreferDedicatedSpecialHoursColumn_WhenAvailable));

            dbContext.Stores.Add(new StoreEntity
            {
                StoreId = "001",
                Name = "Oxford Circus",
                Label = "Oxford Circus",
                StoreWorkHoursJson = "[{\"day\":\"Mon\",\"openingTime\":\"09:00\",\"closingTime\":\"19:00\"}]",
                StoreSpecialHoursJson = "{\"specialHours\":[{\"date\":\"2026-12-25\",\"label\":\"Christmas Day\",\"isClosed\":true}]}"
            });

            await dbContext.SaveChangesAsync();

            var repository = new StoreRepository(dbContext);
            var store = await repository.GetStoreByIdAsync("001");

            Assert.NotNull(store);
            var specialHour = Assert.Single(store!.FullDetails!.OpeningHours.SpecialHours);
            Assert.Equal(new DateOnly(2026, 12, 25), specialHour.Date);
            Assert.Equal("Christmas Day", specialHour.Label);
            Assert.True(specialHour.IsClosed);
        }

        [Fact]
        public async Task GetStoreByIdAsync_ShouldFallbackToEmbeddedSpecialHours_WhenDedicatedColumnEmpty()
        {
            await using var dbContext = BuildDbContext(nameof(GetStoreByIdAsync_ShouldFallbackToEmbeddedSpecialHours_WhenDedicatedColumnEmpty));

            dbContext.Stores.Add(new StoreEntity
            {
                StoreId = "002",
                Name = "Manchester",
                Label = "Manchester",
                StoreWorkHoursJson = "{\"weeklyHours\":[{\"day\":\"Mon\",\"openingTime\":\"09:00\",\"closingTime\":\"19:00\"}],\"specialHours\":[{\"date\":\"2026-12-26\",\"isClosed\":true}]}",
                StoreSpecialHoursJson = string.Empty
            });

            await dbContext.SaveChangesAsync();

            var repository = new StoreRepository(dbContext);
            var store = await repository.GetStoreByIdAsync("002");

            Assert.NotNull(store);
            var specialHour = Assert.Single(store!.FullDetails!.OpeningHours.SpecialHours);
            Assert.Equal(new DateOnly(2026, 12, 26), specialHour.Date);
            Assert.Equal("Closed", specialHour.Label);
            Assert.True(specialHour.IsClosed);
        }


        [Fact]
        public async Task GetStoreByIdAsync_ShouldReturnEmptySpecialHours_WhenDedicatedPayloadContainsEmptyArray()
        {
            await using var dbContext = BuildDbContext(nameof(GetStoreByIdAsync_ShouldReturnEmptySpecialHours_WhenDedicatedPayloadContainsEmptyArray));

            dbContext.Stores.Add(new StoreEntity
            {
                StoreId = "003",
                Name = "Leeds",
                Label = "Leeds",
                StoreWorkHoursJson = "{\"weeklyHours\":[{\"day\":\"Mon\",\"openingTime\":\"09:00\",\"closingTime\":\"19:00\"}],\"specialHours\":[{\"date\":\"2026-12-26\",\"isClosed\":true}]}",
                StoreSpecialHoursJson = "{\"specialHours\":[]}"
            });

            await dbContext.SaveChangesAsync();

            var repository = new StoreRepository(dbContext);
            var store = await repository.GetStoreByIdAsync("003");

            Assert.NotNull(store);
            Assert.Empty(store!.FullDetails!.OpeningHours.SpecialHours);
        }

        [Fact]
        public async Task GetStoreByIdAsync_ShouldRenderClosedForWeeklyHours_WhenDayIsMarkedClosed()
        {
            await using var dbContext = BuildDbContext(nameof(GetStoreByIdAsync_ShouldRenderClosedForWeeklyHours_WhenDayIsMarkedClosed));

            dbContext.Stores.Add(new StoreEntity
            {
                StoreId = "005",
                Name = "Cardiff",
                Label = "Cardiff",
                StoreWorkHoursJson = "[{\"day\":\"Sun\",\"isClosed\":1,\"openingTime\":\"\",\"closingTime\":\"\"},{\"day\":\"Mon\",\"openingTime\":\"09:00\",\"closingTime\":\"19:00\"}]",
                StoreSpecialHoursJson = "{\"specialHours\":[]}"
            });

            await dbContext.SaveChangesAsync();

            var repository = new StoreRepository(dbContext);
            var store = await repository.GetStoreByIdAsync("005");

            Assert.NotNull(store);
            var sunday = Assert.Single(store!.FullDetails!.OpeningHours.WeeklyHours, item => item.Day == "Sun");
            Assert.Equal("Closed", sunday.Hours);
        }
        [Fact]
        public async Task GetStoreByIdAsync_ShouldSetIsOpenNowFalse_WhenTodayIsMarkedClosed()
        {
            await using var dbContext = BuildDbContext(nameof(GetStoreByIdAsync_ShouldSetIsOpenNowFalse_WhenTodayIsMarkedClosed));

            var ukToday = GetUkTodayShortName();

            dbContext.Stores.Add(new StoreEntity
            {
                StoreId = "006",
                Name = "Bristol",
                Label = "Bristol",
                StoreWorkHoursJson = $$"""[{"day":"{{ukToday}}","isClosed":1,"openingTime":"00:00","closingTime":"23:59"}]""",
                StoreSpecialHoursJson = "{\"specialHours\":[]}"
            });

            await dbContext.SaveChangesAsync();

            var repository = new StoreRepository(dbContext);
            var store = await repository.GetStoreByIdAsync("006");

            Assert.NotNull(store);
            Assert.False(store!.FullDetails!.OpeningHours.IsOpenNow);
        }

        [Fact]
        public async Task GetStoreByIdAsync_ShouldSetIsOpenNowFalse_WhenStoreHasActiveClosureToday()
        {
            await using var dbContext = BuildDbContext(nameof(GetStoreByIdAsync_ShouldSetIsOpenNowFalse_WhenStoreHasActiveClosureToday));

            var todayUk = GetUkToday();
            dbContext.Stores.Add(BuildStore("closure-today", BuildOpenTodayHours()));
            dbContext.StoreManagerClosures.Add(BuildClosure("closure-today", todayUk, isActive: true));
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            var repository = new StoreRepository(dbContext);
            var store = await repository.GetStoreByIdAsync("closure-today");

            Assert.NotNull(store);
            Assert.False(store!.FullDetails!.OpeningHours.IsOpenNow);
        }

        [Fact]
        public async Task GetStoreByIdAsync_ShouldUseWeeklyHours_WhenTodayClosureIsInactive()
        {
            await using var dbContext = BuildDbContext(nameof(GetStoreByIdAsync_ShouldUseWeeklyHours_WhenTodayClosureIsInactive));

            var todayUk = GetUkToday();
            dbContext.Stores.Add(BuildStore("inactive-closure", BuildOpenTodayHours()));
            dbContext.StoreManagerClosures.Add(BuildClosure("inactive-closure", todayUk, isActive: false));
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            var repository = new StoreRepository(dbContext);
            var store = await repository.GetStoreByIdAsync("inactive-closure");

            Assert.NotNull(store);
            Assert.True(store!.FullDetails!.OpeningHours.IsOpenNow);
        }

        [Fact]
        public async Task GetStoreByIdAsync_ShouldUseWeeklyHours_WhenClosureIsNotToday()
        {
            await using var dbContext = BuildDbContext(nameof(GetStoreByIdAsync_ShouldUseWeeklyHours_WhenClosureIsNotToday));

            var tomorrowUk = GetUkToday().AddDays(1);
            dbContext.Stores.Add(BuildStore("future-closure", BuildOpenTodayHours()));
            dbContext.StoreManagerClosures.Add(BuildClosure("future-closure", tomorrowUk, isActive: true));
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            var repository = new StoreRepository(dbContext);
            var store = await repository.GetStoreByIdAsync("future-closure");

            Assert.NotNull(store);
            Assert.True(store!.FullDetails!.OpeningHours.IsOpenNow);
        }

        [Fact]
        public async Task GetStoresAsync_ShouldSetIsOpenNowFalseOnlyForStoresWithActiveClosureToday()
        {
            await using var dbContext = BuildDbContext(nameof(GetStoresAsync_ShouldSetIsOpenNowFalseOnlyForStoresWithActiveClosureToday));

            var todayUk = GetUkToday();
            dbContext.Stores.Add(BuildStore("closed-store", BuildOpenTodayHours()));
            dbContext.Stores.Add(BuildStore("open-store", BuildOpenTodayHours()));
            dbContext.StoreManagerClosures.Add(BuildClosure("closed-store", todayUk, isActive: true));
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            var repository = new StoreRepository(dbContext);
            var stores = await repository.GetStoresAsync();

            Assert.False(stores.Single(store => store.StoreId == "closed-store").FullDetails!.OpeningHours.IsOpenNow);
            Assert.True(stores.Single(store => store.StoreId == "open-store").FullDetails!.OpeningHours.IsOpenNow);
        }

        [Fact]
        public async Task GetStoreByIdAsync_ShouldReturnMarkedAsClosed()
        {
            await using var dbContext = BuildDbContext(nameof(GetStoreByIdAsync_ShouldReturnMarkedAsClosed));
            var store = BuildStore("007", """[{"day":"Mon","openingTime":"09:00","closingTime":"17:00","isClosed":false}]""");
            store.MarkedAsClosed = true;
            dbContext.Stores.Add(store);
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            var repository = new StoreRepository(dbContext);
            var result = await repository.GetStoreByIdAsync("007");

            Assert.NotNull(result);
            Assert.True(result!.MarkedAsClosed);
        }

        [Fact]
        public async Task GetStoresAsync_ShouldReturnMarkedAsClosed()
        {
            await using var dbContext = BuildDbContext(nameof(GetStoresAsync_ShouldReturnMarkedAsClosed));
            var store = BuildStore("008", """[{"day":"Mon","openingTime":"09:00","closingTime":"17:00","isClosed":false}]""");
            store.MarkedAsClosed = true;
            dbContext.Stores.Add(store);
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            var repository = new StoreRepository(dbContext);
            var result = await repository.GetStoresAsync();

            Assert.True(Assert.Single(result).MarkedAsClosed);
        }

        [Fact]
        public async Task GetStoreSlotsByDateAsync_ShouldReturnNoSlots_WhenRequestedDayIsClosed()
        {
            await using var dbContext = BuildDbContext(nameof(GetStoreSlotsByDateAsync_ShouldReturnNoSlots_WhenRequestedDayIsClosed));
            dbContext.Stores.Add(BuildStore("1001", """[{"day":"Tue","openingTime":"00:00","closingTime":"00:00","isClosed":true}]"""));
            dbContext.StoreSlots.Add(BuildSlot("1001", "Tue", new TimeOnly(10, 0), new TimeOnly(11, 0)));
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            var repository = new StoreRepository(dbContext);
            var slots = await repository.GetStoreSlotsByDateAsync("1001", new DateTime(2027, 4, 13));

            Assert.Empty(slots);
        }

        [Fact]
        public async Task GetStoreSlotsByDateAsync_ShouldReturnNoSlots_WhenStoreIsMarkedClosed()
        {
            await using var dbContext = BuildDbContext(nameof(GetStoreSlotsByDateAsync_ShouldReturnNoSlots_WhenStoreIsMarkedClosed));
            var store = BuildStore("1005", """[{"day":"Tue","openingTime":"07:00","closingTime":"18:00","isClosed":false}]""");
            store.MarkedAsClosed = true;
            dbContext.Stores.Add(store);
            dbContext.StoreSlots.Add(BuildSlot("1005", "Tue", new TimeOnly(10, 0), new TimeOnly(11, 0)));
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            var repository = new StoreRepository(dbContext);
            var slots = await repository.GetStoreSlotsByDateAsync("1005", new DateTime(2027, 4, 13));

            Assert.Empty(slots);
        }

        [Fact]
        public async Task GetStoreSlotsByDateAsync_ShouldReturnNoSlots_WhenRequestedDayIsMissingFromWorkHours()
        {
            await using var dbContext = BuildDbContext(nameof(GetStoreSlotsByDateAsync_ShouldReturnNoSlots_WhenRequestedDayIsMissingFromWorkHours));
            dbContext.Stores.Add(BuildStore("1006", """[{"day":"Mon","openingTime":"07:00","closingTime":"18:00","isClosed":false}]"""));
            dbContext.StoreSlots.Add(BuildSlot("1006", "Tue", new TimeOnly(10, 0), new TimeOnly(11, 0)));
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            var repository = new StoreRepository(dbContext);
            var slots = await repository.GetStoreSlotsByDateAsync("1006", new DateTime(2027, 4, 13));

            Assert.Empty(slots);
        }

        [Fact]
        public async Task GetStoreSlotsByDateAsync_ShouldReturnNoSlots_WhenOpeningHoursAreOvernight()
        {
            await using var dbContext = BuildDbContext(nameof(GetStoreSlotsByDateAsync_ShouldReturnNoSlots_WhenOpeningHoursAreOvernight));
            dbContext.Stores.Add(BuildStore("1007", """[{"day":"Tue","openingTime":"22:00","closingTime":"02:00","isClosed":false}]"""));
            dbContext.StoreSlots.Add(BuildSlot("1007", "Tue", new TimeOnly(22, 0), new TimeOnly(23, 0)));
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            var repository = new StoreRepository(dbContext);
            var slots = await repository.GetStoreSlotsByDateAsync("1007", new DateTime(2027, 4, 13));

            Assert.Empty(slots);
        }

        [Fact]
        public async Task GetStoreSlotsByDateAsync_ShouldReturnOnlySlotsInsideOpeningHours_WhenGeneratedSlotsAreStale()
        {
            await using var dbContext = BuildDbContext(nameof(GetStoreSlotsByDateAsync_ShouldReturnOnlySlotsInsideOpeningHours_WhenGeneratedSlotsAreStale));
            dbContext.Stores.Add(BuildStore("1002", """[{"day":"Tue","openingTime":"12:00","closingTime":"16:00","isClosed":false}]"""));
            dbContext.StoreSlots.Add(BuildSlot("1002", "Tue", new TimeOnly(10, 0), new TimeOnly(11, 0)));
            dbContext.StoreSlots.Add(BuildSlot("1002", "Tue", new TimeOnly(12, 0), new TimeOnly(13, 0)));
            dbContext.StoreSlots.Add(BuildSlot("1002", "Tue", new TimeOnly(15, 0), new TimeOnly(16, 0)));
            dbContext.StoreSlots.Add(BuildSlot("1002", "Tue", new TimeOnly(16, 0), new TimeOnly(17, 0)));
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            var repository = new StoreRepository(dbContext);
            var slots = await repository.GetStoreSlotsByDateAsync("1002", new DateTime(2027, 4, 13));

            Assert.Equal(2, slots.Count);
            Assert.All(slots, slot =>
            {
                Assert.True(slot.StartTime.TimeOfDay >= new TimeSpan(12, 0, 0));
                Assert.True(slot.EndTime.TimeOfDay <= new TimeSpan(16, 0, 0));
            });
        }

        [Fact]
        public async Task GetStoreSlotsByDateAsync_ShouldReturnNoSlots_WhenSelectedDateHasActiveClosure()
        {
            await using var dbContext = BuildDbContext(nameof(GetStoreSlotsByDateAsync_ShouldReturnNoSlots_WhenSelectedDateHasActiveClosure));
            var closureDate = new DateOnly(2027, 4, 13);
            dbContext.Stores.Add(BuildStore("1011", """[{"day":"Tue","openingTime":"09:00","closingTime":"17:00","isClosed":false}]"""));
            dbContext.StoreSlots.Add(BuildSlot("1011", "Tue", new TimeOnly(10, 0), new TimeOnly(11, 0)));
            dbContext.StoreManagerClosures.Add(BuildClosure("1011", closureDate, isActive: true));
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            var repository = new StoreRepository(dbContext);
            var slots = await repository.GetStoreSlotsByDateAsync("1011", closureDate.ToDateTime(TimeOnly.MinValue));

            Assert.Empty(slots);
        }

        [Fact]
        public async Task GetStoreCalendarAvailabilityAsync_ShouldReturnUnavailable_WhenRequestedDayIsClosed()
        {
            await using var dbContext = BuildDbContext(nameof(GetStoreCalendarAvailabilityAsync_ShouldReturnUnavailable_WhenRequestedDayIsClosed));
            dbContext.Stores.Add(BuildStore("1003", """[{"day":"Tue","openingTime":"00:00","closingTime":"00:00","isClosed":true}]"""));
            dbContext.StoreSlots.Add(BuildSlot("1003", "Tue", new TimeOnly(10, 0), new TimeOnly(11, 0)));
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            var repository = new StoreRepository(dbContext);
            var days = await repository.GetStoreCalendarAvailabilityAsync("1003", 2027, 4);

            Assert.False(days.Single(day => day.Date.Date == new DateTime(2027, 4, 13)).HasAvailableSlot);
        }

        [Fact]
        public async Task GetStoreCalendarAvailabilityAsync_ShouldReturnUnavailable_WhenRequestedDayIsMissingFromWorkHours()
        {
            await using var dbContext = BuildDbContext(nameof(GetStoreCalendarAvailabilityAsync_ShouldReturnUnavailable_WhenRequestedDayIsMissingFromWorkHours));
            dbContext.Stores.Add(BuildStore("1008", """[{"day":"Mon","openingTime":"09:00","closingTime":"17:00","isClosed":false}]"""));
            dbContext.StoreSlots.Add(BuildSlot("1008", "Tue", new TimeOnly(10, 0), new TimeOnly(11, 0)));
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            var repository = new StoreRepository(dbContext);
            var days = await repository.GetStoreCalendarAvailabilityAsync("1008", 2027, 4);

            Assert.False(days.Single(day => day.Date.Date == new DateTime(2027, 4, 13)).HasAvailableSlot);
        }

        [Fact]
        public async Task GetStoreCalendarAvailabilityAsync_ShouldReturnAvailable_WhenSlotIsInsideOpeningHours()
        {
            await using var dbContext = BuildDbContext(nameof(GetStoreCalendarAvailabilityAsync_ShouldReturnAvailable_WhenSlotIsInsideOpeningHours));
            dbContext.Stores.Add(BuildStore("1004", """[{"day":"Tue","openingTime":"12:00","closingTime":"16:00","isClosed":false}]"""));
            dbContext.StoreSlots.Add(BuildSlot("1004", "Tue", new TimeOnly(10, 0), new TimeOnly(11, 0)));
            dbContext.StoreSlots.Add(BuildSlot("1004", "Tue", new TimeOnly(12, 0), new TimeOnly(13, 0)));
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            var repository = new StoreRepository(dbContext);
            var days = await repository.GetStoreCalendarAvailabilityAsync("1004", 2027, 4);

            Assert.True(days.Single(day => day.Date.Date == new DateTime(2027, 4, 13)).HasAvailableSlot);
        }

        [Fact]
        public async Task GetStoreCalendarAvailabilityAsync_ShouldReturnUnavailable_WhenDateHasActiveClosure()
        {
            await using var dbContext = BuildDbContext(nameof(GetStoreCalendarAvailabilityAsync_ShouldReturnUnavailable_WhenDateHasActiveClosure));
            var closureDate = new DateOnly(2027, 4, 13);
            dbContext.Stores.Add(BuildStore("1012", """[{"day":"Tue","openingTime":"09:00","closingTime":"17:00","isClosed":false}]"""));
            dbContext.StoreSlots.Add(BuildSlot("1012", "Tue", new TimeOnly(10, 0), new TimeOnly(11, 0)));
            dbContext.StoreManagerClosures.Add(BuildClosure("1012", closureDate, isActive: true));
            await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            var repository = new StoreRepository(dbContext);
            var days = await repository.GetStoreCalendarAvailabilityAsync("1012", 2027, 4);

            Assert.False(days.Single(day => day.Date.Date == closureDate.ToDateTime(TimeOnly.MinValue)).HasAvailableSlot);
        }

        private static string GetUkTodayShortName()
        {
            return GetUkToday().DayOfWeek.ToString()[..3];
        }

        private static DateOnly GetUkToday()
        {
            return DateOnly.FromDateTime(BritishTimeZoneHelper.ConvertToUkLocal(DateTimeOffset.UtcNow).DateTime);
        }

        private static string BuildOpenTodayHours()
        {
            return $$"""[{"day":"{{GetUkTodayShortName()}}","openingTime":"00:00","closingTime":"23:59:59","isClosed":false}]""";
        }

        private static StoreAppointmentDbContext BuildDbContext(string databaseName)
        {
            var options = new DbContextOptionsBuilder<StoreAppointmentDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            return new StoreAppointmentDbContext(options);
        }

        private static StoreEntity BuildStore(string storeId, string storeWorkHoursJson) => new()
        {
            StoreId = storeId,
            Name = $"Store {storeId}",
            Label = $"Store {storeId}",
            StoreWorkHoursJson = storeWorkHoursJson,
            StoreSpecialHoursJson = "{\"specialHours\":[]}"
        };

        private static StoreManagerClosureEntity BuildClosure(string storeId, DateOnly closureDate, bool isActive) => new()
        {
            ClosureId = $"closure-{storeId}-{closureDate:yyyyMMdd}-{isActive}",
            StoreId = storeId,
            ClosureDate = closureDate,
            ClosureType = "TEST",
            IsActive = isActive
        };

        private static VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.Entities.StoreSlotEntity BuildSlot(
            string storeId,
            string dayCode,
            TimeOnly startTime,
            TimeOnly endTime) => new()
            {
                StoreId = storeId,
                DayCode = dayCode,
                SlotStartTime = startTime,
                SlotEndTime = endTime,
                Capacity = 1,
                IsActive = true
            };
    }
}
