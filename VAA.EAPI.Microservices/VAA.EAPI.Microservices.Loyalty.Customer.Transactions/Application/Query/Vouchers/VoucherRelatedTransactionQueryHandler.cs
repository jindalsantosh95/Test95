using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Awards;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Vouchers
{
    public class VoucherRelatedTransactionQueryHandler : IVoucherRelatedTransactionQueryHandler
    {
        private readonly TransactionDocumentQueryHandler _transactionDocumentQueryHandler;
        private readonly AwardsQueryHandler _awardsQueryHandler;
        private readonly ILogger<VoucherRelatedTransactionQueryHandler> _logger;

        public VoucherRelatedTransactionQueryHandler(TransactionDocumentQueryHandler transactionDocumentQueryHandler, AwardsQueryHandler awardsQueryHandler, ILogger<VoucherRelatedTransactionQueryHandler> logger)
        {
            _transactionDocumentQueryHandler = transactionDocumentQueryHandler;
            _awardsQueryHandler = awardsQueryHandler;
            _logger = logger;
        }

        public async Task<Result<Response<TransactionResponse>>> GetVoucherRelatedTransactions(VoucherRelatedTransactionQuery query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query.CustomerId))
                    return Result.Failure<Response<TransactionResponse>>("CustomerId is required");

                if (string.IsNullOrWhiteSpace(query.VoucherNumber))
                    return Result.Failure<Response<TransactionResponse>>("VoucherNumber is required");

                var memberTransactions = await _transactionDocumentQueryHandler.Handle(new TransactionQuery { CustomerId = query.CustomerId, DocType = ConfigReader.Get(EnvironmentSettings.TransactionDocType) }, CancellationToken.None);

                if (memberTransactions.IsFailure)
                {
                    return Result.Failure<Response<TransactionResponse>>(memberTransactions.Error);
                }

                var response = memberTransactions.Value;
                var transactionResponse = response?.Value;
                var transactions = transactionResponse?.TransactionDocuments;

                if (transactions == null || !transactions.Any())
                {
                    return Result.Failure<Response<TransactionResponse>>(ErrorCodes.CustomerIdNotFoundMessage);
                }

                var awardId = query.AwardId;

                if (string.IsNullOrEmpty(awardId))
                {
                    var awardsResult = await _awardsQueryHandler.Handle(new AwardsQuery { CustomerId = query.CustomerId, DocType = ConfigReader.Get(EnvironmentSettings.AwardDocType) }, CancellationToken.None);

                    if (awardsResult.IsFailure)
                    {
                        return Result.Failure<Response<TransactionResponse>>("Failed to retrieve awards");
                    }

                    var awards = awardsResult.Value?.Value?.Awards;

                    if (awards == null || !awards.Any())
                    {
                        return Result.Failure<Response<TransactionResponse>>("No awards found");
                    }

                    var voucher = query.VoucherNumber?.Trim();

                    var matchedAward = awards.FirstOrDefault(a => !string.IsNullOrEmpty(a.AwardNumber) && a.AwardNumber.Trim().Equals(voucher, StringComparison.OrdinalIgnoreCase));

                    if (matchedAward == null)
                    {
                        return Result.Failure<Response<TransactionResponse>>("No award found for voucher number");
                    }

                    awardId = matchedAward.AwardId;
                }

                var normalizedAwardId = awardId?.Trim();

                var windowHours = int.Parse(ConfigReader.Get(Constants.EnvironmentSettings.TransactionRecentWindowHours));

                var thresholdTime = DateTime.UtcNow.AddHours(-windowHours);

                var relatedTransactions = transactions.Where(t => !string.IsNullOrEmpty(t.AwardId) && t.AwardId.Trim().Equals(normalizedAwardId, StringComparison.OrdinalIgnoreCase) && t.CreatedOn >= thresholdTime && t.StatusCode == Constants.Application.StatusCode && string.IsNullOrEmpty(t.CancelledTransactionId))
                    .ToList();

                return Result.Success(new Response<TransactionResponse>
                {
                    Status = Status.OK,
                    Value = new TransactionResponse
                    {
                        CustomerId = query.CustomerId,
                        TransactionDocuments = relatedTransactions
                    }
                });
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "VoucherRelatedTransactionQueryHandler failed due to Exception");

                return Result.Failure<Response<TransactionResponse>>("An unexpected error occurred while retrieving voucher related transactions.");
            }
        }
    }
}
