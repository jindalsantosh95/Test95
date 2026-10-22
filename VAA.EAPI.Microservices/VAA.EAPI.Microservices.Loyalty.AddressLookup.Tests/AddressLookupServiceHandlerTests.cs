using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Service;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Tests
{
    public class AddressLookupServiceHandlerTests
    {
        private readonly Mock<ILogger<AddressLookUpServiceHandler>> _mockLogger;
        private readonly AddressLookUpServiceHandler _mockHandler;
        public AddressLookupServiceHandlerTests()
        {
            _mockLogger = new Mock<ILogger<AddressLookUpServiceHandler>>();
            _mockHandler = new AddressLookUpServiceHandler(_mockLogger.Object);
            ConfigReader.Set("WesalEndpoint", "http://dummy.com");
        }

        //[Fact]
        //public async Task Handle_Request_ReturnsNull()
        //{
        //    //Act
        //    // Assert
        //    await Assert.ThrowsAsync<MessageSecurityException>(() => _mockHandler.Handle(new AddressLookUpServiceCommand("dummy"), CancellationToken.None));
        //}
    }

}
