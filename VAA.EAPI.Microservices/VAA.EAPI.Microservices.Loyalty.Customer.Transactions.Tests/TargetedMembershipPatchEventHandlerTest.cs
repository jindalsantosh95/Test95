using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.TargetedMemberships;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.TargetedMembership;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.TargetedMembership;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class TargetedMembershipPatchEventHandlerTest
    {
        protected readonly Mock<IMediator> _mockMediator;
        protected readonly Mock<ILogger<TransactionBaseEventHandler>> _mockLogger;
        private readonly TargetedMembershipPatchEventHandler _handler;

        public TargetedMembershipPatchEventHandlerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<TransactionBaseEventHandler>>();
            _handler = new TargetedMembershipPatchEventHandler(_mockMediator.Object, _mockLogger.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TargetedMembershipPatchEventHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TargetedMembershipPatchEventHandler(_mockMediator.Object, null));
        }

        [Fact]
        public void HandlerSuccessWithNoError()
        {
            List<TargetedMembership> memberCampaign = TestDataProvider.GetTestData<List<TargetedMembership>>("TargetedMembership\\ListOfTargetedMembership");
            string customerID = "D4718732-BAEF-E0ED-E053-6941D90AAD4B";

            var targetedMemberResponse = Result.Success(new Response<TargetedMembershipResponse>()
            {
                Value = new TargetedMembershipResponse()
                {
                    CustomerId = customerID,
                    TargetedMembershipDocuments = memberCampaign
                }
            });

            _mockMediator.Setup(m => m.Send(It.IsAny<ServiceResponseInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Send(It.IsAny<TargetedMembershipQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(targetedMemberResponse);

            var eventRequest = TestDataProvider.GetTestData<TargetedMembershipPatchEvent>("TargetedMembership\\TargertedMembershipPatchEventData");
            var response = _handler.Handle(eventRequest, System.Threading.CancellationToken.None);
            Assert.True(response.Result.IsSuccess);
            Assert.False(response.Result.IsFailure);
        }

        [Fact]
        public void HandlerSuccessWithExceptionValidData()
        {

            List<TargetedMembership> memberCampaign = TestDataProvider.GetTestData<List<TargetedMembership>>("TargetedMembership\\ListOfTargetedMembership");
            string customerID = "D4718732-BAEF-E0ED-E053-6941D90AAD4B";

            var targetedMemberResponse = Result.Success(new Response<TargetedMembershipResponse>()
            {
                Value = new TargetedMembershipResponse()
                {
                    CustomerId = customerID,
                    TargetedMembershipDocuments = memberCampaign
                }
            });

            _mockMediator.Setup(m => m.Send(It.IsAny<ServiceResponseInsertCommand>(), System.Threading.CancellationToken.None)).Throws(new Exception());
            _mockMediator.Setup(m => m.Send(It.IsAny<TargetedMembershipQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(targetedMemberResponse);

            var eventRequest = TestDataProvider.GetTestData<TargetedMembershipPatchEvent>("TargetedMembership\\TargertedMembershipPatchEventData");
            var response = _handler.Handle(eventRequest, System.Threading.CancellationToken.None);
            Assert.NotEmpty(response.Exception.InnerExceptions);
        }

    }
}
