using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Services
{
    public class TransactionStatementTextService : ITransactionStatementTextService
    {
        private readonly IAzureBlobService _azureBlobService;
        private readonly string _stmtTxtStorageUrl = ConfigReader.Get(Constants.EnvironmentSettings.BlobConnStringForStatementTxt);
        private readonly string _containerNameForStatementText = ConfigReader.Get(Constants.EnvironmentSettings.ContainerNameForStatementTxt);
        private readonly string _blobNameForStatementText = ConfigReader.Get(Constants.EnvironmentSettings.BlobNameForStatementTxt);
        private readonly string _mbrMgtStorageSASKey = ConfigReader.Get(Constants.EnvironmentSettings.LoyaltyMemberMgmtStorageSASKey);

        public TransactionStatementTextService(IAzureBlobService azureBlobService)
        {
            _azureBlobService = azureBlobService ?? throw new ArgumentNullException(nameof(azureBlobService));
        }

        public async Task<string> GetTransactionStatementText(StatementTextRequest request, CancellationToken cancellationToken)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));

            string statementText = string.Empty;
            var replacements = new Dictionary<string, string>
                {
                    { "PartnerCode", request.PartnerCode},
                    { "ProductName", request.ProductName},
                    { "ProductDescription", request.ProductDescription },
                    { "ActivityNumber", request.ActivityNumber },
                    { "PartnerName", request.PartnerName },
                    { "ExternalReservationNumber", request.ExternalReservationNumber },
                    { "Quantity", request.Quantity },
                    { "OriginAirport", request.OriginAirport },
                    { "DestinationAirport", request.DestinationAirport },
                    { "MemberType", request.MemberType },
                    { "MarketingCarrierCode", request.MarketingCarrierCode },
                    { "CabinClass", request.CabinClass},
                    { "VACodeShareFlag", request.VACodeShareFlag },
                    { "MergedMemberNumber",request.MergedMemberNumber },
                    { "MemberNumber", request.MemberNumber },
                    { "ClassCode", request.ClassCode },
                    { "VoucherName", request.VoucherName },
                    { "MemberMediaCode", request.MemberMediaCode },
                    { "StatementText", request.StatementText }
                };

            await _azureBlobService.DownloadBlobAsync(_stmtTxtStorageUrl, _containerNameForStatementText, _blobNameForStatementText, _mbrMgtStorageSASKey, cancellationToken);

            string content = await _azureBlobService.BlobStringAsync(_blobNameForStatementText, cancellationToken);

            StatementTextValues stmtValues = JsonConvert.DeserializeObject<StatementTextValues>(content);

            var transactionSubType = request.TransactionSubType?.ToUpperInvariant() ?? string.Empty;
            var transactionType = request.TransactionType?.ToUpperInvariant() ?? string.Empty;
            var productName = request.ProductName?.ToUpperInvariant() ?? string.Empty;
            var marketingCarrierCode = request.MarketingCarrierCode?.ToUpperInvariant() ?? string.Empty;

            if (transactionSubType.Contains("CANCELLATION"))
            {
                statementText = stmtValues.Cancellation;
            }
            else if (transactionType.Is("ACCRUAL"))
            {
                if (request.VACodeShareFlag == "Y" && request.PartnerCode != request.MarketingCarrierCode && ConfigReader.Get(Constants.EnvironmentSettings.CodeSharePartnerForStatementText).Split(',').Any(x => x.Is(request.MarketingCarrierCode)))
                {
                    statementText = stmtValues.Accrual.FirstOrDefault(x => x.ProductName == $"{marketingCarrierCode}CODESHARE{productName}")?.StatementTextValue;
                }
                else
                {
                    statementText = stmtValues.Accrual.FirstOrDefault(x => x.ProductName == productName)?.StatementTextValue;
                }
            }
            else if (transactionType.Is("REDEMPTION"))
            {
                statementText = stmtValues.Redemption.FirstOrDefault(x => x.ProductName == productName)?.StatementTextValue;
            }

            return !string.IsNullOrEmpty(statementText) ? RegexCompiled.Replace(statementText, @"\{(\w+)\}", match =>
                {
                    var key = match.Groups[1].Value;
                    return replacements.TryGetValue(key, out var value) ? (value ?? string.Empty) : match.Value;
                }) : string.Empty;
        }
    }
}
