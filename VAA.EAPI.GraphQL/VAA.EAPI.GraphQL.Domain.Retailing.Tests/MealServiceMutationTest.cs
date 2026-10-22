using HotChocolate;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Interface;
using VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Services;
using VAA.EAPI.GraphQL.Domain.Retailing.Models;
using VAA.EAPI.GraphQL.Domain.Retailing.Models.MealServiceMutaion;
using VAA.EAPI.GraphQL.Domain.Retailing.Mutations;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.Retailing.Tests
{
    public class MealServiceMutationTest : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public MealServiceMutationTest(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("MealServiceProvider:FlightServiceMsUri", "https://tempuri.org");
            ConfigReader.Set("MealServiceProvider:OcpKey", "test123");
            ConfigReader.Set("CateringProvider:CateringMsUri", "https://tempuri.org/catering");
            ConfigReader.Set("cateringprovider:OcpKey", "12345678");


            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<IMealServiceProvider, MealServiceProvider>()
                                                      .AddSingleton<ICateringProvider, CateringProvider>()
                                                      .AddSingleton<MealServiceMutation>()
                                                      .BuildServiceProvider();

            AsyncContext.Current = new() { Context = new Core.Functions.TestFunctionContext() };
        }

        [Fact]
        public async Task Execute_AddMeal_Success()
        {
            var mealServiceMutations = _serviceProvider.GetService<MealServiceMutation>()!;

            var request = GetAddMealRequest();

            var result = await mealServiceMutations.AddMeal(request);

            Assert.Equal("PNR123", result.RecordLocator);
            Assert.Single(result.Segments);
            Assert.Equal("2025-12-15", result.Segments.First().FlightDate.ToString("yyy-MM-dd"));
            Assert.Equal("FF123456", result.Segments.First().Passengers.First().FrequentFlyerNumber);
            Assert.Equal("VGML", result.Segments.First().Passengers.First().Meals.First().Code);
            Assert.Equal("Vegetarian Meal", result.Segments.First().Passengers.First().Meals.First().Description);
        }

        [Fact]
        public async Task AddMeal_ShouldThrowGraphQLException_WhenRequestIsInvalid()
        {
            var mealServiceMutations = _serviceProvider.GetService<MealServiceMutation>()!;

            var request = GetAddMealRequest();
            request.RecordLocator = string.Empty;

            await Assert.ThrowsAsync<GraphQLException>(async () => await mealServiceMutations.AddMeal(request));

        }

        [Fact]
        public async Task Execute_CancelMeal_Success()
        {
            var mealServiceMutations = _serviceProvider.GetService<MealServiceMutation>()!;

            var request = GetCancelMealRequest();

            var result = await mealServiceMutations.CancelMeal(request);

            Assert.Equal("PNR123", result.RecordLocator);
            Assert.Single(result.Segments);
            Assert.Equal("2025-12-15", result.Segments.First().FlightDate.ToString("yyy-MM-dd"));
            Assert.Equal("FF123456", result.Segments.First().Passengers.First().FrequentFlyerNumber);
            Assert.Equal("VGML", result.Segments.First().Passengers.First().Meals.First().Code);
            Assert.Equal("Vegetarian Meal", result.Segments.First().Passengers.First().Meals.First().Description);
        }

        [Fact]
        public async Task CancelMeal_ShouldThrowGraphQLException_WhenRequestIsInvalid()
        {
            var mealServiceMutations = _serviceProvider.GetService<MealServiceMutation>()!;

            var request = GetCancelMealRequest();
            request.RecordLocator = string.Empty;

            await Assert.ThrowsAsync<GraphQLException>(async () => await mealServiceMutations.CancelMeal(request));

        }

        private AddMealRequest GetAddMealRequest()
        {
            return new AddMealRequest
            {
                OrderId = "Order123",
                RecordLocator = "PNR123",
                Segments = new List<FlightSegment>
            {
            new FlightSegment
            {            
            SegmentId = "SEG001",
            FlightNumber = new FlightNumber
            {
                AirlineCode = "VS",
                Number = 302
            },
            Passengers = new List<Passenger>
            {
                new Passenger
                {
                    PassengerId = "PAX001",
                    FirstName = "John",
                    LastName = "Doe",
                    FrequentFlyerNumber = "FF123456",
                    SelectedMeal = "VGML",
                    Meals = new List<Meal>
                    {
                        new Meal { Code = "VGML", Description = "Vegetarian Meal"
                        }
                    }
                },
                new Passenger
                {
                    PassengerId = "PAX002",
                    FirstName = "Jane",
                    LastName = "Smith",
                    FrequentFlyerNumber = "FF654321",
                    SelectedMeal = "CHML",
                    Meals = new List<Meal>
                    {
                        new Meal { Code = "CHML", Description = "Child Meal"
                        }
                    }
                }
            }
        }
    }
            };
        }


        private CancelMealRequest GetCancelMealRequest()
        {
            return new CancelMealRequest
            {
                OrderId = "Order123",
                RecordLocator = "PNR123",
                Segments = new List<FlightSegment>
                {
                new FlightSegment
                    {            
            SegmentId = "SEG001",
            FlightNumber = new FlightNumber
            {
                AirlineCode = "VS",
                Number = 302
            },
            Passengers = new List<Passenger>
            {
                new Passenger
                {
                    PassengerId = "PAX001",
                    FirstName = "John",
                    LastName = "Doe",
                    FrequentFlyerNumber = "FF123456",
                    SelectedMeal = "VGML",
                    Meals = new List<Meal>
                    {
                        new Meal { Code = "VGML", Description = "Vegetarian Meal"
                        }
                    }
                },
                new Passenger
                {
                    PassengerId = "PAX002",
                    FirstName = "Jane",
                    LastName = "Smith",
                    FrequentFlyerNumber = "FF654321",
                    SelectedMeal = "CHML",
                    Meals = new List<Meal>
                    {
                        new Meal { Code = "CHML", Description = "Child Meal"
                        }
                    }
                }
            }
        }
    }
            };

        }
    }
}