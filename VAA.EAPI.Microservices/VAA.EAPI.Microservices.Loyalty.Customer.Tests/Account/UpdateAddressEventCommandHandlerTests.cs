using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using System.Threading;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
using VAA.EAPI.Domain.Entities.Common;
using System.Linq;
using VAA.EAPI.Domain.Events;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.ManageAccount;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.Account
{
    public class UpdateAddressEventCommandHandlerTests
    {
        private readonly Mock<IMediator> _mockmediator;
        private readonly Mock<ILogger<UpdateAddressEventCommandHandler>> _mockLogger;
        private readonly UpdateAddressEventCommandHandler _mockHandler;

        public UpdateAddressEventCommandHandlerTests()
        {
            _mockmediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<UpdateAddressEventCommandHandler>>();
            _mockHandler = new UpdateAddressEventCommandHandler(_mockmediator.Object, _mockLogger.Object);
            SettingsHelper.CreateTestEnvironmentSettings();
        }


        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new UpdateAddressEventCommandHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new UpdateAddressEventCommandHandler(_mockmediator.Object, null));
        }

        [Fact]
        public async Task Handler_ShouldThrowNullException_ForNullRequest()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _mockHandler.Handle(null, CancellationToken.None));
        }

        [Fact]
        public async Task Handler_ShouldReturnSucceess_ForGoodRequestForIndividualMember()
        {
            // Arrange
            var updateMemberAddressCommandRequest = TestDataProvider.GetTestData<UpdateMemberAddressCommand>("CustomerMS\\Account\\UpdateMemberAddressCommandRequest");

            var request = new UpdateAddressEventCommand
            {
                MemberId = "jdasd-dasdas-ddasdasd-232342",
                UpdateAddressRequest = new Domain.ManageAccount.UpdateAddressRequest
                {
                    NewAddressDetails = updateMemberAddressCommandRequest.AddressDetail,
                }
            };

            // Act
            var result = await _mockHandler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Null(result.Errors);
            Assert.Equal(Status.Accepted, result.Status);
            Assert.NotNull(result.HeaderParameters);
            Assert.Contains("TestResponseURL", result.HeaderParameters.FirstOrDefault().Value);
        }

        [Fact]
        public async Task Handler_ShouldReturnError_ForBadRequest()
        {
            // Arrange
            var updateMemberAddressCommandRequest = TestDataProvider.GetTestData<UpdateMemberAddressCommand>("CustomerMS\\Account\\UpdateMemberAddressCommandRequest");

            var request = new UpdateAddressEventCommand
            {
                MemberId = "jdasd-dasdas-ddasdasd-232342",
                UpdateAddressRequest = new Domain.ManageAccount.UpdateAddressRequest
                {
                    NewAddressDetails = null
                }
            };

            _mockmediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

            // Act
            var result = await _mockHandler.Handle(request, CancellationToken.None);

            //Assert                    
            Assert.NotNull(result.Errors);
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.Equal("G009", result.Errors.First().Code);
        }

        [Fact]
        public async Task Handler_ShouldReturnSucceess_ForGoodRequestForHouseHoldandLinkedMember()
        {
            // Arrange
            var updateMemberAddressCommandRequest = TestDataProvider.GetTestData<UpdateMemberAddressCommand>("CustomerMS\\Account\\UpdateMemberAddressCommandRequest");

            var request = new UpdateAddressEventCommand
            {
                MemberId = "jdasd-dasdas-ddasdasd-232342",
                UpdateAddressRequest = new Domain.ManageAccount.UpdateAddressRequest
                {
                    NewAddressDetails = updateMemberAddressCommandRequest.AddressDetail,
                    CustomerIds = new List<string>
                    {
                        "das76s-d523as-as7262d-asd1s",
                        "fsad76-67dass-dasda65-asd55",
                    }
                }
            };

            // Act
            var result = await _mockHandler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Null(result.Errors);
            Assert.Equal(Status.Accepted, result.Status);
            Assert.NotNull(result.HeaderParameters);
            Assert.Contains("TestResponseURL", result.HeaderParameters.FirstOrDefault().Value);
        }
    }
}
