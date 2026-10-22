using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.HoHAccount;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class HouseHoldAccountDelinkingCommandHandlerTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<HouseHoldAccountDelinkingCommandHandler>> _mockLogger;
        private readonly HouseHoldAccountDelinkingCommandHandler _handler;
        private readonly Mock<IValidator<HouseHoldAccountDelinkingCommand>> _validator;

        public HouseHoldAccountDelinkingCommandHandlerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<HouseHoldAccountDelinkingCommandHandler>>();
            _validator = new Mock<IValidator<HouseHoldAccountDelinkingCommand>>();
            _handler = new HouseHoldAccountDelinkingCommandHandler(_mockMediator.Object, _mockLogger.Object, _validator.Object);
            MemberTestHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new HouseHoldAccountDelinkingCommandHandler(null, _mockLogger.Object, _validator.Object));

            Assert.Throws<ArgumentNullException>(() =>
            new HouseHoldAccountDelinkingCommandHandler(_mockMediator.Object, null, _validator.Object));

            Assert.Throws<ArgumentNullException>(() =>
            new HouseHoldAccountDelinkingCommandHandler(_mockMediator.Object, _mockLogger.Object, null));
        }

        [Fact]
        public void HandlerShouldSuccessWithException()
        {
            HouseHoldAccountDelinkingCommand command = null;
            var result = _handler.Handle(command, CancellationToken.None);
            Assert.NotNull(result.Exception);
        }

        [Fact]
        public void HandlerShouldThrowExceptionForNullHeadAccountAndChildAccount()
        {
            HouseHoldAccountDelinkingCommand command = new HouseHoldAccountDelinkingCommand()
            {
                HeadAccount = null,
                ChildAccount = null
            };

            var validationFailure = new FluentValidation.Results.ValidationFailure("HeadAccount", "15001");

            var validationResult = new ValidationResult(new[] { validationFailure });
            _validator
                .Setup(validator => validator.ValidateAsync(It.IsAny<HouseHoldAccountDelinkingCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            var result = _handler.Handle(command, CancellationToken.None);
            Assert.True(result.Result.IsFailure);
        }

        [Fact]
        public void HandlerShouldThrowExceptionForNullHeadAccount()
        {
            HouseHoldAccount childCustomer = new HouseHoldAccount()
            {
                CustomerId = "CustomerId",
                MemberId = "MemberId",
                FirstName = "First",
                LastName = "Last",
                EmailId = "email@mail.com"
            };
            HouseHoldAccountDelinkingCommand command = new HouseHoldAccountDelinkingCommand()
            {
                HeadAccount = null,
                ChildAccount = childCustomer
            };

            var validationFailure = new FluentValidation.Results.ValidationFailure("HeadAccount", "15001");

            var validationResult = new ValidationResult(new[] { validationFailure });
            _validator
                .Setup(validator => validator.ValidateAsync(It.IsAny<HouseHoldAccountDelinkingCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            var result = _handler.Handle(command, CancellationToken.None);
            Assert.True(result.Result.IsFailure);
        }

        [Fact]
        public void HandlerShouldThrowExceptionForNullChildAccount()
        {
            HouseHoldAccount headCustomer = new HouseHoldAccount()
            {
                CustomerId = "CustomerId",
                MemberId = "MemberId",
                FirstName = "First",
                LastName = "Last",
                EmailId = "email@mail.com"
            };
            HouseHoldAccountDelinkingCommand command = new HouseHoldAccountDelinkingCommand()
            {
                HeadAccount = headCustomer,
                ChildAccount = null
            };
            var validationFailure = new FluentValidation.Results.ValidationFailure("ChildAccount", "15001");

            var validationResult = new ValidationResult(new[] { validationFailure });
            _validator
                .Setup(validator => validator.ValidateAsync(It.IsAny<HouseHoldAccountDelinkingCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            var result = _handler.Handle(command, CancellationToken.None);
            Assert.True(result.Result.IsFailure);
        }

        [Fact]
        public void HandlerShouldThrowExceptionForInvalidGroupId()
        {
            HouseHoldAccount headCustomer = new HouseHoldAccount()
            {
                CustomerId = "CustomerId1",
                MemberId = "MemberId1",
                FirstName = "First",
                LastName = "Last",
                EmailId = "email@mail.com"
            };
            HouseHoldAccount childCustomer = new HouseHoldAccount()
            {
                CustomerId = "CustomerId2",
                MemberId = "MemberId2",
                FirstName = "First",
                LastName = "Last",
                EmailId = "email@mail.com"
            };
            HouseHoldAccountDelinkingCommand command = new HouseHoldAccountDelinkingCommand()
            {
                HeadAccount = headCustomer,
                ChildAccount = childCustomer
            };
            var fcMember = new FlyingClubMember()
            {
                CustomerId = "1",
                Status = "Active",
                TierCode = "Silver"
            };

            _mockMediator.Setup(m => m.Send(It.IsAny<FlyingClubMemberDocumentServiceQuery>(), CancellationToken.None)).ReturnsAsync(new Response<FlyingClubMember>() { Status = Status.OK, Value = fcMember });
            _validator
                .Setup(validator => validator.ValidateAsync(It.IsAny<HouseHoldAccountDelinkingCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var result = _handler.Handle(command, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsFailure);
        }

        [Fact]
        public void HandlerShouldSuccess()
        {
            HouseHoldAccount headCustomer = new HouseHoldAccount()
            {
                CustomerId = "CustomerId1",
                MemberId = "MemberId1",
                FirstName = "First",
                LastName = "Last",
                EmailId = "email@mail.com"
            };
            HouseHoldAccount childCustomer = new HouseHoldAccount()
            {
                CustomerId = "CustomerId2",
                MemberId = "MemberId2",
                FirstName = "First",
                LastName = "Last",
                EmailId = "email@mail.com"
            };
            HouseHoldAccountDelinkingCommand request = new HouseHoldAccountDelinkingCommand()
            {
                HeadAccount = headCustomer,
                ChildAccount = childCustomer
            };

            var flyingclubDoc = TestDataProvider.GetTestData<FlyingClubMember>("MemberManagement\\FlyingclubMemberDocumentWithGroupId");
            _validator
                .Setup(validator => validator.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _mockMediator.Setup(m => m.Send(It.IsAny<FlyingClubMemberDocumentServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<FlyingClubMember> { Status = Status.OK, Value = flyingclubDoc });
            _mockMediator.Setup(m => m.Send(It.IsAny<HouseHoldAccountEmailTriggerCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            var result = _handler.Handle(request, System.Threading.CancellationToken.None);
            Assert.Null(result.Exception);
            Assert.True(result.Result.IsSuccess);
        }


        [Fact]
        public void HandlerShouldFailWithEmailTriggerException()
        {
            HouseHoldAccount headCustomer = new HouseHoldAccount()
            {
                CustomerId = "CustomerId1",
                MemberId = "MemberId1",
                FirstName = "First",
                LastName = "Last",
                EmailId = "email@mail.com"
            };
            HouseHoldAccount childCustomer = new HouseHoldAccount()
            {
                CustomerId = "CustomerId2",
                MemberId = "MemberId2",
                FirstName = "First",
                LastName = "Last",
                EmailId = "email@mail.com"
            };
            HouseHoldAccountDelinkingCommand request = new HouseHoldAccountDelinkingCommand()
            {
                HeadAccount = headCustomer,
                ChildAccount = childCustomer
            };

            var flyingclubDoc = TestDataProvider.GetTestData<FlyingClubMember>("MemberManagement\\FlyingclubMemberDocumentWithGroupId");
            _validator
                .Setup(validator => validator.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _mockMediator.Setup(m => m.Send(It.IsAny<FlyingClubMemberDocumentServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<FlyingClubMember> { Status = Status.OK, Value = flyingclubDoc });
            _mockMediator.Setup(m => m.Send(It.IsAny<HouseHoldAccountEmailTriggerCommand>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            _mockMediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            var result = _handler.Handle(request, System.Threading.CancellationToken.None);
            Assert.Null(result.Exception);
            Assert.True(result.Result.IsFailure);
        }

        [Fact]
        public void HandlerShouldFailWithEventException()
        {
            HouseHoldAccount headCustomer = new HouseHoldAccount()
            {
                CustomerId = "CustomerId1",
                MemberId = "MemberId1",
                FirstName = "First",
                LastName = "Last",
                EmailId = "email@mail.com"
            };
            HouseHoldAccount childCustomer = new HouseHoldAccount()
            {
                CustomerId = "CustomerId2",
                MemberId = "MemberId2",
                FirstName = "First",
                LastName = "Last",
                EmailId = "email@mail.com"
            };
            HouseHoldAccountDelinkingCommand request = new HouseHoldAccountDelinkingCommand()
            {
                HeadAccount = headCustomer,
                ChildAccount = childCustomer
            };

            var flyingclubDoc = TestDataProvider.GetTestData<FlyingClubMember>("MemberManagement\\FlyingclubMemberDocumentWithGroupId");
            _validator
                .Setup(validator => validator.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _mockMediator.Setup(m => m.Send(It.IsAny<FlyingClubMemberDocumentServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<FlyingClubMember> { Status = Status.OK, Value = flyingclubDoc });
            _mockMediator.Setup(m => m.Send(It.IsAny<HouseHoldAccountEmailTriggerCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            var result = _handler.Handle(request, System.Threading.CancellationToken.None);
            Assert.Null(result.Exception);
            Assert.True(result.Result.IsFailure);
        }

        [Fact]
        public void HandlerShouldFailWithSameCustomerId()
        {
            HouseHoldAccount headCustomer = new HouseHoldAccount()
            {
                CustomerId = "CustomerId1",
                MemberId = "MemberId1",
                FirstName = "First",
                LastName = "Last",
                EmailId = "email@mail.com"
            };
            HouseHoldAccount childCustomer = new HouseHoldAccount()
            {
                CustomerId = "CustomerId1",
                MemberId = "MemberId2",
                FirstName = "First",
                LastName = "Last",
                EmailId = "email@mail.com"
            };
            HouseHoldAccountDelinkingCommand request = new HouseHoldAccountDelinkingCommand()
            {
                HeadAccount = headCustomer,
                ChildAccount = childCustomer
            };

            var flyingclubDoc = TestDataProvider.GetTestData<FlyingClubMember>("MemberManagement\\FlyingclubMemberDocumentWithGroupId");
            _validator
                .Setup(validator => validator.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _mockMediator.Setup(m => m.Send(It.IsAny<FlyingClubMemberDocumentServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<FlyingClubMember> { Status = Status.OK, Value = flyingclubDoc });
            var result = _handler.Handle(request, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsFailure);
        }

        [Fact]
        public void HandlerShouldFailWithNullFlyingClubDoc()
        {
            HouseHoldAccount headCustomer = new HouseHoldAccount()
            {
                CustomerId = "CustomerId1",
                MemberId = "MemberId1",
                FirstName = "First",
                LastName = "Last",
                EmailId = "email@mail.com"
            };
            HouseHoldAccount childCustomer = new HouseHoldAccount()
            {
                CustomerId = "CustomerId2",
                MemberId = "MemberId2",
                FirstName = "First",
                LastName = "Last",
                EmailId = "email@mail.com"
            };
            HouseHoldAccountDelinkingCommand request = new HouseHoldAccountDelinkingCommand()
            {
                HeadAccount = headCustomer,
                ChildAccount = childCustomer
            };

            FlyingClubMember flyingclubDoc = null;
            _validator
                .Setup(validator => validator.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _mockMediator.Setup(m => m.Send(It.IsAny<FlyingClubMemberDocumentServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<FlyingClubMember> { Status = Status.OK, Value = flyingclubDoc });
            var result = _handler.Handle(request, System.Threading.CancellationToken.None);
            Assert.True(result.Result.IsFailure);
        }
    }
}



