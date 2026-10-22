using Newtonsoft.Json;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class CampaignManagementMutationsTests
    {
        private readonly CampaignManagementMutations _mutation;
        private readonly Mock<ILogger<CampaignManagementMutations>> _mockLogger;
        private readonly Mock<ICampaignManagementMutationProvider> _mockCampaignManagementProvider;
        private readonly Mock<ITargetedMembershipQueryProvider> _mockTargetedMembershipProvider;

        public CampaignManagementMutationsTests()
        {
            _mockLogger = new Mock<ILogger<CampaignManagementMutations>>();
            _mockCampaignManagementProvider = new Mock<ICampaignManagementMutationProvider>();
            _mockTargetedMembershipProvider = new Mock<ITargetedMembershipQueryProvider>();
            _mutation = new CampaignManagementMutations(_mockCampaignManagementProvider.Object, _mockLogger.Object, _mockTargetedMembershipProvider.Object);
        }

        [Fact]
        public async Task UnassignCampaign_SuccessFromAPI()
        {
            //Arrange
            string requestJson = "{\"CustomerId\":\"a\",\"DocumentId\":\"b\"}";
            CampaignUnassignMutationResponse response = new CampaignUnassignMutationResponse()
            {
                Value = "Un-assign campaign completed successfully"
            };
            CampaignUnassignMutationRequest request = JsonConvert.DeserializeObject<CampaignUnassignMutationRequest>(requestJson);
            _mockCampaignManagementProvider.Setup(x => x.UnassignCampaign(It.IsAny<CampaignUnassignMutationRequest>())).Returns(Task.FromResult(response));
            //Act
            var result = await _mutation.UnassignCampaign(request);

            //Assert
            Assert.Equal("Un-assign campaign completed successfully", result);
        }

        [Fact]
        public async Task UnassignCampaign_ArgumentNullException()
        {
            //Arrange
            CampaignUnassignMutationRequest request = null;

            //Act
            await Assert.ThrowsAsync<ArgumentNullException>(() => _mutation.UnassignCampaign(request));
        }

        [Fact]
        public async Task UnassignCampaign_CustomerIdArgumentNullException()
        {
            //Arrange
            CampaignUnassignMutationRequest request = new CampaignUnassignMutationRequest()
            {
                CustomerId = null,
                DocumentId = "A",
            };

            //Act
            await Assert.ThrowsAsync<ArgumentException>(() => _mutation.UnassignCampaign(request));
        }

        [Fact]
        public async Task UnassignCampaign_documentIdArgumentNullException()
        {
            //Arrange
            CampaignUnassignMutationRequest request = new CampaignUnassignMutationRequest()
            {
                CustomerId = "A",
                DocumentId = null
            };

            //Act
            await Assert.ThrowsAsync<ArgumentException>(() => _mutation.UnassignCampaign(request));
        }

        [Fact]
        public async Task UnassignCampaign_UnhandledlException()
        {
            //Arrange
            CampaignUnassignMutationRequest request = new CampaignUnassignMutationRequest()
            {
                CustomerId = "A",
                DocumentId = "B"
            };
            _mockCampaignManagementProvider.Setup(x => x.UnassignCampaign(It.IsAny<CampaignUnassignMutationRequest>())).Throws(new Exception());

            //Act
            await Assert.ThrowsAsync<Exception>(() => _mutation.UnassignCampaign(request));
        }

        [Fact]
        public async Task UnassignCampaign_EAPIConnectionException()
        {
            //Arrange
            CampaignUnassignMutationRequest request = new CampaignUnassignMutationRequest()
            {
                CustomerId = "A",
                DocumentId = "B"
            };
            _mockCampaignManagementProvider.Setup(x => x.UnassignCampaign(It.IsAny<CampaignUnassignMutationRequest>())).Throws(new HttpRequestException("HttpConnector failed.", new HttpResponseException(System.Net.HttpStatusCode.InternalServerError), System.Net.HttpStatusCode.InternalServerError));

            //Act
            await Assert.ThrowsAsync<HttpRequestException>(() => _mutation.UnassignCampaign(request));
        }

        [Fact]
        public async Task LinkCampaignPromotion_SuccessFromAPI()
        {
            //Arrange
            string requestJson = "{\"CampaignId\":\"1-A976-1\",\"PromotionId\":\"1-2OHIEB\",\"CreatedBy\":\"test@email.com\"}";
            CampaignLinkingMutationRequest request = JsonConvert.DeserializeObject<CampaignLinkingMutationRequest>(requestJson);
            _mockCampaignManagementProvider.Setup(x => x.LinkCampaignPromotion(It.IsAny<CampaignLinkingMutationRequest>())).Returns(Task.FromResult("Ok"));
            //Act
            var result = await _mutation.LinkCampaignPromotion(request);
            //Assert
            Assert.Equal("Ok", result);
        }

        [Fact(Skip = "Implemented test is not configured/implemented correctly")]
        public async Task LinkCampaignPromotion_ArgumentNullException()
        {
            //Arrange
            CampaignLinkingMutationRequest request = null;
            //Act
            await Assert.ThrowsAsync<ArgumentNullException>(() => _mutation.LinkCampaignPromotion(request));
        }

        [Fact(Skip = "Implemented test is not configured/implemented correctly")]
        public async Task LinkCampaignPromotion_CustomerIdArgumentNullException()
        {
            //Arrange
            CampaignLinkingMutationRequest request = new CampaignLinkingMutationRequest()
            {
                CampaignId = null,
                PromotionId = "1-2OHIEB",
                CreatedBy = "test@email.com"
            };
            //Act
            await Assert.ThrowsAsync<ArgumentException>(() => _mutation.LinkCampaignPromotion(request));
        }

        [Fact]
        public async Task LinkCampaignPromotion_UnhandledlException()
        {
            //Arrange
            CampaignLinkingMutationRequest request = new CampaignLinkingMutationRequest()
            {
                CampaignId = "",
                PromotionId = "1-2OHIEB",
                CreatedBy = "test@email.com"
            };
            _mockCampaignManagementProvider.Setup(x => x.LinkCampaignPromotion(It.IsAny<CampaignLinkingMutationRequest>())).Throws(new Exception());
            //Act
            await Assert.ThrowsAsync<Exception>(() => _mutation.LinkCampaignPromotion(request));
        }

        [Fact]
        public async Task LinkCampaignPromotion_EAPIConnectionException()
        {
            //Arrange
            CampaignLinkingMutationRequest request = new CampaignLinkingMutationRequest()
            {
                CampaignId = "",
                PromotionId = "1-2OHIEB",
                CreatedBy = "test@email.com"
            };
            _mockCampaignManagementProvider.Setup(x => x.LinkCampaignPromotion(It.IsAny<CampaignLinkingMutationRequest>())).Throws(new HttpRequestException("HttpConnector failed.", new HttpResponseException(System.Net.HttpStatusCode.InternalServerError), System.Net.HttpStatusCode.InternalServerError));
            //Act
            await Assert.ThrowsAsync<HttpRequestException>(() => _mutation.LinkCampaignPromotion(request));
        }

        [Fact]
        public async Task DelinkCampaignPromotion_SuccessFromAPI()
        {
            //Arrange
            CampaignLinkingMutationRequest request = TestDataProvider.GetTestData<CampaignLinkingMutationRequest>("CampaignManagement//DelinkCampaignPromotionRequest");
            _mockCampaignManagementProvider.Setup(x => x.DelinkCampaignPromotion(It.IsAny<CampaignLinkingMutationRequest>())).Returns(Task.FromResult("Ok"));
            //Act
            var result = await _mutation.DelinkCampaignPromotion(request);
            //Assert
            Assert.Equal("Ok", result);
        }

        [Fact(Skip = "Implemented test is not configured/implemented correctly")]
        public async Task DelinkCampaignPromotion_ArgumentNullException()
        {
            //Arrange
            CampaignLinkingMutationRequest request = null;
            //Act
            await Assert.ThrowsAsync<ArgumentNullException>(() => _mutation.DelinkCampaignPromotion(request));
        }

        [Fact(Skip = "Implemented test is not configured/implemented correctly")]
        public async Task DelinkCampaignPromotion_CustomerIdArgumentNullException()
        {
            //Arrange
            CampaignLinkingMutationRequest request = new CampaignLinkingMutationRequest()
            {
                CampaignId = null,
                PromotionId = "1-2OHIEB"
            };
            //Act
            await Assert.ThrowsAsync<ArgumentException>(() => _mutation.DelinkCampaignPromotion(request));
        }

        [Fact]
        public async Task DelinkCampaignPromotion_UnhandledlException()
        {
            //Arrange
            CampaignLinkingMutationRequest request = new CampaignLinkingMutationRequest()
            {
                CampaignId = "",
                PromotionId = "1-2OHIEB",
            };
            _mockCampaignManagementProvider.Setup(x => x.DelinkCampaignPromotion(It.IsAny<CampaignLinkingMutationRequest>())).Throws(new Exception());
            //Act
            await Assert.ThrowsAsync<Exception>(() => _mutation.DelinkCampaignPromotion(request));
        }

        [Fact]
        public async Task DelinkCampaignPromotion_EAPIConnectionException()
        {
            //Arrange
            CampaignLinkingMutationRequest request = new CampaignLinkingMutationRequest()
            {
                CampaignId = "",
                PromotionId = "1-2OHIEB"
            };
            _mockCampaignManagementProvider.Setup(x => x.DelinkCampaignPromotion(It.IsAny<CampaignLinkingMutationRequest>())).Throws(new HttpRequestException("HttpConnector failed.", new HttpResponseException(System.Net.HttpStatusCode.InternalServerError), System.Net.HttpStatusCode.InternalServerError));
            //Act
            await Assert.ThrowsAsync<HttpRequestException>(() => _mutation.DelinkCampaignPromotion(request));
        }
    }
}
