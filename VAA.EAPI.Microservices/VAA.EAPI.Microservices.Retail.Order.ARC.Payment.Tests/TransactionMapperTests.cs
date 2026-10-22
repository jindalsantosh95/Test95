using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Domain;
using VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Infrastructure.Mapper;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Tests
{
    public class TransactionMapperTests
    {
        [Fact]
        public void EvenExchangeReportsResidualAsChangePenalty()
        {
            // Arrange
            ConfigReader.Set("ticketingsystemcode", "932");
            ConfigReader.Set("ARCEnvironmentName", "dev");

            var request = TestDataProvider.GetTestData<TransactionRequest>("ARCPaymentMS\\TransactionRequest");
            request.ARCRequestType = Constants.Enums.ARCRequestType.EvenExchange;
            request.ARCDocument = new ARCDocument { ARCTransactionHistory = [] };
            var orderItem = request.OrderItems.First();
            foreach (var document in request.AccountableDocumentRef)
            {
                document.OriginalIssueDate = document.IssueDate;
                document.OriginalDocumentNumber = document.DocumentNumber;
            }
            orderItem.Services.First().PenaltyRef.ChangePenaltyRefIds = [];
            request.DataList.Penalty.First().Id = "PEN1-CHANGE";
            request.DataList.Penalty.First().MaxPrice.Amount.MonetaryAmount = "100.00";
            orderItem.SalePriceDifference =
            [
                new VAA.EAPI.Domain.Virgin.Models.Common.Price { PriceComponentType = VAA.EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BaseFareEnum, Amount = new VAA.EAPI.Domain.Virgin.Models.Common.CurrencyAmount { MonetaryAmount = "4.00" } },
                new VAA.EAPI.Domain.Virgin.Models.Common.Price { PriceComponentType = VAA.EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.TotalTaxEnum, Amount = new VAA.EAPI.Domain.Virgin.Models.Common.CurrencyAmount { MonetaryAmount = "-118.40" } }
            ];

            // Act
            var transaction = TransactionMapper.MapArcTranctionRequest(request).Transactions.First();
            var taxes = transaction.Record5.SelectMany(record => new[] { record.TAX_ONE, record.TAX_TWO, record.TAX_THREE, record.TAX_FOUR, record.TAX_FIVE, record.TAX_SIX });

            // Assert
            Assert.Equal(11440, Assert.Single(taxes, tax => tax?.TFCC == "CP").TFCA);
        }

        [Fact]
        public void AddCollectReportsPenaltyAndResidualAsChangePenalty()
        {
            // Arrange
            ConfigReader.Set("ticketingsystemcode", "932");
            ConfigReader.Set("ARCEnvironmentName", "dev");

            var request = TestDataProvider.GetTestData<TransactionRequest>("ARCPaymentMS\\TransactionRequest");
            request.ARCRequestType = Constants.Enums.ARCRequestType.AddCollect;
            request.ARCDocument = new ARCDocument { ARCTransactionHistory = [] };
            var orderItem = request.OrderItems.First();
            foreach (var document in request.AccountableDocumentRef)
            {
                document.OriginalIssueDate = document.IssueDate;
                document.OriginalDocumentNumber = document.DocumentNumber;
            }
            orderItem.Services.First().PenaltyRef.ChangePenaltyRefIds = ["PEN1-CHANGE"];
            request.DataList.Penalty.First().Id = "PEN1-CHANGE";
            request.DataList.Penalty.First().MaxPrice.Amount.MonetaryAmount = "100.00";
            orderItem.SalePriceDifference =
            [
                new VAA.EAPI.Domain.Virgin.Models.Common.Price { PriceComponentType = VAA.EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BaseFareEnum, Amount = new VAA.EAPI.Domain.Virgin.Models.Common.CurrencyAmount { MonetaryAmount = "-11.60" } },
                new VAA.EAPI.Domain.Virgin.Models.Common.Price { PriceComponentType = VAA.EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.TotalTaxEnum, Amount = new VAA.EAPI.Domain.Virgin.Models.Common.CurrencyAmount { MonetaryAmount = "0.00" } }
            ];

            // Act
            var transaction = TransactionMapper.MapArcTranctionRequest(request).Transactions.First();
            var taxes = transaction.Record5.SelectMany(record => new[] { record.TAX_ONE, record.TAX_TWO, record.TAX_THREE, record.TAX_FOUR, record.TAX_FIVE, record.TAX_SIX });

            // Assert
            Assert.Equal(11160, Assert.Single(taxes, tax => tax?.TFCC == "CP").TFCA);
        }

        [Fact]
        public void Record4IssueDateUsesThreeLetterMonth()
        {
            var previousCulture = System.Globalization.CultureInfo.CurrentCulture;
            try
            {
                // Arrange
                System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en-IN");
                ConfigReader.Set("ticketingsystemcode", "932");
                ConfigReader.Set("ARCEnvironmentName", "dev");

                var request = TestDataProvider.GetTestData<TransactionRequest>("ARCPaymentMS\\TransactionRequest");
                request.ARCDocument = new ARCDocument { ARCTransactionHistory = [] };
                foreach (var document in request.AccountableDocumentRef)
                {
                    document.OriginalIssueDate = "2026-09-28";
                    document.IssueDate = "2026-09-29";
                    document.OriginalDocumentNumber = document.DocumentNumber;
                }

                // Act
                request.ARCRequestType = Constants.Enums.ARCRequestType.EvenExchange;
                var exchangeTransaction = TransactionMapper.MapArcTranctionRequest(request).Transactions.First();
                request.ARCRequestType = null;
                var ticketTransaction = TransactionMapper.MapArcTranctionRequest(request).Transactions.First();

                // Assert
                Assert.Equal("28Sep26", exchangeTransaction.Record4.OIDT);
                Assert.Equal("29Sep26", ticketTransaction.Record4.OIDT);
            }
            finally
            {
                System.Globalization.CultureInfo.CurrentCulture = previousCulture;
            }
        }

        [Fact]
        public void MapTransactionRequest()
        {
            // Arrange
            ConfigReader.Set("ticketingsystemcode", "932");
            ConfigReader.Set("ARCEnvironmentName", "dev");

            var request = TestDataProvider.GetTestData<TransactionRequest>("ARCPaymentMS\\EMDTransactionRequest");

            // Act
            var response = TransactionMapper.MapArcTranctionRequest(request);

            // Assert
            Assert.NotNull(response);
            Assert.NotNull(response.Transactions);
            Assert.Equal(2, response.Transactions.Count());
            Assert.NotNull(response.Transactions.First().Record2);
            Assert.NotNull(response.Transactions.First().Record4);
            Assert.NotNull(response.Transactions.First().Record5);
            Assert.NotNull(response.Transactions.First().RecordA);
            Assert.NotNull(response.Transactions.First().RecordG);
            Assert.Equal("0177005189", response.Transactions.First().Record2.TDNM);
            Assert.Contains("0177005189", response.Transactions.First().Record4.OITD);
            Assert.Equal("53.94", response.Transactions.First().Record4.TTDS);
            Assert.Equal("R", response.Transactions.First().Record5.First().ARIN);
            Assert.Equal("9322520162219", response.Transactions.First().RecordA.ICWD);
        }
    }
}
