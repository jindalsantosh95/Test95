using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Entity;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Vred.Applications.Commands.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services.Interface;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Tests
{
    public class UpdateVredCustomerDetailsHandlerTest : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public UpdateVredCustomerDetailsHandlerTest(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("microservicedbkey", "DefaultEndpointsProtocol = https; AccountName = dummyaccount; AccountKey = dummyaccountkey1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef; TableEndpoint = https://dummyaccount.table.core.windows.net/");
            ConfigReader.Set("microserviceeventstoretablename", "TestTable");
            ConfigReader.Set("microserviceresponsetablename", "TestResponseTable");
            ConfigReader.Set("updatevredauthurl", "http://tempuri.org");
            ConfigReader.Set("vredauthurl", "http://tempuri.org");
            ConfigReader.Set("authclientid", "123");
            ConfigReader.Set("authclientsecret", "xxx");
            ConfigReader.Set("authaudience", "12344");

            var mediatorMock = new Mock<IMediator>();
            mediatorMock
                .Setup(m => m.Send(It.IsAny<UpdateVredRepoQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _serviceProvider = new ServiceCollection()
                 .AddLogging()
                 .AddSingleton<IHttpConnector, HttpConnector>()
                 .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                 .AddSingleton<IVredCustomerDetails, UpdateVredCustomerDetailsHandler>()
                 .AddSingleton(typeof(ITableStorageService<>), typeof(Core.Xunit.Azure.TableStorageService<>))
                 .AddSingleton<EventStoreRequestInsertCommandHandler>()
                 .AddSingleton<EventStoreRequestDeleteCommandHandler>()
                 .AddSingleton<ServiceResponseInsertCommandHandler>()
                 .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UpdateVredRepoHandler).Assembly))
                 .AddSingleton<UpdateVredRepoHandler>()
                 .AddSingleton<IMediator>(mediatorMock.Object)
                 .BuildServiceProvider();
        }

        [Fact]
        public async Task UpdateVredCustomerDetailsHandler_Should_Return_When_UpdateRepoSucceeds()
        {
            var table = _serviceProvider.GetService<ITableStorageService<ServiceResponseDatabaseEntity>>()!;
            var entity = new ServiceResponseDatabaseEntity
            {
                PartitionKey = "VAA.Vred.UpdateVredCustomerDetail",
                RowKey = "5100f1be-9f61-42f1-b1c1-6db3ab6ec23e",
                Id = "12345",
                Status = "Completed",
                ResponseData = "{...}",
                Type = "VAA.Vred.UpdateVredCustomerDetail",
                Error = null
            };

            await table.InsertOrMergeAsync(entity);

            var handler = _serviceProvider.GetRequiredService<IVredCustomerDetails>();

            var command = new UpdateVredMemberDetailsRequestDataPayload
            {
                Data = new UpdateVredMemberDetailsRequest
                {
                    MemberNumber = "12345",
                    MemberStatus = "Active",
                    Channel = "Web",
                    CreateUpdateTimestamp = "2026-05-08T07:20:40Z",
                    CustomerId = "5100f1be-9f61-42f1-b1c1-6db3ab6ec23e",
                    FirstName = "John",
                    LastName = "Doe",
                    EmailId = "john.doe@example.com"
                }
            };

            var result = await handler.Handle(command, CancellationToken.None);

            var retriveEntity = await table.RetrieveAsync("VAA.Vred.UpdateVredCustomerDetail", "5100f1be-9f61-42f1-b1c1-6db3ab6ec23e");

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(retriveEntity);
            Assert.Equal("Completed", retriveEntity.Status);
            Assert.Equal("12345", retriveEntity.Id);
            Assert.Equal("VAA.Vred.UpdateVredCustomerDetail", retriveEntity.PartitionKey);
            Assert.Equal("5100f1be-9f61-42f1-b1c1-6db3ab6ec23e", retriveEntity.RowKey);
            Assert.Null(retriveEntity.Error);
        }


        [Fact]
        public async Task HandleAsync_UpdateVredMemberDetailsRequestEmpty_ThrowsException()
        {
            var handler = _serviceProvider.GetService<IVredCustomerDetails>()!;

            var message = new UpdateVredMemberDetailsRequestDataPayload
            {
                Data = new UpdateVredMemberDetailsRequest
                {
                }
            };

            await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(null, CancellationToken.None));
        }

        [Fact]
        public async Task HandleAsync_VredMemberNumberEmpty_ThrowsException()
        {
            var handler = _serviceProvider.GetService<IVredCustomerDetails>()!;

            var message = new UpdateVredMemberDetailsRequestDataPayload
            {
                Data = new UpdateVredMemberDetailsRequest
                {
                    CustomerId = "5100f1be-9f61-42f1-b1c1-6db3ab6ec23e",
                    MemberNumber = "",
                    MemberStatus = "Active",
                    FirstName = "Vtest",
                    LastName = "VirginTest",
                    EmailId = "zyx987@gmail.com",
                    Channel = "powerapps",
                    CreateUpdateTimestamp = "2026-05-08T07:20:40Z"
                }
            };

            await Assert.ThrowsAsync<ArgumentNullException>(() => handler.Handle(message, CancellationToken.None));
        }

    }

}

