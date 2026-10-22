using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerRetro;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Reference;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.TxnStatementText;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class SkyPartnerRetroMutationTest
    {
        private readonly SkyPartnerRetroMutation _mutation;
        private readonly Mock<ISkyPartnerRetroMutationProvider> _mockSkyPartnerRetroProvider;
        private readonly Mock<IReferenceDataMutationProvider> _mockReferenceDataProvider;
        private readonly Mock<IInRuleMutationProvider> _mockInRuleProvider;
        private readonly Mock<ICustomerProvider> _mockCustomerProvider;
        private readonly Mock<IProductsQueryProvider> _mockProductsQueryProvider;
        private readonly Mock<ICustomerTransactionMutationProvider<TransactionResponse>> _mockTransactionMutationProvider;
        private readonly Mock<IVredMutationProvider> _mockVredMutationProvider;
        private readonly Mock<IPartnersProfilesQueryProvider> _mockPartnersProfilesQueryProvider;
        private readonly Mock<ILoyaltyMemberMgtProvider> _mockMbrMgmtProvider;
        private readonly Mock<ICustomerTransactionQueryProvider> _mockCustomerTransactionQueryProvider;

        public SkyPartnerRetroMutationTest()
        {
            _mockSkyPartnerRetroProvider = new Mock<ISkyPartnerRetroMutationProvider>();
            _mockReferenceDataProvider = new Mock<IReferenceDataMutationProvider>();
            _mockInRuleProvider = new Mock<IInRuleMutationProvider>();
            _mockProductsQueryProvider = new Mock<IProductsQueryProvider>();
            _mockTransactionMutationProvider = new Mock<ICustomerTransactionMutationProvider<TransactionResponse>>();
            _mockVredMutationProvider = new Mock<IVredMutationProvider>();
            _mockPartnersProfilesQueryProvider = new Mock<IPartnersProfilesQueryProvider>();
            _mockMbrMgmtProvider = new Mock<ILoyaltyMemberMgtProvider>();
            _mockCustomerProvider = new Mock<ICustomerProvider>();
            _mockCustomerTransactionQueryProvider = new Mock<ICustomerTransactionQueryProvider>();
            _mutation = new SkyPartnerRetroMutation(_mockCustomerProvider.Object, _mockSkyPartnerRetroProvider.Object, _mockProductsQueryProvider.Object, _mockTransactionMutationProvider.Object, _mockVredMutationProvider.Object, _mockPartnersProfilesQueryProvider.Object, _mockMbrMgmtProvider.Object, _mockReferenceDataProvider.Object, _mockInRuleProvider.Object, _mockCustomerTransactionQueryProvider.Object);
            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SkyPartnerRetroMutation(null, _mockSkyPartnerRetroProvider.Object, _mockProductsQueryProvider.Object, _mockTransactionMutationProvider.Object, _mockVredMutationProvider.Object, _mockPartnersProfilesQueryProvider.Object, _mockMbrMgmtProvider.Object, _mockReferenceDataProvider.Object, _mockInRuleProvider.Object, _mockCustomerTransactionQueryProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new SkyPartnerRetroMutation(_mockCustomerProvider.Object, null, _mockProductsQueryProvider.Object, _mockTransactionMutationProvider.Object, _mockVredMutationProvider.Object, _mockPartnersProfilesQueryProvider.Object, _mockMbrMgmtProvider.Object, _mockReferenceDataProvider.Object, _mockInRuleProvider.Object, _mockCustomerTransactionQueryProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new SkyPartnerRetroMutation(_mockCustomerProvider.Object, _mockSkyPartnerRetroProvider.Object, null, _mockTransactionMutationProvider.Object, _mockVredMutationProvider.Object, _mockPartnersProfilesQueryProvider.Object, _mockMbrMgmtProvider.Object, _mockReferenceDataProvider.Object, _mockInRuleProvider.Object, _mockCustomerTransactionQueryProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new SkyPartnerRetroMutation(_mockCustomerProvider.Object, _mockSkyPartnerRetroProvider.Object, _mockProductsQueryProvider.Object, null, _mockVredMutationProvider.Object, _mockPartnersProfilesQueryProvider.Object, _mockMbrMgmtProvider.Object, _mockReferenceDataProvider.Object, _mockInRuleProvider.Object, _mockCustomerTransactionQueryProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new SkyPartnerRetroMutation(_mockCustomerProvider.Object, _mockSkyPartnerRetroProvider.Object, _mockProductsQueryProvider.Object, _mockTransactionMutationProvider.Object, null, _mockPartnersProfilesQueryProvider.Object, _mockMbrMgmtProvider.Object, _mockReferenceDataProvider.Object, _mockInRuleProvider.Object, _mockCustomerTransactionQueryProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new SkyPartnerRetroMutation(_mockCustomerProvider.Object, _mockSkyPartnerRetroProvider.Object, _mockProductsQueryProvider.Object, _mockTransactionMutationProvider.Object, _mockVredMutationProvider.Object, null, _mockMbrMgmtProvider.Object, _mockReferenceDataProvider.Object, _mockInRuleProvider.Object, _mockCustomerTransactionQueryProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new SkyPartnerRetroMutation(_mockCustomerProvider.Object, _mockSkyPartnerRetroProvider.Object, _mockProductsQueryProvider.Object, _mockTransactionMutationProvider.Object, _mockVredMutationProvider.Object, _mockPartnersProfilesQueryProvider.Object, null, _mockReferenceDataProvider.Object, _mockInRuleProvider.Object, _mockCustomerTransactionQueryProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new SkyPartnerRetroMutation(_mockCustomerProvider.Object, _mockSkyPartnerRetroProvider.Object, _mockProductsQueryProvider.Object, _mockTransactionMutationProvider.Object, _mockVredMutationProvider.Object, _mockPartnersProfilesQueryProvider.Object, _mockMbrMgmtProvider.Object, null, _mockInRuleProvider.Object, _mockCustomerTransactionQueryProvider.Object));
            Assert.Throws<ArgumentNullException>(() => new SkyPartnerRetroMutation(_mockCustomerProvider.Object, _mockSkyPartnerRetroProvider.Object, _mockProductsQueryProvider.Object, _mockTransactionMutationProvider.Object, _mockVredMutationProvider.Object, _mockPartnersProfilesQueryProvider.Object, _mockMbrMgmtProvider.Object, _mockReferenceDataProvider.Object, null, _mockCustomerTransactionQueryProvider.Object));

        }

        [Fact]
        public async Task CreateSkyPartnerRetroTransactionShouldSuccess()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<SkyPartnerRetroRequest>("Loyalty//SkyPartnerRetroRequest");
            request.LoyTransaction.BookingDetails.FlightDetails.FlightActivityDate = DateTime.Now.Date.AddDays(-1);
            var customerDTO = TestDataProvider.GetTestData<CustomerDTO>("Loyalty//CustomerDTO");
            var flyingActivityDetails = TestDataProvider.GetTestData<FlyingActivityDetails>("Loyalty//FlyingActivityDetail");
            var distanceInMiles = "5123";
            var bookingDetails = TestDataProvider.GetTestData<CabinClassDetailsResponse>("Loyalty//BookingDetailResponse");
            var earnedPoints = TestDataProvider.GetTestData<CreditPointsResponse>("Loyalty//CreditPointsResponse");
            var partnerProfilesResponse = TestDataProvider.GetTestData<APIResponse<IEnumerable<PartnerProfileDetails>>>("MemberCardLink//PartnersProfiles_APIResponse");
            var productResponse = TestDataProvider.GetTestData<APIResponse<IEnumerable<ProductDetails>>>("Loyalty//Products_APIResponse");
            var txnCreateResponse = TestDataProvider.GetTestData<APIResponse<TransactionResponse>>("Loyalty//Transaction//TransactionCreation_APIResponse");
            var vredCreateResponse = TestDataProvider.GetTestData<APIResponse<VRedTransactionResponse>>("Loyalty//Transaction//VredSync_APIResponse");
            var stmtResponse = TestDataProvider.GetTestData<APIResponse<TransactionStatementTextResponse>>("Loyalty//TransactionStatementTextAPIResponse");
            var feedback = TestDataProvider.GetTestData<SkyMSSuccessResponse>("Loyalty//SkyMSFeedbackResponse");

            _mockCustomerProvider.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerDTO);
            _mockSkyPartnerRetroProvider.Setup(z => z.GetFlyingActivityDetails(It.IsAny<SkyPartnerMSRequest>())).ReturnsAsync(flyingActivityDetails);
            _mockReferenceDataProvider.Setup(m => m.GetSegmentsDistance(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(distanceInMiles);
            _mockReferenceDataProvider.Setup(m => m.GetCabinClassDetails(It.IsAny<CabinClassDetailsRequest>())).ReturnsAsync(bookingDetails);
            _mockInRuleProvider.Setup(m => m.CalculatePoints(It.IsAny<AirlineEarnedPointsRequest>())).ReturnsAsync(earnedPoints);
            _mockPartnersProfilesQueryProvider.Setup(m => m.GetPartnerProfileDetails()).ReturnsAsync(partnerProfilesResponse.Response);
            _mockProductsQueryProvider.Setup(m => m.GetProductDetails()).ReturnsAsync(productResponse.Response);
            _mockTransactionMutationProvider.Setup(s => s.CreateTransaction(It.IsAny<Models.Transaction.TransactionRequest>())).ReturnsAsync(txnCreateResponse.Response);
            _mockVredMutationProvider.Setup(s => s.PostTransactionToVred(It.IsAny<VredSyncTransactionRequest>())).ReturnsAsync(vredCreateResponse.Response);
            _mockMbrMgmtProvider.Setup(k => k.GenerateStatetmentText(It.IsAny<TransactionStatementTextRequest>())).ReturnsAsync(stmtResponse.Response);
            _mockSkyPartnerRetroProvider.Setup(k => k.RetroCreditFeedback(It.IsAny<SkyMSFeedbackRequest>())).ReturnsAsync(feedback);

            request.LoyTransaction.BookingDetails.FlightDetails.FlightActivityDate = DateTime.Now.AddDays(-32);

            //Act
            var result = await _mutation.CreatePartnerRetroTxn(request);

            //Assert
            Assert.Null(result.Errors);
            Assert.Equal(txnCreateResponse.Response.Transaction.TransactionId, result.TransactionId);
            Assert.Equal(txnCreateResponse.Response.Transaction.CustomerId, result.CustomerId);
        }

        [Fact]
        public async Task SkyPartnerRetroTransactionMutationsValidationFailure()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<SkyPartnerRetroRequest>("Loyalty//SkyPartnerRetroRequest");
            request.CreatedBy = null;

            //Assert
            await Assert.ThrowsAsync<RequestValidationException>(() => _mutation.CreatePartnerRetroTxn(request));
        }

        [Fact]
        public async Task SkyPartnerRetroTransactionMutationsCosmosCreateFailure()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<SkyPartnerRetroRequest>("Loyalty//SkyPartnerRetroRequest");
            request.LoyTransaction.BookingDetails.FlightDetails.FlightActivityDate = DateTime.Now.Date.AddDays(-1);
            var customerDTO = TestDataProvider.GetTestData<CustomerDTO>("Loyalty//CustomerDTO");
            var flyingActivityDetails = TestDataProvider.GetTestData<FlyingActivityDetails>("Loyalty//FlyingActivityDetail");
            var distanceInMiles = "5123";
            var bookingDetails = TestDataProvider.GetTestData<CabinClassDetailsResponse>("Loyalty//BookingDetailResponse");
            var earnedPoints = TestDataProvider.GetTestData<CreditPointsResponse>("Loyalty//CreditPointsResponse");
            var partnerProfilesResponse = TestDataProvider.GetTestData<APIResponse<IEnumerable<PartnerProfileDetails>>>("MemberCardLink//PartnersProfiles_APIResponse");
            var productResponse = TestDataProvider.GetTestData<APIResponse<IEnumerable<ProductDetails>>>("Loyalty//Products_APIResponse");
            var vredCreateResponse = TestDataProvider.GetTestData<APIResponse<VRedTransactionResponse>>("Loyalty//Transaction//VredSync_APIResponse");
            var stmtResponse = TestDataProvider.GetTestData<APIResponse<TransactionStatementTextResponse>>("Loyalty//TransactionStatementTextAPIResponse");
            TransactionResponse txnCreateResponse = new TransactionResponse() { Errors = new List<ErrorDetails> { new ErrorDetails() { Code = "G009", Message = "There was an unexpected error.", DeveloperMessage = "Miscellaneous technical error. | ErrorID: 13519065-348e-4853-a486-c0dd08eed59a | TimeStamp: 6/18/2024 4:08:35 PM", ErrorSource = "Source system" } } };

            _mockCustomerProvider.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerDTO);
            _mockSkyPartnerRetroProvider.Setup(z => z.GetFlyingActivityDetails(It.IsAny<SkyPartnerMSRequest>())).ReturnsAsync(flyingActivityDetails);
            _mockReferenceDataProvider.Setup(m => m.GetSegmentsDistance(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(distanceInMiles);
            _mockReferenceDataProvider.Setup(m => m.GetCabinClassDetails(It.IsAny<CabinClassDetailsRequest>())).ReturnsAsync(bookingDetails);
            _mockInRuleProvider.Setup(m => m.CalculatePoints(It.IsAny<AirlineEarnedPointsRequest>())).ReturnsAsync(earnedPoints);
            _mockPartnersProfilesQueryProvider.Setup(m => m.GetPartnerProfileDetails()).ReturnsAsync(partnerProfilesResponse.Response);
            _mockProductsQueryProvider.Setup(m => m.GetProductDetails()).ReturnsAsync(productResponse.Response);
            _mockTransactionMutationProvider.Setup(s => s.CreateTransaction(It.IsAny<Models.Transaction.TransactionRequest>())).ReturnsAsync(txnCreateResponse);
            _mockVredMutationProvider.Setup(s => s.PostTransactionToVred(It.IsAny<VredSyncTransactionRequest>())).ReturnsAsync(vredCreateResponse.Response);
            _mockMbrMgmtProvider.Setup(k => k.GenerateStatetmentText(It.IsAny<TransactionStatementTextRequest>())).ReturnsAsync(stmtResponse.Response);

            request.LoyTransaction.BookingDetails.FlightDetails.FlightActivityDate = DateTime.Now.AddDays(-32);

            //Act
            var result = await _mutation.CreatePartnerRetroTxn(request);

            //Assert
            Assert.Equal(txnCreateResponse.Errors?.FirstOrDefault()?.Code, result.Errors?.FirstOrDefault()?.Code);
            Assert.Equal(txnCreateResponse.Errors?.FirstOrDefault()?.Message, result.Errors?.FirstOrDefault()?.Message);
        }

        [Fact]
        public async Task SkyPartnerRetroTransactionMutationsVredCreateFailure()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<SkyPartnerRetroRequest>("Loyalty//SkyPartnerRetroRequest");
            request.LoyTransaction.BookingDetails.FlightDetails.FlightActivityDate = DateTime.Now.Date.AddDays(-1);
            var customerDTO = TestDataProvider.GetTestData<CustomerDTO>("Loyalty//CustomerDTO");
            var flyingActivityDetails = TestDataProvider.GetTestData<FlyingActivityDetails>("Loyalty//FlyingActivityDetail");
            var distanceInMiles = "5123";
            var bookingDetails = TestDataProvider.GetTestData<CabinClassDetailsResponse>("Loyalty//BookingDetailResponse");
            var earnedPoints = TestDataProvider.GetTestData<CreditPointsResponse>("Loyalty//CreditPointsResponse");
            var partnerProfilesResponse = TestDataProvider.GetTestData<APIResponse<IEnumerable<PartnerProfileDetails>>>("MemberCardLink//PartnersProfiles_APIResponse");
            var productResponse = TestDataProvider.GetTestData<APIResponse<IEnumerable<ProductDetails>>>("Loyalty//Products_APIResponse");
            var txnCreateResponse = TestDataProvider.GetTestData<APIResponse<TransactionResponse>>("Loyalty//Transaction//TransactionCreation_APIResponse");
            var stmtResponse = TestDataProvider.GetTestData<APIResponse<TransactionStatementTextResponse>>("Loyalty//TransactionStatementTextAPIResponse");
            VRedTransactionResponse vredCreateResponse = new VRedTransactionResponse() { Errors = new List<ErrorDetails> { new ErrorDetails() { Code = "500", Message = "Invalid Transaction Type" } } };

            _mockCustomerProvider.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerDTO);
            _mockSkyPartnerRetroProvider.Setup(z => z.GetFlyingActivityDetails(It.IsAny<SkyPartnerMSRequest>())).ReturnsAsync(flyingActivityDetails);
            _mockReferenceDataProvider.Setup(m => m.GetSegmentsDistance(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(distanceInMiles);
            _mockReferenceDataProvider.Setup(m => m.GetCabinClassDetails(It.IsAny<CabinClassDetailsRequest>())).ReturnsAsync(bookingDetails);
            _mockInRuleProvider.Setup(m => m.CalculatePoints(It.IsAny<AirlineEarnedPointsRequest>())).ReturnsAsync(earnedPoints);
            _mockPartnersProfilesQueryProvider.Setup(m => m.GetPartnerProfileDetails()).ReturnsAsync(partnerProfilesResponse.Response);
            _mockProductsQueryProvider.Setup(m => m.GetProductDetails()).ReturnsAsync(productResponse.Response);
            _mockTransactionMutationProvider.Setup(s => s.CreateTransaction(It.IsAny<Models.Transaction.TransactionRequest>())).ReturnsAsync(txnCreateResponse.Response);
            _mockVredMutationProvider.Setup(s => s.PostTransactionToVred(It.IsAny<VredSyncTransactionRequest>())).ReturnsAsync(vredCreateResponse);
            _mockMbrMgmtProvider.Setup(k => k.GenerateStatetmentText(It.IsAny<TransactionStatementTextRequest>())).ReturnsAsync(stmtResponse.Response);

            request.LoyTransaction.BookingDetails.FlightDetails.FlightActivityDate = DateTime.Now.AddDays(-32);

            //Act
            var result = await _mutation.CreatePartnerRetroTxn(request);

            //Assert
            Assert.Equal(vredCreateResponse.Errors?.FirstOrDefault()?.Code, result.Errors?.FirstOrDefault()?.Code);
            Assert.Equal(vredCreateResponse.Errors?.FirstOrDefault()?.Message, result.Errors?.FirstOrDefault()?.Message);
        }
    }
}
