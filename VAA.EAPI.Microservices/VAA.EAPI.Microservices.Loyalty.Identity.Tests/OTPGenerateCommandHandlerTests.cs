using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.ServiceBus;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Entity;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.IdentityEntities;
using Xunit;
using IdentityApplication = VAA.EAPI.Microservices.Loyalty.Identity.Constants.Application;
using InfrastructureConstants = VAA.EAPI.Infrastructure.Constants.Constants;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Tests
{
    [Collection("EnvironmentDependentTests")]
    public class OTPGenerateCommandHandlerTests
    {
        private readonly IServiceProvider _serviceProvider;

        public OTPGenerateCommandHandlerTests()
        {
            IdentityTestsCommon.CreateTestEnvironmentSettings();
            ConfigReader.Set(InfrastructureConstants.MicroServiceDBKey, "test-db-key");
            ConfigReader.Set(InfrastructureConstants.MicroServiceEventStoreTableName, "test-event-store");
            ConfigReader.Set(Constants.Environment.ServiceBusConnectionString, "test-connection-string");
            ConfigReader.Set(Constants.Environment.OTPEmailTemplate, "vaaMyAccRePlatOTP");

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<ITableStorageService<GenerateOtpDbEntities>, Core.Xunit.Azure.TableStorageService<GenerateOtpDbEntities>>()
                .AddSingleton<ITableStorageService<EventStoreDatabaseEntity>, Core.Xunit.Azure.TableStorageService<EventStoreDatabaseEntity>>()
                .AddSingleton<IMessageBusFactory, MessageBusFactory>()
                .AddSingleton<EventStoreRequestInsertCommandHandler>()
                .AddSingleton<EventStoreRequestDeleteCommandHandler>()
                .AddSingleton<ServiceBusTopicMessageEventCommandHandler>()
                .AddSingleton<IOtpEmailEventPublisher, OtpEmailEventPublisher>()
                .AddSingleton<OTPGenerateCommandHandler>()
                .AddSingleton<OTPGenerateContinuationTokenCommandHandler>()
                .BuildServiceProvider();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenOtpEmailEventPublisherObjectIsNull()
        {
            var logger = _serviceProvider.GetRequiredService<ILogger<OTPGenerateCommand>>();
            var tableStorageService = _serviceProvider.GetRequiredService<ITableStorageService<GenerateOtpDbEntities>>();

            Assert.Throws<ArgumentNullException>(() =>
                new OTPGenerateCommandHandler(null, logger, tableStorageService));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            var otpEmailEventPublisher = _serviceProvider.GetRequiredService<IOtpEmailEventPublisher>();
            var tableStorageService = _serviceProvider.GetRequiredService<ITableStorageService<GenerateOtpDbEntities>>();

            Assert.Throws<ArgumentNullException>(() =>
                new OTPGenerateCommandHandler(otpEmailEventPublisher, null, tableStorageService));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenTableStorageServiceObjectIsNull()
        {
            var otpEmailEventPublisher = _serviceProvider.GetRequiredService<IOtpEmailEventPublisher>();
            var logger = _serviceProvider.GetRequiredService<ILogger<OTPGenerateCommand>>();

            Assert.Throws<ArgumentNullException>(() =>
                new OTPGenerateCommandHandler(otpEmailEventPublisher, logger, null));
        }

        [Fact]
        public async Task Handler_ShouldReturnError_ForInvalidEmailID()
        {
            var request = new OTPGenerateCommand
            {
                EmailId = "Test@fly@virgin.com"
            };

            var handler = _serviceProvider.GetRequiredService<OTPGenerateCommandHandler>();
            var result = await handler.Handle(request, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("G013", result.Errors.First().Code);
            Assert.Contains("Email id in the request is Invalid or Missing", result.Errors.First().Message);
        }

        [Fact]
        public async Task Handler_ShouldReturnError_ForNullEmailID()
        {
            var request = new OTPGenerateCommand
            {
                EmailId = null
            };

            var handler = _serviceProvider.GetRequiredService<OTPGenerateCommandHandler>();
            var result = await handler.Handle(request, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("G013", result.Errors.First().Code);
            Assert.Equal("Email id in the request is Invalid or Missing", result.Errors.First().Message);
        }

        [Fact]
        public async Task Handler_ShouldReturnError_ForEmptyEmailID()
        {
            var request = new OTPGenerateCommand
            {
                EmailId = string.Empty
            };

            var handler = _serviceProvider.GetRequiredService<OTPGenerateCommandHandler>();
            var result = await handler.Handle(request, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("G013", result.Errors.First().Code);
            Assert.Equal("Email id in the request is Invalid or Missing", result.Errors.First().Message);
        }

        [Fact]
        public async Task Handler_ShouldReturnError_WhenStorageTableCannotBeRead()
        {
            var request = new OTPGenerateCommand
            {
                EmailId = "Test@fly.virgin.com"
            };

            var originalConnectionString = ConfigReader.Get(Constants.Environment.ReferenceDataMicroServiceDBKey);

            try
            {
                ConfigReader.Set(Constants.Environment.ReferenceDataMicroServiceDBKey, "invalid-storage-connection-string");

                var handler = new OTPGenerateCommandHandler(
                    _serviceProvider.GetRequiredService<IOtpEmailEventPublisher>(),
                    _serviceProvider.GetRequiredService<ILogger<OTPGenerateCommand>>(),
                    new TableStorageService<GenerateOtpDbEntities>());

                var result = await handler.Handle(request, CancellationToken.None);

                Assert.NotNull(result);
                Assert.Equal("10155", result.Errors.First().Code);
                Assert.Equal(" Unable to generate OTP at the moment. Please try again later.", result.Errors.First().Message);
            }
            finally
            {
                ConfigReader.Set(Constants.Environment.ReferenceDataMicroServiceDBKey, originalConnectionString);
            }
        }

        [Fact]
        public async Task Handler_ShouldGenerateSixDigitOtp_WhenUsingDefaultOtpRange()
        {
            ConfigReader.Set(Constants.Environment.RandomDigitMinValue, "100000");
            ConfigReader.Set(Constants.Environment.RandomDigitMaxValue, "1000000");

            var handler = _serviceProvider.GetRequiredService<OTPGenerateCommandHandler>();
            var tableStorageService = _serviceProvider.GetRequiredService<ITableStorageService<GenerateOtpDbEntities>>();

            var result = await handler.Handle(new OTPGenerateCommand { EmailId = "Test@fly.virgin.com" }, CancellationToken.None);
            var storedEntity = await tableStorageService.RetrieveAsync(IdentityApplication.OtpGenerationEvent, "test@fly.virgin.com");

            Assert.Equal(Status.OK, result.Status);
            Assert.NotNull(storedEntity);
            Assert.InRange(storedEntity.OTP, 100000, 999999);
        }

        [Fact]
        public async Task ContinuationTokenHandler_ShouldGenerateEightDigitOtp_WhenUsingContinuationRange()
        {
            ConfigReader.Set(Constants.Environment.ContinuationTokenRandomDigitMinValue, "10000000");
            ConfigReader.Set(Constants.Environment.ContinuationTokenRandomDigitMaxValue, "100000000");

            var handler = _serviceProvider.GetRequiredService<OTPGenerateContinuationTokenCommandHandler>();
            var tableStorageService = _serviceProvider.GetRequiredService<ITableStorageService<GenerateOtpDbEntities>>();

            var result = await handler.Handle(new OTPGenerateContinuationTokenCommand { EmailId = "Test@fly.virgin.com" }, CancellationToken.None);
            var storedEntity = await tableStorageService.RetrieveAsync(IdentityApplication.OtpGenerationEvent, "test@fly.virgin.com");

            Assert.Equal(Status.OK, result.Status);
            Assert.NotNull(storedEntity);
            Assert.InRange(storedEntity.OTP, 10000000, 99999999);
        }
    }
}
