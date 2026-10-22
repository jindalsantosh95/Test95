using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands.AddressUpdate;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Customer;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.HoHAccount;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class AddressUpdateNotificationCommandHandlerTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ICustomerProvider> _mockCustomerProvider;
        private readonly Mock<ILogger<AddressUpdateNotificationCommandHandler>> _mockLogger;
        private readonly AddressUpdateNotificationCommandHandler _handler;

        public AddressUpdateNotificationCommandHandlerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _mockCustomerProvider = new Mock<ICustomerProvider>();
            _mockLogger = new Mock<ILogger<AddressUpdateNotificationCommandHandler>>();
            _handler = new AddressUpdateNotificationCommandHandler(_mockMediator.Object, _mockLogger.Object, _mockCustomerProvider.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AddressUpdateNotificationCommandHandler(null, _mockLogger.Object, _mockCustomerProvider.Object));

            Assert.Throws<ArgumentNullException>(() =>
            new AddressUpdateNotificationCommandHandler(_mockMediator.Object, null, _mockCustomerProvider.Object));

            Assert.Throws<ArgumentNullException>(() =>
            new AddressUpdateNotificationCommandHandler(_mockMediator.Object, _mockLogger.Object, null));
        }

        [Fact]
        public async Task HandlerShouldSuccess()
        {
            var eventRequest = TestDataProvider.GetTestData<AddressUpdateNotificationCommand>("MemberManagement\\AddressUpdateNotificationEventData");
            Customer customerResponse = TestDataProvider.GetTestData<Customer>("MemberManagement\\CustomerDocument");
            var flyingclubDoc = TestDataProvider.GetTestData<FlyingClubMember>("MemberManagement\\FlyingclubMemberDocumentWithGroupId");
            var gropDoc = TestDataProvider.GetTestData<HouseHoldGroup>("MemberManagement\\GroupDocument");

            _mockCustomerProvider.Setup(x => x.GetCustomerDocument(It.IsAny<string>())).ReturnsAsync(customerResponse);
            _mockMediator.Setup(m => m.Send(It.IsAny<FlyingClubMemberDocumentServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<FlyingClubMember> { Status = Status.OK, Value = flyingclubDoc });
            _mockMediator.Setup(m => m.Send(It.IsAny<GroupDocumentServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<HouseHoldGroup> { Status = Status.OK, Value = gropDoc });
            _mockMediator.Setup(m => m.Send(It.IsAny<HouseHoldAccountEmailTriggerCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await _handler.Handle(eventRequest, CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.Null(result.Value.Errors);
        }

        [Fact]
        public async Task HandlerShouldSuccessWhenAccountNotHoh()
        {
            var eventRequest = TestDataProvider.GetTestData<AddressUpdateNotificationCommand>("MemberManagement\\AddressUpdateNotificationEventData");
            Customer customerResponse = TestDataProvider.GetTestData<Customer>("MemberManagement\\CustomerDocument");
            var flyingclubDoc = TestDataProvider.GetTestData<FlyingClubMember>("MemberManagement\\FlyingclubMemberDocumentWithoutGroupId");

            _mockCustomerProvider.Setup(x => x.GetCustomerDocument(It.IsAny<string>())).ReturnsAsync(customerResponse);
            _mockMediator.Setup(m => m.Send(It.IsAny<FlyingClubMemberDocumentServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<FlyingClubMember> { Status = Status.OK, Value = flyingclubDoc });

            var result = await _handler.Handle(eventRequest, CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Errors);
        }

        [Fact]
        public async Task HandlerShouldSuccessWithValidationFailure()
        {
            var eventRequest = TestDataProvider.GetTestData<AddressUpdateNotificationCommand>("MemberManagement\\AddressUpdateNotificationEventData");

            eventRequest.Data = eventRequest.Data.ToString().Replace("D971F4C1-DC2D-84D1-E053-6941D90A4EF2", "");

            var result = await _handler.Handle(eventRequest, CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Errors);
        }

        [Fact]
        public async Task HandlerShouldFailWhenCustDocNotRetrieved()
        {
            var eventRequest = TestDataProvider.GetTestData<AddressUpdateNotificationCommand>("MemberManagement\\AddressUpdateNotificationEventData");
            Customer customerResponse = null;

            _mockCustomerProvider.Setup(x => x.GetCustomerDocument(It.IsAny<string>())).ReturnsAsync(customerResponse);

            var result = await _handler.Handle(eventRequest, CancellationToken.None);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public async Task HandlerShouldFailWhenGroupDocNotRetrieved()
        {
            var eventRequest = TestDataProvider.GetTestData<AddressUpdateNotificationCommand>("MemberManagement\\AddressUpdateNotificationEventData");
            Customer customerResponse = TestDataProvider.GetTestData<Customer>("MemberManagement\\CustomerDocument");
            var flyingclubDoc = TestDataProvider.GetTestData<FlyingClubMember>("MemberManagement\\FlyingclubMemberDocumentWithGroupId");
            HouseHoldGroup gropDoc = null;

            _mockCustomerProvider.Setup(x => x.GetCustomerDocument(It.IsAny<string>())).ReturnsAsync(customerResponse);
            _mockMediator.Setup(m => m.Send(It.IsAny<FlyingClubMemberDocumentServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<FlyingClubMember> { Status = Status.OK, Value = flyingclubDoc });
            _mockMediator.Setup(m => m.Send(It.IsAny<GroupDocumentServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<HouseHoldGroup> { Status = Status.OK, Value = gropDoc });

            var result = await _handler.Handle(eventRequest, CancellationToken.None);
            Assert.True(result.IsFailure);

        }

        [Fact]
        public async Task HandlerShouldThrowException()
        {
            var eventRequest = TestDataProvider.GetTestData<AddressUpdateNotificationCommand>("MemberManagement\\AddressUpdateNotificationEventData");
            Customer customerResponse = TestDataProvider.GetTestData<Customer>("MemberManagement\\CustomerDocument");
            var flyingclubDoc = TestDataProvider.GetTestData<FlyingClubMember>("MemberManagement\\FlyingclubMemberDocumentWithGroupId");
            var gropDoc = TestDataProvider.GetTestData<HouseHoldGroup>("MemberManagement\\GroupDocument");

            _mockCustomerProvider.Setup(x => x.GetCustomerDocument(It.IsAny<string>())).ReturnsAsync(customerResponse);
            _mockMediator.Setup(m => m.Send(It.IsAny<FlyingClubMemberDocumentServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<FlyingClubMember> { Status = Status.OK, Value = flyingclubDoc });
            _mockMediator.Setup(m => m.Send(It.IsAny<GroupDocumentServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<HouseHoldGroup> { Status = Status.OK, Value = gropDoc });
            _mockMediator.Setup(m => m.Send(It.IsAny<HouseHoldAccountEmailTriggerCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>())).Throws(new Exception());

            var result = await _handler.Handle(eventRequest, CancellationToken.None);
            Assert.True(result.IsFailure);
        }
    }
}
