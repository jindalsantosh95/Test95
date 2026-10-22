using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.FlightOperations.Flights.Application.Interface;
using VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models;
using VAA.EAPI.Microservices.FlightOperations.Flights.Application.Service;
using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Interface;
using Xunit;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Tests
{
    public sealed class CheckAircraftCategoryServiceTests
    {
        private readonly CheckAircraftCategoryDbServiceStub _checkAircraftCategoryDbService = new();
        private readonly IServiceProvider _serviceProvider;

        public CheckAircraftCategoryServiceTests()
        {
            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<ICheckAircraftCategoryDbService>(_checkAircraftCategoryDbService)
                .AddSingleton<ICheckAircraftCategoryService, CheckAircraftCategoryService>()
                .BuildServiceProvider();
        }

        [Fact]
        public async Task CheckAircraftCategoryAsync_ReturnsCategoryStatusForEachTail()
        {
            _checkAircraftCategoryDbService.TailsWithCategory = ["G-VTEA"];

            var checkAircraftCategoryService = _serviceProvider.GetRequiredService<ICheckAircraftCategoryService>();

            var result = await checkAircraftCategoryService.CheckAircraftCategoryAsync(new CheckAircraftCategoryRequest
            {
                CategoryIndex = 11,
                TailNumbers = [" G-VTEA ", "g-vtea", string.Empty, "G-VWHO"]
            }, CancellationToken.None);

            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Equal(11, _checkAircraftCategoryDbService.CapturedCategoryIndex);
            Assert.Equal(["G-VTEA", "G-VWHO"], _checkAircraftCategoryDbService.CapturedTailNumbers);

            var categoryChecks = result.Value.Value.AircraftCategoryChecks.ToDictionary(item => item.TailNumber);
            Assert.True(categoryChecks["G-VTEA"].DoesCategoryExist);
            Assert.False(categoryChecks["G-VWHO"].DoesCategoryExist);

            var serializedResponse = Json.Serialize(result.Value.Value);
            Assert.Contains("\"doesCategoryExist\":true", serializedResponse);
            Assert.Contains("\"doesCategoryExist\":false", serializedResponse);
        }

        [Fact]
        public async Task CheckAircraftCategoryAsync_ReturnsErrorForEmptyTailNumbers()
        {
            var checkAircraftCategoryService = _serviceProvider.GetRequiredService<ICheckAircraftCategoryService>();

            var result = await checkAircraftCategoryService.CheckAircraftCategoryAsync(new CheckAircraftCategoryRequest
            {
                CategoryIndex = 11
            }, CancellationToken.None);

            Assert.Equal(Status.Error, result.Value.Status);
            Assert.Contains("At least one tail number is required.", result.Value.ErrorMessage);
            Assert.Equal(0, _checkAircraftCategoryDbService.CallCount);
        }

        [Fact]
        public async Task CheckAircraftCategoryAsync_ReturnsErrorForInvalidCategoryIndex()
        {
            var checkAircraftCategoryService = _serviceProvider.GetRequiredService<ICheckAircraftCategoryService>();

            var result = await checkAircraftCategoryService.CheckAircraftCategoryAsync(new CheckAircraftCategoryRequest
            {
                TailNumbers = ["G-VTEA"]
            }, CancellationToken.None);

            Assert.Equal(Status.Error, result.Value.Status);
            Assert.Contains("Category index must be greater than zero.", result.Value.ErrorMessage);
            Assert.Equal(0, _checkAircraftCategoryDbService.CallCount);
        }

        private sealed class CheckAircraftCategoryDbServiceStub : ICheckAircraftCategoryDbService
        {
            public IReadOnlyCollection<string> TailsWithCategory { get; set; } = [];

            public IReadOnlyCollection<string> CapturedTailNumbers { get; private set; } = [];

            public int CapturedCategoryIndex { get; private set; }

            public int CallCount { get; private set; }

            public Task<IReadOnlyCollection<string>> GetTailNumbersWithCategoryAsync(IEnumerable<string> tailNumbers, int categoryIndex, CancellationToken cancellationToken)
            {
                CallCount++;
                CapturedTailNumbers = tailNumbers.ToArray();
                CapturedCategoryIndex = categoryIndex;

                return Task.FromResult(TailsWithCategory);
            }
        }
    }
}
