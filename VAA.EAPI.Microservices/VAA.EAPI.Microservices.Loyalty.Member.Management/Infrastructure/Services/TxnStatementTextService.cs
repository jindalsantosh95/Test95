using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.TransactionStatement;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services
{
    public class TxnStatementTextService : ITxnStatementTextService
    {
        private readonly IBlobStorageServices _blobStorageServices;

        public TxnStatementTextService(IBlobStorageServices blobStorageServices)
        {
            _blobStorageServices = blobStorageServices ?? throw new ArgumentNullException(nameof(blobStorageServices));
        }

        public async Task<StatementTextResponse> GetTransactionStatementText(TransactionStatementTextQuery request)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));

            string statementText = string.Empty;
            var replacements = new Dictionary<string, string>
                {
                    { "PartnerCode", request.StatementDescriptionRequest.PartnerCode},
                    { "ProductName", request.StatementDescriptionRequest.ProductName},
                    { "ProductDescription", request.StatementDescriptionRequest.ProductDescription },
                    { "ActivityNumber", request.StatementDescriptionRequest.ActivityNumber },
                    { "PartnerName", request.StatementDescriptionRequest.PartnerName },
                    { "ExternalReservationNumber", request.StatementDescriptionRequest.ExternalReservationNumber },
                    { "Quantity", request.StatementDescriptionRequest.Quantity },
                    { "OriginAirport", request.StatementDescriptionRequest.OriginAirport },
                    { "DestinationAirport", request.StatementDescriptionRequest.DestinationAirport },
                    { "MemberType", request.StatementDescriptionRequest.MemberType },
                    { "MarketingCarrierCode", request.StatementDescriptionRequest.MarketingCarrierCode },
                    { "CabinClass", request.StatementDescriptionRequest.CabinClass},
                    { "VACodeShareFlag", request.StatementDescriptionRequest.VACodeShareFlag },
                    { "MergedMemberNumber",request.StatementDescriptionRequest.MergedMemberNumber },
                    { "MemberNumber", request.StatementDescriptionRequest.MemberNumber },
                    { "ClassCode", request.StatementDescriptionRequest.ClassCode },
                    { "VoucherName", request.StatementDescriptionRequest.VoucherName },
                    { "MemberMediaCode", request.StatementDescriptionRequest.MemberMediaCode },
                    { "StatementText", request.StatementDescriptionRequest.StatementText }
                };


            StatementTextValues stmtValues = await _blobStorageServices.Deserialize<StatementTextValues>();

            if (request.StatementDescriptionRequest.TransactionSubType.ToUpper().Contains("CANCELLATION"))
            {
                statementText = stmtValues.Cancellation;
            }
            else if (request.StatementDescriptionRequest.TransactionType.ToUpper() == "ACCRUAL")
            {
                if (request.StatementDescriptionRequest.VACodeShareFlag == "Y" && request.StatementDescriptionRequest.PartnerCode != request.StatementDescriptionRequest.MarketingCarrierCode && ConfigReader.Get(Constants.Environment.CodeSharePartnerForStatementText).Split(',').Any(x => x.Is(request.StatementDescriptionRequest.MarketingCarrierCode)))
                {
                    statementText = stmtValues.Accrual.FirstOrDefault(x => x.ProductName == $"{request.StatementDescriptionRequest.MarketingCarrierCode.ToUpper()}CODESHARE{request.StatementDescriptionRequest.ProductName.ToUpper()}")?.StatementTextValue;
                }
                else
                {
                    statementText = stmtValues.Accrual.FirstOrDefault(x => x.ProductName == request.StatementDescriptionRequest.ProductName.ToUpper())?.StatementTextValue;
                }
            }
            else if (request.StatementDescriptionRequest.TransactionType.ToUpper() == "REDEMPTION")
            {
                statementText = stmtValues.Redemption.FirstOrDefault(x => x.ProductName == request.StatementDescriptionRequest.ProductName.ToUpper())?.StatementTextValue;
            }




            return new()
            {
                StatementText = !string.IsNullOrEmpty(statementText) ? Regex.Replace(statementText, @"\{(\w+)\}", match =>
                {
                    string key = match.Groups[1].Value;
                    return replacements.ContainsKey(key) ? replacements[key] : match.Value;
                }) : string.Empty
            };

        }
    }
}
