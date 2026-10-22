using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Application.Command;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.RuleEngine;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Interfaces;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Tests
{
    public class ExecuteTierRuleSetCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldMapSuccessfulInRuleResponse()
        {
            RuleEngineTestHelpers.CreateTestEnvironmentSettings();
            var repository = GetRepository();
            repository.Response = CreateResponse();
            var handler = GetHandler();
            var request = new TierRuleSetEntityRequest
            {
                CustomerId = "test-customer",
                MemberNumber = "123456789"
            };

            var result = await handler.Handle(request, CancellationToken.None);

            Assert.Equal(Status.OK, result.Status);
            Assert.Equal(request.CustomerId, result.Value.ResponseEntity.CustomerId);
            Assert.Equal(request.MemberNumber, result.Value.ResponseEntity.MemberNumber);
            Assert.False(result.Value.HasRuntimeErrors);
            Assert.Equal("Create", Assert.Single(result.Value.ResponseEntity.RuleResponse.Actions).Name);
            Assert.Same(request, ((EntityState)repository.Request.RequestEntity).Customers.Single());
        }

        [Fact]
        public async Task Handle_ShouldReuseCorrelationId_WhenInRuleEvaluationFails()
        {
            RuleEngineTestHelpers.CreateTestEnvironmentSettings();
            var correlationId = Guid.Parse("3f4a0e40-3a7d-46d1-93a8-a57404470057");
            var previousContext = AsyncContext.Current;
            AsyncContext.Current = new AsyncContext { Context = new TestFunctionContext() };
            AsyncContext.Current.SetCorrelationId(correlationId);
            var repository = GetRepository();
            repository.Exception = new InvalidOperationException("InRule is unavailable.");
            var handler = GetHandler();

            try
            {
                var result = await handler.Handle(
                    new TierRuleSetEntityRequest { CustomerId = "test-customer" },
                    CancellationToken.None);

                Assert.Equal(Status.Error, result.Status);
                var error = Assert.Single(result.Errors);
                Assert.Equal("10199", error.Code);
                Assert.Equal(correlationId.ToString(), error.Id);
            }
            finally
            {
                AsyncContext.Current = previousContext;
            }
        }

        [Fact]
        public async Task Handle_ShouldGenerateFallbackErrorId_WhenRequestContextIsMissing()
        {
            RuleEngineTestHelpers.CreateTestEnvironmentSettings();
            var previousContext = AsyncContext.Current;
            AsyncContext.Current = null;
            var repository = GetRepository();
            repository.Exception = new InvalidOperationException("InRule is unavailable.");
            var handler = GetHandler();

            try
            {
                var result = await handler.Handle(
                    new TierRuleSetEntityRequest { CustomerId = "test-customer" },
                    CancellationToken.None);

                var error = Assert.Single(result.Errors);
                Assert.True(Guid.TryParse(error.Id, out var errorId));
                Assert.NotEqual(Guid.Empty, errorId);
            }
            finally
            {
                AsyncContext.Current = previousContext;
            }
        }

        private readonly IServiceProvider _serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddSingleton<TestInRuleRepository>()
            .AddSingleton<IInRuleRepository>(sp => sp.GetRequiredService<TestInRuleRepository>())
            .AddSingleton<IExecuteTierRuleSetCommand, ExecuteTierRuleSetCommandHandler>()
            .BuildServiceProvider();

        private IExecuteTierRuleSetCommand GetHandler() =>
            _serviceProvider.GetRequiredService<IExecuteTierRuleSetCommand>();

        private TestInRuleRepository GetRepository() =>
            _serviceProvider.GetRequiredService<TestInRuleRepository>();

        private static InRuleResponse CreateResponse()
        {
            return new InRuleResponse
            {
                ResponseEntity = Json.Serialize(new TierRuleEntityState
                {
                    Customers = new List<Customer>
                    {
                        new()
                        {
                            RuleResponse = new RuleResponse
                            {
                                Actions = new List<VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.RuleEngine.Action>
                                {
                                    new() { Name = "Create" }
                                }
                            }
                        }
                    }
                })
            };
        }
    }
}
