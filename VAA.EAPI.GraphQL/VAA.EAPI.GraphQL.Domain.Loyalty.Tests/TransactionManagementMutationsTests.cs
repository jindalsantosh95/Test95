using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Reference;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.TxnStatementText;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class TransactionManagementMutationsTests
    {
        private readonly TransactionManagementMutations _mutation;
        private readonly Mock<ICustomerTransactionQueryProvider> _mockCustTxnQueryProvider;
        private readonly Mock<ICustomerTransactionMutationProvider<TransactionResponse>> _mockCustTxnMutationProvider;
        private readonly Mock<ICustomerProvider> _mockCustomerProvider;
        private readonly Mock<IProductsQueryProvider> _mockProductsProvider;
        private readonly Mock<IPartnersProfilesQueryProvider> _mockPartnerProfileProvider;
        private readonly Mock<IVredMutationProvider> _mockVredProvider;
        private readonly Mock<ILoyaltyMemberMgtProvider> _mockMbrMgmtProvider;
        private readonly Mock<ILogger<TransactionManagementMutations>> _mocklogger;
        private readonly Mock<IVredCardDetailsQueryProvider> _mockVredQueryprovider;


        public TransactionManagementMutationsTests()
        {
            _mockCustTxnQueryProvider = new Mock<ICustomerTransactionQueryProvider>();
            _mockCustTxnMutationProvider = new Mock<ICustomerTransactionMutationProvider<TransactionResponse>>();
            _mockCustomerProvider = new Mock<ICustomerProvider>();
            _mockProductsProvider = new Mock<IProductsQueryProvider>();
            _mockPartnerProfileProvider = new Mock<IPartnersProfilesQueryProvider>();
            _mockVredProvider = new Mock<IVredMutationProvider>();
            _mockMbrMgmtProvider = new Mock<ILoyaltyMemberMgtProvider>();
            _mocklogger = new Mock<ILogger<TransactionManagementMutations>>();
            _mockVredQueryprovider = new Mock<IVredCardDetailsQueryProvider>();
            _mutation = new TransactionManagementMutations(_mockCustomerProvider.Object, _mockCustTxnMutationProvider.Object, _mockCustTxnQueryProvider.Object, _mockPartnerProfileProvider.Object, _mockProductsProvider.Object, _mockVredProvider.Object, _mockMbrMgmtProvider.Object, _mocklogger.Object, _mockVredQueryprovider.Object);
            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TransactionManagementMutations(_mockCustomerProvider.Object, _mockCustTxnMutationProvider.Object, _mockCustTxnQueryProvider.Object, _mockPartnerProfileProvider.Object, _mockProductsProvider.Object, null, _mockMbrMgmtProvider.Object, _mocklogger.Object, _mockVredQueryprovider.Object));
            Assert.Throws<ArgumentNullException>(() => new TransactionManagementMutations(_mockCustomerProvider.Object, _mockCustTxnMutationProvider.Object, _mockCustTxnQueryProvider.Object, _mockPartnerProfileProvider.Object, null, _mockVredProvider.Object, _mockMbrMgmtProvider.Object, _mocklogger.Object, _mockVredQueryprovider.Object));
            Assert.Throws<ArgumentNullException>(() => new TransactionManagementMutations(_mockCustomerProvider.Object, _mockCustTxnMutationProvider.Object, _mockCustTxnQueryProvider.Object, null, _mockProductsProvider.Object, _mockVredProvider.Object, _mockMbrMgmtProvider.Object, _mocklogger.Object, _mockVredQueryprovider.Object));
            Assert.Throws<ArgumentNullException>(() => new TransactionManagementMutations(_mockCustomerProvider.Object, _mockCustTxnMutationProvider.Object, null, _mockPartnerProfileProvider.Object, _mockProductsProvider.Object, _mockVredProvider.Object, _mockMbrMgmtProvider.Object, _mocklogger.Object, _mockVredQueryprovider.Object));
            Assert.Throws<ArgumentNullException>(() => new TransactionManagementMutations(_mockCustomerProvider.Object, null, _mockCustTxnQueryProvider.Object, _mockPartnerProfileProvider.Object, _mockProductsProvider.Object, _mockVredProvider.Object, _mockMbrMgmtProvider.Object, _mocklogger.Object, _mockVredQueryprovider.Object));
            Assert.Throws<ArgumentNullException>(() => new TransactionManagementMutations(null, _mockCustTxnMutationProvider.Object, _mockCustTxnQueryProvider.Object, _mockPartnerProfileProvider.Object, _mockProductsProvider.Object, _mockVredProvider.Object, _mockMbrMgmtProvider.Object, _mocklogger.Object, _mockVredQueryprovider.Object));
            Assert.Throws<ArgumentNullException>(() => new TransactionManagementMutations(_mockCustomerProvider.Object, _mockCustTxnMutationProvider.Object, _mockCustTxnQueryProvider.Object, _mockPartnerProfileProvider.Object, _mockProductsProvider.Object, _mockVredProvider.Object, null, _mocklogger.Object, _mockVredQueryprovider.Object));
        }

        [Fact]
        public async Task TransactionManagementMutationsShouldSuccess()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<CreateTransactionMutationRequest>("Loyalty\\Transaction\\CreateAccrualTransactionRequest");
            var customerDTO = TestDataProvider.GetTestData<CustomerDTO>("Loyalty\\CustomerDTO");
            var awardDetailsResponse = TestDataProvider.GetTestData<APIResponse<AwardResponse>>("Loyalty\\AwardDetails_APIResponse");
            var partnerProfilesResponse = TestDataProvider.GetTestData<APIResponse<IEnumerable<PartnerProfileDetails>>>("MemberCardLink\\PartnersProfiles_APIResponse");
            var productResponse = TestDataProvider.GetTestData<APIResponse<IEnumerable<ProductDetails>>>("Loyalty\\Products_APIResponse");
            var txnCreateResponse = TestDataProvider.GetTestData<APIResponse<TransactionResponse>>("Loyalty\\Transaction\\TransactionCreation_APIResponse");
            var vredCreateResponse = TestDataProvider.GetTestData<APIResponse<VRedTransactionResponse>>("Loyalty\\Transaction\\VredSync_APIResponse");
            var stmtResponse = TestDataProvider.GetTestData<APIResponse<TransactionStatementTextResponse>>("Loyalty\\TransactionStatementTextAPIResponse");

            _mockCustomerProvider.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerDTO);
            _mockCustTxnQueryProvider.Setup(x => x.GetAwardDetails(It.IsAny<string>())).ReturnsAsync(awardDetailsResponse.Response);
            _mockPartnerProfileProvider.Setup(m => m.GetPartnerProfileDetails()).ReturnsAsync(partnerProfilesResponse.Response);
            _mockProductsProvider.Setup(m => m.GetProductDetails()).ReturnsAsync(productResponse.Response);
            _mockCustTxnMutationProvider.Setup(s => s.CreateTransaction(It.IsAny<TransactionRequest>())).ReturnsAsync(txnCreateResponse.Response);
            _mockVredProvider.Setup(s => s.PostTransactionToVred(It.IsAny<VredSyncTransactionRequest>())).ReturnsAsync(vredCreateResponse.Response);
            _mockMbrMgmtProvider.Setup(k => k.GenerateStatetmentText(It.IsAny<TransactionStatementTextRequest>())).ReturnsAsync(stmtResponse.Response);

            //Act
            var result = await _mutation.CreateAccrualTransaction(request);

            //Assert
            Assert.Equal("171871054135506012", result.TransactionId);
            Assert.Equal("Processed", result.TransactionStatus);
        }

        [Fact]
        public async Task TransactionManagementMutationsValidationFailure()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<CreateTransactionMutationRequest>("Loyalty\\Transaction\\CreateAccrualTransactionRequest");
            request.CreatedBy = null;

            //Assert
            await Assert.ThrowsAsync<RequestValidationException>(() => _mutation.CreateAccrualTransaction(request));
        }

        [Fact]
        public async Task TransactionManagementMutationsMissingReferenceData()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<CreateTransactionMutationRequest>("Loyalty\\Transaction\\CreateAccrualTransactionRequest");
            var customerDTO = TestDataProvider.GetTestData<CustomerDTO>("Loyalty\\CustomerDTO");
            var awardDetailsResponse = TestDataProvider.GetTestData<APIResponse<AwardResponse>>("Loyalty\\AwardDetails_APIResponse");

            _mockCustomerProvider.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerDTO);
            _mockCustTxnQueryProvider.Setup(x => x.GetAwardDetails(It.IsAny<string>())).ReturnsAsync(awardDetailsResponse.Response);
            _mockPartnerProfileProvider.Setup(m => m.GetPartnerProfileDetails()).ReturnsAsync(new List<PartnerProfileDetails>() { });

            //Assert
            await Assert.ThrowsAsync<RequestValidationException>(() => _mutation.CreateAccrualTransaction(request));
        }

        [Fact]
        public async Task TransactionManagementMutationsStatusValidationFailure()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<CreateTransactionMutationRequest>("Loyalty\\Transaction\\CreateAccrualTransactionRequest");
            var customerDTO = TestDataProvider.GetTestData<CustomerDTO>("Loyalty\\CustomerDTO");
            var awardDetailsResponse = TestDataProvider.GetTestData<APIResponse<AwardResponse>>("Loyalty\\AwardDetails_APIResponse");
            var partnerProfilesResponse = TestDataProvider.GetTestData<APIResponse<IEnumerable<PartnerProfileDetails>>>("MemberCardLink\\PartnersProfiles_APIResponse");
            var productResponse = TestDataProvider.GetTestData<APIResponse<IEnumerable<ProductDetails>>>("Loyalty\\Products_APIResponse");

            request.MemberStatus = "Deceased";
            request.MemberNumber = "9123434545";

            _mockCustomerProvider.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerDTO);
            _mockCustTxnQueryProvider.Setup(x => x.GetAwardDetails(It.IsAny<string>())).ReturnsAsync(awardDetailsResponse.Response);
            _mockPartnerProfileProvider.Setup(m => m.GetPartnerProfileDetails()).ReturnsAsync(partnerProfilesResponse.Response);
            _mockProductsProvider.Setup(m => m.GetProductDetails()).ReturnsAsync(productResponse.Response);

            //Assert
            await Assert.ThrowsAsync<RequestValidationException>(() => _mutation.CreateAccrualTransaction(request));

        }

        [Fact]
        public async Task TransactionManagementMutationsCosmosCreateFailure()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<CreateTransactionMutationRequest>("Loyalty\\Transaction\\CreateAccrualTransactionRequest");
            var customerDTO = TestDataProvider.GetTestData<CustomerDTO>("Loyalty\\CustomerDTO");
            var awardDetailsResponse = TestDataProvider.GetTestData<APIResponse<AwardResponse>>("Loyalty\\AwardDetails_APIResponse");
            var partnerProfilesResponse = TestDataProvider.GetTestData<APIResponse<IEnumerable<PartnerProfileDetails>>>("MemberCardLink\\PartnersProfiles_APIResponse");
            var productResponse = TestDataProvider.GetTestData<APIResponse<IEnumerable<ProductDetails>>>("Loyalty\\Products_APIResponse");
            var vredCreateResponse = TestDataProvider.GetTestData<APIResponse<VRedTransactionResponse>>("Loyalty\\Transaction\\VredSync_APIResponse");
            var stmtResponse = TestDataProvider.GetTestData<APIResponse<TransactionStatementTextResponse>>("Loyalty\\TransactionStatementTextAPIResponse");


            TransactionResponse txnCreateResponse = new TransactionResponse() { Errors = new List<ErrorDetails> { new ErrorDetails() { Code = "G009", Message = "There was an unexpected error.", DeveloperMessage = "Miscellaneous technical error. | ErrorID: 13519065-348e-4853-a486-c0dd08eed59a | TimeStamp: 6/18/2024 4:08:35 PM", ErrorSource = "Source system" } } };

            _mockCustomerProvider.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerDTO);
            _mockCustTxnQueryProvider.Setup(x => x.GetAwardDetails(It.IsAny<string>())).ReturnsAsync(awardDetailsResponse.Response);
            _mockPartnerProfileProvider.Setup(m => m.GetPartnerProfileDetails()).ReturnsAsync(partnerProfilesResponse.Response);
            _mockProductsProvider.Setup(m => m.GetProductDetails()).ReturnsAsync(productResponse.Response);
            _mockCustTxnMutationProvider.Setup(s => s.CreateTransaction(It.IsAny<TransactionRequest>())).ReturnsAsync(txnCreateResponse);
            _mockVredProvider.Setup(s => s.PostTransactionToVred(It.IsAny<VredSyncTransactionRequest>())).ReturnsAsync(vredCreateResponse.Response);
            _mockMbrMgmtProvider.Setup(k => k.GenerateStatetmentText(It.IsAny<TransactionStatementTextRequest>())).ReturnsAsync(stmtResponse.Response);

            //Act
            var result = await _mutation.CreateAccrualTransaction(request);

            //Assert
            Assert.Equal("G009", result.Errors?.FirstOrDefault()?.Code);
            Assert.Equal("LOOPS Service failure - There was an unexpected error.", result.Errors?.FirstOrDefault()?.Message);
        }

        [Fact]
        public async Task TransactionManagementMutationsVredCreateFailure()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<CreateTransactionMutationRequest>("Loyalty\\Transaction\\CreateAccrualTransactionRequest");
            var customerDTO = TestDataProvider.GetTestData<CustomerDTO>("Loyalty\\CustomerDTO");
            var awardDetailsResponse = TestDataProvider.GetTestData<APIResponse<AwardResponse>>("Loyalty\\AwardDetails_APIResponse");
            var partnerProfilesResponse = TestDataProvider.GetTestData<APIResponse<IEnumerable<PartnerProfileDetails>>>("MemberCardLink\\PartnersProfiles_APIResponse");
            var productResponse = TestDataProvider.GetTestData<APIResponse<IEnumerable<ProductDetails>>>("Loyalty\\Products_APIResponse");
            var txnCreateResponse = TestDataProvider.GetTestData<APIResponse<TransactionResponse>>("Loyalty\\Transaction\\TransactionCreation_APIResponse");
            var stmtResponse = TestDataProvider.GetTestData<APIResponse<TransactionStatementTextResponse>>("Loyalty\\TransactionStatementTextAPIResponse");
            VRedTransactionResponse vredCreateResponse = new VRedTransactionResponse() { Errors = new List<ErrorDetails> { new ErrorDetails() { Code = "500", Message = "Invalid Transaction Type" } } };

            _mockCustomerProvider.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerDTO);
            _mockCustTxnQueryProvider.Setup(x => x.GetAwardDetails(It.IsAny<string>())).ReturnsAsync(awardDetailsResponse.Response);
            _mockPartnerProfileProvider.Setup(m => m.GetPartnerProfileDetails()).ReturnsAsync(partnerProfilesResponse.Response);
            _mockProductsProvider.Setup(m => m.GetProductDetails()).ReturnsAsync(productResponse.Response);
            _mockMbrMgmtProvider.Setup(k => k.GenerateStatetmentText(It.IsAny<TransactionStatementTextRequest>())).ReturnsAsync(stmtResponse.Response);
            _mockCustTxnMutationProvider.Setup(s => s.CreateTransaction(It.IsAny<TransactionRequest>())).ReturnsAsync(txnCreateResponse.Response);
            _mockVredProvider.Setup(s => s.PostTransactionToVred(It.IsAny<VredSyncTransactionRequest>())).ReturnsAsync(vredCreateResponse);

            //Act
            var result = await _mutation.CreateAccrualTransaction(request);

            //Assert
            Assert.Equal("500", result.Errors?.FirstOrDefault()?.Code);
            Assert.Equal("VRED Service failure - Invalid Transaction Type", result.Errors?.FirstOrDefault()?.Message);
        }
    }
}
