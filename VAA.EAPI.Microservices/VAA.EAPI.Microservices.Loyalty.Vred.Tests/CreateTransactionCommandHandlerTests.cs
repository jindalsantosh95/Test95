using Moq;
using Xunit;
using System;
using MediatR;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
using VAA.EAPI.Microservices.Loyalty.Vred.Applications.Commands.Transaction;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository.Transaction;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Tests
{
    public class CreateTransactionCommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ITransactionDBRepository> _mockTransactionDBRepository;
        private readonly CreateTransactionCommandHandler _handler;

        public CreateTransactionCommandHandlerTests()
        {
            VRedCommon.CreateTestEnvironmentSettings();
            _mediator = new Mock<IMediator>();
            _mockTransactionDBRepository = new Mock<ITransactionDBRepository>();
            _handler = new CreateTransactionCommandHandler(_mediator.Object, _mockTransactionDBRepository.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CreateTransactionCommandHandler(null,null));
        }

        [Fact]
        public async Task CommandHandler_Redemption_Transaction_Success()
        {
            string requestId = "c71aa07a-fc0d-4dd3-b451-e2f4ab8a8b97";
            CreateTransactionRequest request = new CreateTransactionRequest()
            {
                TransactionId = "AZ1716523988211",
                TransactionChannel = "PAUSER",
                CustomerId = "D971F4C2-1765-84D1-E053-6941D90A4EF2",
                MemberNumber = "1078583299",
                PartnerCode = "DF",
                ProductName = "VAA Duty Free",
                ActivityDate = DateTime.Parse("14-May-2024"),
                Comments = "Test_123",
                TransactionSource = "RetailTherapy",
                CreatedBy = "1078583299",
                Points = 100,
                ProductDescription = "Virgin Atlantic Duty Free Shopping",
                PartnerName = "Duty Free",
                PartnerCategory = "Ground Partner",
                ProductCategory = "Manual Adjustment",
                ActivityType = "Redemption"
            };
            CreateTransactionCommand command = new CreateTransactionCommand()
            {
                Id = requestId,
                Request = request
            };
            VRedTransactionResponse successResponse = TestDataProvider.GetTestData<VRedTransactionResponse>("VRedMS\\Transaction\\BurnVRedTransactionSuccessResponse");
            _mediator.Setup(m => m.Send(It.IsAny<RedemptionVredTransactionRepoQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(successResponse);
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);

            VRedTransactionResponse result = await _handler.Handle(command, System.Threading.CancellationToken.None);

            Assert.NotNull(result.SuccessResponse);
            Assert.Equal("AZ1716523988211", result.SuccessResponse.PartnerTransactionId);
            Assert.Equal("VAA", result.SuccessResponse.PartnerCode);
            Assert.Equal("c71aa07a-fc0d-4dd3-b451-e2f4ab8a8b97", result.SuccessResponse.RequestId);
            Assert.NotEmpty(result.SuccessResponse.Id);
        }

        [Fact]
        public async Task CommandHandler_Error_Invalid_Type()
        {
            string requestId = "c71aa07a-fc0d-4dd3-b451-e2f4ab8a8b97";
            //No ActivityType
            CreateTransactionRequest request = new CreateTransactionRequest()
            {
                TransactionId = "AZ1716523988211",
                TransactionChannel = "PAUSER",
                CustomerId = "D971F4C2-1765-84D1-E053-6941D90A4EF2",
                MemberNumber = "1078583299",
                PartnerCode = "DF",
                ProductName = "VAA Duty Free",
                ActivityDate = DateTime.Parse("14-May-2024"),
                Comments = "Test_123",
                TransactionSource = "RetailTherapy",
                CreatedBy = "1078583299",
                Points = 100,
                ProductDescription = "Virgin Atlantic Duty Free Shopping",
                PartnerName = "Duty Free",
                PartnerCategory = "Ground Partner",
                ProductCategory = "Manual Adjustment"
            };
            CreateTransactionCommand command = new CreateTransactionCommand()
            {
                Id = requestId,
                Request = request
            };
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);

            VRedTransactionResponse result = await _handler.Handle(command, System.Threading.CancellationToken.None);

            Assert.NotNull(result.ErrorResponse);
            Assert.Equal("500", result.ErrorResponse.Code);
            Assert.Equal("Invalid Transaction Type", result.ErrorResponse.Message);
            Assert.Null(result.SuccessResponse);
        }

        [Fact]
        public async Task CommandHandler_Error()
        {
            string requestId = null;
            CreateTransactionRequest request = new CreateTransactionRequest()
            {
                TransactionId = "AZ1716523988211",
                TransactionChannel = "PAUSER",
                CustomerId = "D971F4C2-1765-84D1-E053-6941D90A4EF2",
                MemberNumber = "1078583299",
                PartnerCode = "DF",
                ProductName = "VAA Duty Free",
                ActivityDate = DateTime.Parse("14-May-2024"),
                Comments = "Test_123",
                TransactionSource = "RetailTherapy",
                CreatedBy = "1078583299",
                Points = 100,
                ProductDescription = "Virgin Atlantic Duty Free Shopping",
                PartnerName = "Duty Free",
                PartnerCategory = "Ground Partner",
                ProductCategory = "Manual Adjustment",
                ActivityType = "Accrual"
            };
            CreateTransactionCommand command = new CreateTransactionCommand()
            {
                Id = requestId,
                Request = request
            };

            VRedTransactionResponse errorResponse = TestDataProvider.GetTestData<VRedTransactionResponse>("VRedMS\\Transaction\\EarnVRedTransactionErrorResponse");

            _mediator.Setup(m => m.Send(It.IsAny<AccrualVredTransactionRepoQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(errorResponse);
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);

            VRedTransactionResponse result = await _handler.Handle(command, System.Threading.CancellationToken.None);

            Assert.NotNull(result.ErrorResponse);
            Assert.Equal("400", result.ErrorResponse.Status);
            Assert.Equal("CLIENT_ERROR", result.ErrorResponse.Code);
            Assert.Equal("requestId must not be blank", result.ErrorResponse.Message);
            Assert.Null(result.SuccessResponse);
        }
    }
}