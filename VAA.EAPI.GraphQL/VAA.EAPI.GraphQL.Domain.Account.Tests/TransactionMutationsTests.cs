using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using Newtonsoft.Json;
using System.Text;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.Activity;
using VAA.EAPI.GraphQL.Domain.Account.Models.Transaction;
using VAA.EAPI.GraphQL.Domain.Account.Mutations.Secure;
using VAA.EAPI.GraphQL.Infrastructure.Entities.VRedMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    public class TransactionMutationsTests
    {
        private readonly TransactionMutations _mutation;
        private readonly Mock<ITransactionMutationProvider> _mockTransactionProvider;
        private readonly Mock<IVredPointProvider> _mockVredPointProvider;
        private readonly Mock<ITokenB2CProvider<string>> _mocktokenB2CProvider;
        private readonly Mock<IActivityQueryProvider> _mockactivityQueryProvider;
        private readonly Mock<IRefDataService<ProductDetails>> _mockproductRefService;
        private readonly Mock<IRefDataService<PartnerProfileDetails>> _mockpartnerProfileService;

        public TransactionMutationsTests()
        {
            _mockTransactionProvider = new Mock<ITransactionMutationProvider>();
            _mocktokenB2CProvider = new Mock<ITokenB2CProvider<string>>();
            _mockVredPointProvider = new Mock<IVredPointProvider>();
            _mockactivityQueryProvider = new Mock<IActivityQueryProvider>();
            _mockproductRefService = new Mock<IRefDataService<ProductDetails>>();
            _mockpartnerProfileService = new Mock<IRefDataService<PartnerProfileDetails>>();

            _mutation = new TransactionMutations(_mockTransactionProvider.Object, _mocktokenB2CProvider.Object, _mockVredPointProvider.Object, _mockactivityQueryProvider.Object, _mockproductRefService.Object, _mockpartnerProfileService.Object);
            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TransactionMutations(null, _mocktokenB2CProvider.Object, _mockVredPointProvider.Object, _mockactivityQueryProvider.Object, _mockproductRefService.Object, _mockpartnerProfileService.Object));
            Assert.Throws<ArgumentNullException>(() => new TransactionMutations(_mockTransactionProvider.Object, null, _mockVredPointProvider.Object, _mockactivityQueryProvider.Object, _mockproductRefService.Object, _mockpartnerProfileService.Object));
            Assert.Throws<ArgumentNullException>(() => new TransactionMutations(_mockTransactionProvider.Object, _mocktokenB2CProvider.Object, null, _mockactivityQueryProvider.Object, _mockproductRefService.Object, _mockpartnerProfileService.Object));
            Assert.Throws<ArgumentNullException>(() => new TransactionMutations(_mockTransactionProvider.Object, _mocktokenB2CProvider.Object, _mockVredPointProvider.Object, _mockactivityQueryProvider.Object, null, _mockpartnerProfileService.Object));
            Assert.Throws<ArgumentNullException>(() => new TransactionMutations(_mockTransactionProvider.Object, _mocktokenB2CProvider.Object, _mockVredPointProvider.Object, _mockactivityQueryProvider.Object, _mockproductRefService.Object, null));
        }

        [Fact]
        public async Task CreateBurnTransactionShouldSuccess()
        {
            // Arrange
            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginTokenOtherToken");
            var authResponse = TestDataProvider.GetTestData<string>("Account//AuthTokenResponse");
            var vredPointResponse = TestDataProvider.GetTestData<APIResponse<VRedResponse>>("Account//VRedResponse");
            var request = TestDataProvider.GetTestData<TransactionMutationRequest>("Account//TransactionMutationRequest");
            var redemptionRequest = TestDataProvider.GetTestData<RedemptionRequest>("Account//RedemptionRequest");
            var response = TestDataProvider.GetTestData<VredTransactionResponse>("Account//VredTransactionResponse");

            //Act

            // Configure TestContext first then set to current AsyncContext
            var bindingData = new Dictionary<string, object>
            {
                { "Headers", loginToken }
            };

            var testContext = new TestFunctionContext(bindingData);

            var stringContent = new StringContent($@"{{ }}", Encoding.UTF8);

            var httpRequestData = new TestHttpRequestData(testContext, new Uri("http://tempuri.org/"), stringContent.ReadAsStream(), "GET", new Dictionary<string, string>() { { "user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko)" } });

            testContext.Features.Set<HttpRequestData>(httpRequestData);
            testContext.Features.Set(new JwtPrincipalFeature(new System.Security.Claims.ClaimsPrincipal(), JsonConvert.DeserializeObject<Common.Entities.AuthToken>(loginToken)));

            AsyncContext.Current = new AsyncContext() { Context = testContext };

            _mocktokenB2CProvider.Setup(c => c.ValidateTokenAsync(It.IsAny<string>())).ReturnsAsync(authResponse);
            _mockVredPointProvider.Setup(m => m.VredPointBalance(It.IsAny<string>())).ReturnsAsync(vredPointResponse.Response);
            _mockTransactionProvider.Setup(m => m.CreateBurnTransaction(It.IsAny<CreateTransactionRequest>())).ReturnsAsync(response);
            var result = await _mutation.CreateBurnTransaction(request);

            //Assert
            Assert.Equal("AZ1709894001363", result.VaaTransactionId);
            Assert.Equal("12345", result.OrderId);
        }

        [Fact]
        public async Task CreateEarnTransactionShouldSuccess()
        {
            // Arrange

            var request = TestDataProvider.GetTestData<TransactionMutationRequest>("Account//TransactionMutationRequest");
            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginTokenOtherToken");
            var authResponse = TestDataProvider.GetTestData<string>("Account//AuthTokenResponse");
            var vredPointResponse = TestDataProvider.GetTestData<APIResponse<VRedResponse>>("Account//VRedResponse");
            var accrualRequest = TestDataProvider.GetTestData<AccrualRequest>("Account//AccrualRequest");
            var response = TestDataProvider.GetTestData<VredTransactionResponse>("Account//VredTransactionResponse");

            //Act

            // Configure TestContext first then set to current AsyncContext
            var bindingData = new Dictionary<string, object>
            {
                { "Headers", loginToken }
            };

            var testContext = new TestFunctionContext(bindingData);

            var stringContent = new StringContent($@"{{ }}", Encoding.UTF8);

            var httpRequestData = new TestHttpRequestData(testContext, new Uri("http://tempuri.org/"), stringContent.ReadAsStream(), "GET", new Dictionary<string, string>() { { "user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko)" } });

            testContext.Features.Set<HttpRequestData>(httpRequestData);
            testContext.Features.Set(new JwtPrincipalFeature(new System.Security.Claims.ClaimsPrincipal(), JsonConvert.DeserializeObject<Common.Entities.AuthToken>(loginToken)));

            AsyncContext.Current = new AsyncContext() { Context = testContext };

            _mocktokenB2CProvider.Setup(c => c.ValidateTokenAsync(It.IsAny<string>())).ReturnsAsync(authResponse);
            _mockVredPointProvider.Setup(m => m.VredPointBalance(It.IsAny<string>())).ReturnsAsync(vredPointResponse.Response);
            _mockTransactionProvider.Setup(m => m.CreateEarnTransaction(It.IsAny<CreateTransactionRequest>())).ReturnsAsync(response);
            var result = await _mutation.CreateEarnTransaction(request);

            //Assert
            Assert.Equal("AZ1709894001363", result.VaaTransactionId);
            Assert.Equal("12345", result.OrderId);
        }

        [Fact]
        public async Task CancelTransaction_ShouldSuccessWithError()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<CancelTransactionMutation>("Account//CancelTransactionRequest");
            var loginToken = TestDataProvider.GetTestData<string>("Account//LoginTokenOtherToken");
            var authResponse = TestDataProvider.GetTestData<string>("Account//AuthTokenResponse");
            var vredTransaction = TestDataProvider.GetTestData<APIResponse<VredStatementResponse>>("Account//VredActivityStatement");
            var cancelRequest = TestDataProvider.GetTestData<CancellationRequest>("Account//CancelRequest");
            var response = TestDataProvider.GetTestData<APIResponse<VredTransactionResponse>>("Account//CancelTransactionAPIResponse");

            //Act

            // Configure TestContext first then set to current AsyncContext
            var bindingData = new Dictionary<string, object>
            {
                { "Headers", loginToken }
            };

            var testContext = new TestFunctionContext(bindingData);

            var stringContent = new StringContent($@"{{ }}", Encoding.UTF8);

            var httpRequestData = new TestHttpRequestData(testContext, new Uri("http://tempuri.org/"), stringContent.ReadAsStream(), "GET", new Dictionary<string, string>() { { "user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko)" } });

            testContext.Features.Set<HttpRequestData>(httpRequestData);
            testContext.Features.Set(new JwtPrincipalFeature(new System.Security.Claims.ClaimsPrincipal(), JsonConvert.DeserializeObject<Common.Entities.AuthToken>(loginToken)));

            AsyncContext.Current = new AsyncContext() { Context = testContext };

            request.TransactionId = "AZ171456952296";

            _mocktokenB2CProvider.Setup(c => c.ValidateTokenAsync(It.IsAny<string>())).ReturnsAsync(authResponse);
            _mockactivityQueryProvider.Setup(m => m.GetVredStatements(It.IsAny<string>())).ReturnsAsync(vredTransaction.Response);
            _mockTransactionProvider.Setup(m => m.CancelTransaction(It.IsAny<CancellationRequest>())).ReturnsAsync(response.Response);

            //Assert
            await Assert.ThrowsAsync<RequestValidationException>(() => _mutation.CancelTransaction(request));
        }

    }
}
