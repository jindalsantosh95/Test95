using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.VoucherMonitoring;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Helper.Caching.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Services.VoucherMonitoring
{
    public sealed class VoucherMonitoringService : IVoucherMonitoringService
    {
        private readonly ICustomerTransactionDataProvider _dataProvider;
        private readonly IReferenceService _referenceService;
        private readonly ILogger<VoucherMonitoringService> _logger;

        public VoucherMonitoringService(ICustomerTransactionDataProvider dataProvider, IReferenceService referenceService, ILogger<VoucherMonitoringService> logger)
        {
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            _referenceService = referenceService ?? throw new ArgumentNullException(nameof(referenceService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<VoucherMonitoringResponse> GetVoucherTransactionMonitoringStatus(VoucherMonitoringRequest request, CancellationToken cancellationToken)
        {
            List<VoucherResult> voucherResults = new List<VoucherResult>();

            try
            {
                if (request == null) throw new ArgumentNullException(nameof(request));

                var result = new VoucherMonitoringResponse
                {
                    CustomerId = request.CustomerId,
                    MemberNumber = request.MemberNumber,
                    Vouchers = voucherResults
                };

                //Fetch Awards and transactions for the given customerId
                var awards = await _dataProvider.GetAwardsAsync(request.CustomerId, cancellationToken);
                var allTransactions = await _dataProvider.GetTransactionsAsync(request.CustomerId, cancellationToken);

                //Fetch products reference data 
                var products = await _referenceService.GetAllProductsAsync();

                if (awards is null) throw new ArgumentNullException(nameof(awards));
                if (allTransactions is null) throw new ArgumentNullException(nameof(allTransactions));
                if (products is null) throw new ArgumentNullException(nameof(products));

                string rdmProductId = products.FirstOrDefault(p => p.ProductName == Constants.Application.MainRedemptionProduct)?.ProductId;

                //calculate threshold time for voucher monitoring based on configured duration
                var durationMinutes = GetVoucherMonitoringDurationMinutes();
                var thresholdUtc = DateTime.UtcNow.AddMinutes(-durationMinutes);

                //Filter transactions to only include those within the monitoring window (recent transactions)
                var recentTransactions = allTransactions.Where(t => t.CreatedOn >= thresholdUtc);

                foreach (var voucherNumber in request.VoucherNumbers)
                {
                    //fetch award
                    var awardDetails = awards.FirstOrDefault(a => a.AwardNumber == voucherNumber);

                    if (awardDetails == null)
                    {
                        voucherResults.Add(new VoucherResult
                        {
                            VoucherNumber = voucherNumber,
                            TransactionProcessingStatus = VoucherTxnProcessingStatus.Failed,
                            TransactionFound = 0,
                            Summary = "Voucher Not Found."
                        });

                        continue;
                    }

                    List<TransactionDocument> relatedTransactions = new List<TransactionDocument>();

                    relatedTransactions = recentTransactions.Where(x => x.AwardId == awardDetails.AwardId && x.TransactionSubType == Constants.Application.TxnSubType).ToList();

                    if (relatedTransactions.Any())
                    {
                        // fetch details to find main redemption txn
                        var pnr = relatedTransactions.Select(x => x.FlightDetails?.ExternalReservationNumber).FirstOrDefault(x => !x.IsNullOrWhiteSpace());
                        int creditValue = relatedTransactions.Where(x => x.TransactionType.Is(Constants.Application.AccrualTransactionType)).Sum(x => x.TransactionValue.IsNullOrWhiteSpace() ? 0 : (int)Convert.ToDouble(x.TransactionValue));
                        var routes = relatedTransactions.Where(x => x.FlightDetails != null).Select(x => x.FlightDetails.OriginAirportcode + x.FlightDetails.DestinationAirport).ToHashSet();

                        //identify main redemption transaction
                        var rdmTxns = recentTransactions
                            .Where(t =>
                                t.TransactionType.Is(Constants.Application.RedemptionTransactionType) &&
                                t.ProductId == rdmProductId &&
                                t.FlightDetails != null &&
                                t.AwardId.IsNullOrWhiteSpace());

                        if (rdmTxns.Any())
                        {
                            var mainRedemptionTxn = rdmTxns
                                .Where(t =>
                                    pnr != null
                                    ? t.FlightDetails.ExternalReservationNumber == pnr
                                    : (t.TransactionValue.IsNullOrWhiteSpace() ? 0 : (int)Convert.ToDouble(t.TransactionValue)) >= creditValue &&
                                    t.FlightDetails.PassengerDetails?.SelectMany(p => p.SegmentDetails)
                                    .Any(s => routes.Contains(s.OriginAirportcode + s.DestinationAirportcode)) == true);

                            if (mainRedemptionTxn.Any())
                            {
                                relatedTransactions.AddRange(mainRedemptionTxn);
                            }
                        }


                        var transactionsInfo = relatedTransactions.Select(txn => (VoucherTransactionInfo)(txn, products.FirstOrDefault(p => p.ProductId == txn.ProductId)?.ProductName));

                        voucherResults.Add(await MapVoucherResult(transactionsInfo, voucherNumber, awardDetails.Status));
                    }
                    else
                    {
                        voucherResults.Add(new VoucherResult
                        {
                            VoucherNumber = voucherNumber,
                            VoucherStatus = awardDetails.Status,
                            TransactionProcessingStatus = VoucherTxnProcessingStatus.Failed,
                            TransactionFound = 0,
                            Summary = "No transactions found for the voucher."
                        });
                    }

                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred in Voucher Monitoring Service.");
                throw;
            }
        }

        private async Task<VoucherResult> MapVoucherResult(IEnumerable<VoucherTransactionInfo> transactionInfos, string voucherNumber, string voucherStatus)
        {
            // Identify the types of transactions present in the voucher result
            bool accrualFound = transactionInfos.Any(t => t.TransactionType.Is(Constants.Application.AccrualTransactionType) && t.ProductName.Is(Constants.Application.VoucherPointsProductName));
            bool zeroPointRedemptionFound = transactionInfos.Any(t => t.TransactionType.Is(Constants.Application.RedemptionTransactionType) && t.Points == 0);
            bool mainRedemptionFound = transactionInfos.Any(t => t.TransactionType.Is(Constants.Application.RedemptionTransactionType) && t.Points > 0);

            var missingTransactions = new List<string>();

            if (!accrualFound)
                missingTransactions.Add("Voucher Accrual Transaction");

            if (!zeroPointRedemptionFound)
                missingTransactions.Add("Zero Point Redemption Transaction");

            if (!mainRedemptionFound)
                missingTransactions.Add("Main Redemption Transaction");

            string txnSummary = missingTransactions.Count == 0
                ? "No Missing Transactions."
                : $"Missing {string.Join(" and ", missingTransactions)}.";

            bool isPnrMissing = transactionInfos.Any(x => x.PNR.IsNullOrWhiteSpace());
            string pnrSummary = isPnrMissing ? "PNR is missing for one or more transactions." : "PNR is present for all transactions.";

            return new VoucherResult
            {
                VoucherNumber = voucherNumber,
                VoucherStatus = voucherStatus,
                TransactionProcessingStatus = missingTransactions.Count == 0 && !isPnrMissing ? VoucherTxnProcessingStatus.Success : VoucherTxnProcessingStatus.PartialSuccess,
                TransactionFound = transactionInfos.Count(),
                Summary = $"Transaction summary: {txnSummary} PNR summary:{pnrSummary}",
                Transactions = transactionInfos
            };
        }

        private static int GetVoucherMonitoringDurationMinutes()
        {
            var configuredValue = ConfigReader.Get(EnvironmentSettings.VoucherMonitoringDurationMinutes);

            return int.TryParse(configuredValue, out var minutes) && minutes > 0 ? minutes : 15;
        }

    }
}
