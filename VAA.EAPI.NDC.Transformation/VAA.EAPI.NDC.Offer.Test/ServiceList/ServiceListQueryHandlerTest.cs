using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Offer.Application.Query.ServiceList;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.ServiceListRS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Offer.Mapper.GenericDataMapper;
using VAA.EAPI.NDC.Offer.Validations.ServiceListValidations;
using Xunit;

namespace VAA.EAPI.NDC.Offer.Tests.ServiceList
{
    public class ServiceListQueryHandlerTest
    {
        private readonly Mock<IHostEnvironment> _hostEnvironment;
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<IServiceListRequestValidations<IATA_ServiceListRS>> _requestValidations;
        private readonly Mock<IGenericDataMapper> _genericDataMapper;
        private readonly Mock<IServiceListResponseValidation> _responseValidations;
        private readonly ServiceListQueryHandler _handler;
        private readonly Mock<ILogger<ServiceListQueryHandler>> _logger;
        private readonly Mock<IBrandMSCommand> _command;
        private readonly Mock<IBrandMSPTCCommand> _commandPTC;
        private readonly Mock<ITrackAnalyticsEventCommandHandler> _trackAnalyticsEventCommandHandler;

        public ServiceListQueryHandlerTest()
        {
            _mediator = new Mock<IMediator>();
            _requestValidations = new Mock<IServiceListRequestValidations<IATA_ServiceListRS>>();
            _hostEnvironment = new Mock<IHostEnvironment>();
            _genericDataMapper = new Mock<IGenericDataMapper>();
            _responseValidations = new Mock<IServiceListResponseValidation>();
            _logger = new Mock<ILogger<ServiceListQueryHandler>>();
            _command = new Mock<IBrandMSCommand>();
            _commandPTC = new Mock<IBrandMSPTCCommand>();
            _trackAnalyticsEventCommandHandler = new Mock<ITrackAnalyticsEventCommandHandler>();
            _handler = new ServiceListQueryHandler(_mediator.Object, _hostEnvironment.Object, _logger.Object, _requestValidations.Object, _genericDataMapper.Object, _responseValidations.Object, _command.Object, _commandPTC.Object, _trackAnalyticsEventCommandHandler.Object);
        }

        [Fact]
        public void When_MediatorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ServiceListQueryHandler(null, _hostEnvironment.Object, _logger.Object, _requestValidations.Object, _genericDataMapper.Object, _responseValidations.Object, _command.Object, _commandPTC.Object, _trackAnalyticsEventCommandHandler.Object));
        }

        [Fact]
        public void When_HostEnvironmentObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ServiceListQueryHandler(_mediator.Object, null, _logger.Object, _requestValidations.Object, _genericDataMapper.Object, _responseValidations.Object, _command.Object, _commandPTC.Object, _trackAnalyticsEventCommandHandler.Object));
        }

        [Fact]
        public void When_GenericDataMapperObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ServiceListQueryHandler(_mediator.Object, _hostEnvironment.Object, _logger.Object, _requestValidations.Object, null, _responseValidations.Object, _command.Object, _commandPTC.Object, _trackAnalyticsEventCommandHandler.Object));
        }
    }
}
