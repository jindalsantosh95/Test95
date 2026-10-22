using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignTypeRetrieval;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Application.Command.CampaignTypeRetrieval;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignTypeRetrievalResponseQueryHandlerTest
    {
        private readonly Mock<ILogger<CampaignTypeRetrievalResponseQueryHandler>> _logger;
        private readonly Mock<IMediator> _mediator;
        private readonly CampaignTypeRetrievalResponseQueryHandler _handler;
        private readonly Mock<ICommonRefDBRepository> _commonRefDBRepository;

        public CampaignTypeRetrievalResponseQueryHandlerTest()
        {
            _logger = new Mock<ILogger<CampaignTypeRetrievalResponseQueryHandler>>();
            _mediator = new Mock<IMediator>();
            _commonRefDBRepository = new Mock<ICommonRefDBRepository>();
            _handler = new CampaignTypeRetrievalResponseQueryHandler(_mediator.Object, _logger.Object);
        }


        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignTypeRetrievalResponseQueryHandler(_mediator.Object, null));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
             new CampaignTypeRetrievalResponseQueryHandler(null, _logger.Object));
        }

        [Fact]
        public async Task HandlerSuccessWithNoError()
        {
            var campaignTypeResponse = TestDataProvider.GetTestData<CampaignTypeResponse>("CampaignManagementMS\\CampaignTypeResponse");
            _mediator.Setup(m => m.Send(It.IsAny<CampaignTypesRetrieveCommand>(), CancellationToken.None)).ReturnsAsync(new Response<CampaignTypeResponse> { Status = Status.OK, Value = campaignTypeResponse });
            CampaignTypeRetrievalQuery query = new() { };
            var result = await _handler.Handle(query, CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value.Value);
        }

        [Fact]
        public async Task HandlerSuccessWithError()
        {
            var campaignTypeResponse = TestDataProvider.GetTestData<CampaignTypeResponse>("CampaignManagementMS\\CampaignTypeResponse");
            _mediator.Setup(m => m.Send(It.IsAny<CampaignTypesRetrieveCommand>(), CancellationToken.None)).Throws(new Exception());
            CampaignTypeRetrievalQuery query = new() { };
            var result = await _handler.Handle(query, CancellationToken.None);
            Assert.True(result.IsFailure);
        }


    }
}