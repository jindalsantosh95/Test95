using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.TargetedMembership;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.TargetedMembership;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public class TargetedMembershipCommandStatusQueryHandlerTest
    {
        protected readonly Mock<IMediator> _mockMediator = new();
        protected readonly Mock<ILogger<CommandStatusBaseQueryHandler<TargetedMembershipCommandQueryResponse>>> _mockLogger = new();

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TargetedMembershipCommandStatusQueryHandler(null, _mockLogger.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TargetedMembershipCommandStatusQueryHandler(_mockMediator.Object, null));
        }
    }
}
