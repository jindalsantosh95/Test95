using Moq;
using System.Net;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ValidationMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.ValidationMS;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.OfferPrice
{
    public class ValidationMSCommandHandlerTests
    {
        private readonly ValidationMSCommandHandler _handler;
        private readonly Mock<IHttpConnector> _mockHttpConnector;

        public ValidationMSCommandHandlerTests()
        {
            _mockHttpConnector = new Mock<IHttpConnector>();
            _handler = new ValidationMSCommandHandler(_mockHttpConnector.Object);

            ConfigReader.Set("ValidationMSSubscriptionKey", "Test_ValidationMSSubscriptionKey");
            ConfigReader.Set("agentregistrationenvironment", "NONPROD");
            ConfigReader.Set("validationmsurl", "https://testresources.virginatlantic.com/SIT/ValidationMicroservice/Agency/Validation");
            
        }

        [Fact]
        public async Task Handle_ShouldReturnValidationRS_WhenHttpResponseIsSuccessful()
        {
            var agencyValidateResponse = TestDataProvider.GetTestData<AgencyValidateResponse>("NDCOffer\\AgencyValidationResponse");
            var parties = new Parties
            {
                Participants = new List<Participant>
                {
                    new Participant { AgencyID = "91269021", Name = "go4travel", PartyType = Participant.PartyTypeEnum.Retail }
                }
            };

            var command = new ValidationMSCommand
            {
                AgentSubscriptionKey = "Test_ValidationMSSubscriptionKey",
                AgencyValidateRequest = new AgencyValidateRequest {AgentSubscriptionKey= "Test_ValidationMSSubscriptionKey", CheckAgentTktAuthority=false,Environment= "NONPROD" ,Keytype="Retail",Parties= parties }
            };

            var responseContent = Json.Serialize(agencyValidateResponse);
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            };

            _mockHttpConnector.Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), HttpMethod.Post, It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(httpResponseMessage);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("GB", result.ValidateResponse.AgencyDetails.CountryCode);
            Assert.Equal("DIRECT", result.ValidateResponse.KeyType.ToString());
        }

        [Fact]
        public async Task Handle_ShouldReturnErrorResponse_WhenHttpResponseIsNotSuccessful()
        {
            var command = new ValidationMSCommand
            {
                AgentSubscriptionKey = "Test_ValidationMSSubscriptionKey",
                AgencyValidateRequest = new AgencyValidateRequest {AgentSubscriptionKey= "Test_ValidationMSSubscriptionKey" }
            };
            var errorContent = Json.Serialize(new List<ErrorDetails> { new ErrorDetails { Code = "400", Message = "Bad Request" } });
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(errorContent)
            };

            _mockHttpConnector.Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), HttpMethod.Post, It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(httpResponseMessage);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.NotNull(result);
            Assert.NotNull(result.Error);
            Assert.Equal("400",result.Error.First().Code);
        }

      
    }
}
