using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CommonReferenceDetails;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IRepository.CommonRefDB;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IRepository.DuplicateCheck;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.ManageAccount
{
    public class UpdatePhoneNumberCommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<UpdatePhoneNumberCommandHandler>> _logger;
        private readonly Mock<ICommonRefDBRepository> _commonRefDBRepository;
        private readonly Mock<IDeDupRepositoryEnrolment> _deduprepository;


        public UpdatePhoneNumberCommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger= new Mock<ILogger<UpdatePhoneNumberCommandHandler>>();
            _commonRefDBRepository = new Mock<ICommonRefDBRepository>();
            _deduprepository = new Mock<IDeDupRepositoryEnrolment>();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new UpdatePhoneNumberCommandHandler(null, _logger.Object,_deduprepository.Object,_commonRefDBRepository.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new UpdatePhoneNumberCommandHandler(_mediator.Object, null, _deduprepository.Object, _commonRefDBRepository.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenDeDupRepositoryObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new UpdatePhoneNumberCommandHandler(_mediator.Object, _logger.Object, null, _commonRefDBRepository.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenCommonRefDBRepositoryObjectIsNull()
        {
            Assert.Throws<ArgumentException>(() => new UpdatePhoneNumberCommandHandler(_mediator.Object, _logger.Object, _deduprepository.Object, null));
        }

        [Fact]
        public async Task UpdatePhoneNumberCommandHandler_ValidScenario()
        {
            var updatePhoneNumberRequest = TestDataProvider.GetTestData<UpdatePhoneNumberCommand>("CustomerMS\\ManageAccount\\UpdatePhoneNumberRequest");
            var updatePhoneNumberresponse = TestDataProvider.GetTestData<ManageAccountUpdateResponse>("CustomerMS\\ManageAccount\\UpdatePhoneNumberResponse");
            var customerDocResponse = new Response<Customers>
            {
                Value = TestDataProvider.GetTestData<Customers>("CustomerMS\\CustomerDocument\\CustomerDoc")
            };

            updatePhoneNumberRequest.CorrelationId = "43234-2fgf3-dsf56-545fde";
            var countryCodeResponse = TestDataProvider.GetTestData<List<CountryDetails>>("CustomerMS\\Account\\CountryCodeDetailsResponse");

            _mediator.Setup(c => c.Send(It.IsAny<FetchCustomerDocumentQuery>(), CancellationToken.None)).ReturnsAsync(customerDocResponse);
            _mediator.Setup(m => m.Send(It.IsAny<UpdatePhoneNumberEventCommandHandler>(), CancellationToken.None)).ReturnsAsync(updatePhoneNumberresponse);
           
            UpdatePhoneNumberCommandHandler handler = new UpdatePhoneNumberCommandHandler(_mediator.Object, _logger.Object, _deduprepository.Object, _commonRefDBRepository.Object);
            _commonRefDBRepository.Setup(s => s.GetCountryCodeDetails()).ReturnsAsync(countryCodeResponse);

            var result = await handler.Handle(updatePhoneNumberRequest, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("Updated Phone Number Successfully", result.Value.ToString());
        }

        [Fact]
        public async Task UpdatePhoneNumberCommandHandler_ErrorScenario()
        {
            var updatePhoneNumberRequest = TestDataProvider.GetTestData<UpdatePhoneNumberCommand>("CustomerMS\\ManageAccount\\UpdatePhoneNumberRequest");
            var updatePhoneNumberresponse = TestDataProvider.GetTestData<ManageAccountUpdateResponse>("CustomerMS\\ManageAccount\\UpdatePhoneNumberResponse");
            var customerDocResponse = new Response<Customers>
            {
                Value = TestDataProvider.GetTestData<Customers>("CustomerMS\\CustomerDocument\\CustomerDoc")
            };
            updatePhoneNumberRequest.CorrelationId = "43234-2fgf3-dsf56-545fde";
            
            _mediator.Setup(c => c.Send(It.IsAny<FetchCustomerDocumentQuery>(), CancellationToken.None)).ReturnsAsync(customerDocResponse);
            UpdatePhoneNumberCommandHandler handler = new UpdatePhoneNumberCommandHandler(_mediator.Object, _logger.Object, _deduprepository.Object, _commonRefDBRepository.Object);
            _commonRefDBRepository.Setup(s => s.GetCountryCodeDetails()).ReturnsAsync(new List<CountryDetails>());

            var result = await handler.Handle(updatePhoneNumberRequest, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("G009", result.Errors.First().Code);
            Assert.Equal("There was an unexpected error.", result.Errors.First().Message);

        }
    }
}
