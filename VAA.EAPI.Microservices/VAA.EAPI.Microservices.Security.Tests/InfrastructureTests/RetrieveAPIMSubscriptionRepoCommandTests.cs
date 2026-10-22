using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System.Net;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Security.Applications.Query.APIMSubscription;
using VAA.EAPI.Microservices.Security.Infrastructure.Models.APIMSubscriptionResponse;
using VAA.EAPI.Microservices.Security.Infrastructure.Repositories.APIMSubscription;
using VAA.EAPI.Microservices.Security.Infrastructure.Services.APIMSubscription;

namespace VAA.EAPI.Microservices.Security.Tests.InfrastructureTests
{
    public class RetrieveAPIMSubscriptionRepoCommandTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<RetrieveAPIMSubscriptionRepoQueryHandler>> _logger;

        public RetrieveAPIMSubscriptionRepoCommandTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<RetrieveAPIMSubscriptionRepoQueryHandler>>();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new RetrieveAPIMSubscriptionRepoQueryHandler(null, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new RetrieveAPIMSubscriptionRepoQueryHandler(_mediator.Object, null));
        }

        [Fact]
        public async Task Handler_ShouldReturnAcceptedWhenNoErrorsOccur()
        {
            //Arrange
            var apimRetrieveRQ = new RetrieveAPIMSubscriptionRepoQuery(new SubscriptionRetrieveQuery { SubscriptionId = "SusbcriptionId", Environment = "NONPROD", Channel = "NDC" });
            var mockSubscriptionServiceServiceResponse = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.OK, JsonConvert.SerializeObject(new APIMSubscriptionResponse { Properties = new SubscriptionProperties { PrimaryKey = "SubscriptionKey", State = "active" } }));
            var mockGetSusbcriptionKeyResponse = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.OK, JsonConvert.SerializeObject(new SubscriptionKeys { PrimaryKey = "SubscriptionKey", SecondaryKey = "SecondarySusbcriptionKey" }));


            _mediator.Setup(m => m.Send(It.IsAny<RetrieveAPIMSubscriptionServiceQuery>(), CancellationToken.None)).ReturnsAsync(mockSubscriptionServiceServiceResponse);
            _mediator.Setup(m => m.Send(It.IsAny<GetAPIMSubscriptionKeysServiceQuery>(), CancellationToken.None)).ReturnsAsync(mockGetSusbcriptionKeyResponse);

            //Act
            RetrieveAPIMSubscriptionRepoQueryHandler handler = new(_mediator.Object, _logger.Object);
            var result = await handler.Handle(apimRetrieveRQ, CancellationToken.None);

            //Assert
            Assert.Equal("NDC", result.Channel);
            Assert.Equal("NONPROD", result.Environment);
            Assert.Equal(Status.OK, result.Status);
            Assert.Equal("SubscriptionKey", result.SubscriptionKeys[0].Key);
            Assert.Equal("Enabled", result.SubscriptionState);
        }

        [Fact]
        public async Task Handler_ShouldReturnAcceptedWhenNotFoundOccurs()
        {
            //Arrange
            var apimRetrieveRQ = new RetrieveAPIMSubscriptionRepoQuery(new SubscriptionRetrieveQuery { SubscriptionId = "SusbcriptionId", Environment = "NONPROD", Channel = "NDC" });
            var mockSubscriptionServiceServiceResponse = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.NotFound, JsonConvert.SerializeObject(new APIMSubscriptionResponse { Properties = new SubscriptionProperties { PrimaryKey = "SubscriptionKey", State = "active" } }));
            var mockGetSusbcriptionKeyResponse = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.OK, JsonConvert.SerializeObject(new SubscriptionKeys { PrimaryKey = "SusbcriptionKey", SecondaryKey = "SecondarySusbcriptionKey" }));


            _mediator.Setup(m => m.Send(It.IsAny<RetrieveAPIMSubscriptionServiceQuery>(), CancellationToken.None)).ReturnsAsync(mockSubscriptionServiceServiceResponse);
            _mediator.Setup(m => m.Send(It.IsAny<GetAPIMSubscriptionKeysServiceQuery>(), CancellationToken.None)).ReturnsAsync(mockGetSusbcriptionKeyResponse);

            //Act
            RetrieveAPIMSubscriptionRepoQueryHandler handler = new(_mediator.Object, _logger.Object);
            var result = await handler.Handle(apimRetrieveRQ, CancellationToken.None);

            //Assert
            Assert.Equal(Constants.ErrorCodes.VE002, result.Error.Code);
            Assert.Equal(Constants.ErrorCodes.VE002Reason, result.Error.Message);
            Assert.Equal(Status.OK, result.Status);
        }

        [Fact]
        public async Task Handler_ShouldReturnAcceptedWhenErrorOccurs()
        {
            //Arrange
            var apimRetrieveRQ = new RetrieveAPIMSubscriptionRepoQuery(new SubscriptionRetrieveQuery { SubscriptionId = "SusbcriptionId", Environment = "NONPROD", Channel = "NDC" });
            var mockSubscriptionServiceServiceResponse = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.BadRequest, JsonConvert.SerializeObject(new APIMSubscriptionResponse { Properties = new SubscriptionProperties { PrimaryKey = "SubscriptionKey", State = "active" } }));
            var mockGetSusbcriptionKeyResponse = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.OK, JsonConvert.SerializeObject(new SubscriptionKeys { PrimaryKey = "SusbcriptionKey", SecondaryKey = "SecondarySusbcriptionKey" }));


            _mediator.Setup(m => m.Send(It.IsAny<RetrieveAPIMSubscriptionServiceQuery>(), CancellationToken.None)).ReturnsAsync(mockSubscriptionServiceServiceResponse);
            _mediator.Setup(m => m.Send(It.IsAny<GetAPIMSubscriptionKeysServiceQuery>(), CancellationToken.None)).ReturnsAsync(mockGetSusbcriptionKeyResponse);

            //Act
            RetrieveAPIMSubscriptionRepoQueryHandler handler = new(_mediator.Object, _logger.Object);
            var result = await handler.Handle(apimRetrieveRQ, CancellationToken.None);

            //Assert
            Assert.Equal(Constants.ErrorCodes.G039, result.Error.Code);
            Assert.Equal(Constants.ErrorCodes.G039Reason, result.Error.Message);
            Assert.Equal(Status.Error, result.Status);
        }
    }
}
