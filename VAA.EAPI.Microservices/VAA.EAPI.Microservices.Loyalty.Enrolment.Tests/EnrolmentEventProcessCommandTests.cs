using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Application.Commands;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Interface;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Interface.IRepository.CommonRefDB;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Interface.IRepository.FCNumberGeneration;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Interface.IValidation;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Tests
{
    public class EnrolmentEventProcessCommandTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<EnrolmentEventCommandHandler>> _logger;
        private readonly Mock<IValidateReferenceDB> _validateReferenceDB;
        private readonly Mock<IDeDupValidation> _deDupValidation;
        private readonly Mock<IFCNumberGenerationRepository> _fCNumberGenerationRepository;
        private readonly Mock<IErrorMapper> _errorMapper;
        private readonly Mock<ICommonRefDBRepository> _commonRefDBRepository;
        private readonly EnrolmentEventSubscribeCommandHandler _handler;
        private readonly Mock<IBlobStorageServices> _blobStorageServices;

        public EnrolmentEventProcessCommandTests()
        {
            _mediator = new Mock<IMediator>();
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseInsertCommand>(), It.IsAny<CancellationToken>())).Equals(true);

            _logger = new Mock<ILogger<EnrolmentEventCommandHandler>>();
            _validateReferenceDB = new Mock<IValidateReferenceDB>();
            _deDupValidation = new Mock<IDeDupValidation>();
            _fCNumberGenerationRepository = new Mock<IFCNumberGenerationRepository>();
            _errorMapper = new Mock<IErrorMapper>();
            _commonRefDBRepository = new Mock<ICommonRefDBRepository>();
            _blobStorageServices = new Mock<IBlobStorageServices>();
            _handler = new EnrolmentEventSubscribeCommandHandler(_logger.Object, _validateReferenceDB.Object, _deDupValidation.Object, _errorMapper.Object, _fCNumberGenerationRepository.Object, _mediator.Object, _commonRefDBRepository.Object, _blobStorageServices.Object);

        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new EnrolmentEventSubscribeCommandHandler(_logger.Object, _validateReferenceDB.Object, _deDupValidation.Object, _errorMapper.Object, _fCNumberGenerationRepository.Object, null, _commonRefDBRepository.Object, _blobStorageServices.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new EnrolmentEventSubscribeCommandHandler(null, _validateReferenceDB.Object, _deDupValidation.Object, _errorMapper.Object, _fCNumberGenerationRepository.Object, _mediator.Object, _commonRefDBRepository.Object, _blobStorageServices.Object));
        }

        [Fact]
        public async Task EmptyRequest_ShouldReturnSuccessResult()
        {
            //Arrange 

            //Act

            var result = await _handler.Handle(new EnrolmentEventProcessCommand(), new System.Threading.CancellationToken());

            //Assert
            Assert.IsType<Result<Unit>>(result);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task GoodRequest_ShouldReturnSuccessResult()
        {
            //Arrange 

            var request = new EnrolmentEventProcessCommand()
            {
                Data = new EnrolmentRequest()
                {
                    ChannelUserId = "abc",
                    CustomerDetails = new Domain.CustomerDetails.Customer()
                    {
                        FirstName = "xyz"
                    }
                }
            };
            //Act

            var result = await _handler.Handle(new EnrolmentEventProcessCommand(), new System.Threading.CancellationToken());

            //Assert
            Assert.IsType<Result<Unit>>(result);
            Assert.True(result.IsSuccess);
        }
        [Fact]
        public async Task GoodRequest_ShouldReturnSuccess_AfterValidatingDedupCheck()
        {
            //Arrange 
            var request = TestDataProvider.GetTestData<EnrolmentEventProcessCommand>("EnrolmentMS\\EnrolmentRequestDataForValidatingDeDupCheck");
            var blackListedEmailResponse = TestDataProvider.GetTestData<List<string>>("EnrolmentMS\\BlackListedEmailsResponse");
            var errorDetailsResponse = TestDataProvider.GetTestData<List<ErrorDetails>>("EnrolmentMS\\ErrorDetailsResponse");

            System.Environment.SetEnvironmentVariable(Enrolment.Constants.Environment.WebSiteChannelId, "PDM");
            System.Environment.SetEnvironmentVariable(Enrolment.Constants.ErrorCodes.EmailidDuplicateError, "G014");

            _deDupValidation.Setup(x => x.DeDupValidate(It.IsAny<EnrolmentRequest>())).ReturnsAsync(errorDetailsResponse);
            _blobStorageServices.Setup(x => x.Deserialize<List<string>>()).ReturnsAsync(blackListedEmailResponse);

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.IsType<Result<Unit>>(result);
            Assert.True(result.IsSuccess);
        }
    }
}
