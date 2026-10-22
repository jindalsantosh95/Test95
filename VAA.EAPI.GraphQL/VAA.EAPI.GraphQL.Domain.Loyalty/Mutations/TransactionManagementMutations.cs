using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Mappers;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Reference;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction;
using VAA.EAPI.GraphQL.Domain.Loyalty.Validators;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Mutations
{
    [ExtendObjectType("Mutation")]
    public class TransactionManagementMutations
    {
        private readonly ICustomerTransactionMutationProvider<TransactionResponse> _transactionMutationProvider;
        private readonly ICustomerTransactionQueryProvider _transactionQueryProvider;
        private readonly IPartnersProfilesQueryProvider _partnersProfilesQueryProvider;
        private readonly IProductsQueryProvider _productsQueryProvider;
        private readonly ICustomerProvider _customerProvider;
        private readonly IVredMutationProvider _vredMutationProvider;
        private readonly ILoyaltyMemberMgtProvider _mbrMgmtProvider;
        private readonly ILogger<TransactionManagementMutations> _logger;
        private readonly IVredCardDetailsQueryProvider _vredQueryProvider;

        public TransactionManagementMutations(ICustomerProvider customerProvider, ICustomerTransactionMutationProvider<TransactionResponse> transactionMutationProvider, ICustomerTransactionQueryProvider transactionQueryProvider, IPartnersProfilesQueryProvider partnersProfilesQueryProvider, IProductsQueryProvider productsQueryProvider, IVredMutationProvider vredMutationProvider, ILoyaltyMemberMgtProvider mbrMgmtProvider, ILogger<TransactionManagementMutations> logger, IVredCardDetailsQueryProvider vredQueryProvider)
        {
            _transactionMutationProvider = transactionMutationProvider ?? throw new ArgumentNullException(nameof(transactionMutationProvider));
            _partnersProfilesQueryProvider = partnersProfilesQueryProvider ?? throw new ArgumentNullException(nameof(partnersProfilesQueryProvider));
            _productsQueryProvider = productsQueryProvider ?? throw new ArgumentNullException(nameof(productsQueryProvider));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
            _transactionQueryProvider = transactionQueryProvider ?? throw new ArgumentNullException(nameof(transactionQueryProvider));
            _vredMutationProvider = vredMutationProvider ?? throw new ArgumentNullException(nameof(vredMutationProvider));
            _mbrMgmtProvider = mbrMgmtProvider ?? throw new ArgumentNullException(nameof(mbrMgmtProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _vredQueryProvider = vredQueryProvider ?? throw new ArgumentNullException(nameof(vredQueryProvider));
        }

        [Authorize(Roles = new[] { "Loyalty.Transaction.Create.Earn" })]
        [GraphQLName("loyaltyCreateAccrual")]
        [GraphQLDescription("Create accrual transactions")]
        public async Task<CreateTransactionMutationResponse> CreateAccrualTransaction(CreateTransactionMutationRequest transactionRequest)
        {
            var validationResult = new CreateAccrualTransactionValidator(HttpMethod.Post).Validate(transactionRequest);

            if (!validationResult.IsValid)
                throw new RequestValidationException("Create Accrual Transaction mutation request validation failure.", validationResult.Errors);

            // If customer ID/member number/member status is null in request, then fetch it from customer MS
            if (string.IsNullOrEmpty(transactionRequest.CustomerId) || string.IsNullOrEmpty(transactionRequest.MemberNumber) || string.IsNullOrEmpty(transactionRequest.MemberStatus))
            {
                CustomerCustMS customerResponse = (await _customerProvider.GetCustomer(transactionRequest.CustomerId, transactionRequest.MemberNumber)).Customer;

                transactionRequest.CustomerId = customerResponse.CustomerId;
                transactionRequest.MemberNumber = customerResponse.LoyaltyNumber;
                transactionRequest.MemberStatus = customerResponse.Status;
            }

            Models.Award awardDetails = new();

            if (transactionRequest.VoucherDetails?.VoucherNumber != null)
            {
                awardDetails = (await _transactionQueryProvider.GetAwardDetails(transactionRequest.CustomerId)).Awards.FirstOrDefault(x => x.AwardNumber == transactionRequest.VoucherDetails.VoucherNumber);

                if (awardDetails is null)
                    throw new RequestValidationException("Voucher does not exist in the Member account");

                transactionRequest.VoucherDetails.VoucherId = transactionRequest.VoucherDetails.VoucherId.IsNullOrEmpty() ? awardDetails.AwardId : transactionRequest.VoucherDetails.VoucherId;
                transactionRequest.VoucherDetails.VoucherName = transactionRequest.VoucherDetails.VoucherName.IsNullOrEmpty() ? awardDetails.AwardName : transactionRequest.VoucherDetails.VoucherName;
            }

            //Fecth product & partner id's from reference Ms
            PartnerProfileDetails partnerDetails = (await _partnersProfilesQueryProvider.GetPartnerProfileDetails()).FirstOrDefault(m => m.PartnerCode.ToLower() == transactionRequest.PartnerCode.ToLower());
            IEnumerable<ProductDetails> productDetails = await _productsQueryProvider.GetProductDetails();

            //If reference data is missing then throw error
            if (partnerDetails == null || productDetails == null)
                throw new RequestValidationException("Either 'Partner' or 'Product' does not exist in the system");

            //Check If memberstatus is allowed for accrual
            if (!ConfigReader.Get(Common.Constants.Environment.MemberStatusesForAccrual).Split(',').Any(x => x.Is(RegexCompiled.Replace(transactionRequest.MemberStatus, @"\s+", string.Empty))))
                throw new RequestValidationException("Member status is not valid for creation of accrual transaction");

            Models.Transaction.TransactionRequest transactionData = TransactionMapper.MapToVchrAccrualTransactionRequest(transactionRequest, productDetails.FirstOrDefault(x => x.ProductName == transactionRequest.ProductName)?.RowId, partnerDetails.RowId);
            transactionData.TransactionDocument.StatementText = (await _mbrMgmtProvider.GenerateStatetmentText(TransactionMapper.MapToTransactionStatementRequest(transactionData.TransactionDocument, partnerDetails, productDetails.FirstOrDefault(x => x.RowId == transactionData?.TransactionDocument?.ProductId), null, null))).StatementText;

            //create txn in cosmos before vred sync
            var txnResponse = await _transactionMutationProvider.CreateTransaction(transactionData);

            if (txnResponse.Errors != null)
            {
                return TransactionMapper.MapToTransactionResponse(txnResponse.Transaction, txnResponse.Errors, "LOOPS Service");
            }

            if (txnResponse.CommandStatus == Common.Entities.Enums.Status.OK && !string.IsNullOrEmpty(txnResponse?.Transaction?.TransactionId))
            {
                //create txn in vred upon successful loops resposne
                var vredResponse = await _vredMutationProvider.PostTransactionToVred(TransactionMapper.MapToVredTransactionRequest(transactionRequest, transactionData?.TransactionDocument, partnerDetails, productDetails.FirstOrDefault(x => x.RowId == transactionData?.TransactionDocument?.ProductId), awardDetails, productDetails.FirstOrDefault(x => x.RowId == awardDetails?.ProductId)));

                if (vredResponse.Errors != null)
                {
                    return TransactionMapper.MapToTransactionResponse(null, vredResponse.Errors, "VRED Service");
                }
            }
            return TransactionMapper.MapToTransactionResponse(txnResponse.Transaction, txnResponse.Errors, null);

        }

        [Authorize(Roles = new[] { "Loyalty.Transaction.Cancel" })]
        [GraphQLName("loyaltyTransactionsCancellation")]
        [GraphQLDescription("Cancel Accrual/Redemption transactions")]
        public async Task<string> CancelTransaction(CancelTransactionMutationRequest cancelTransactionRequest)
        {
            var validationResult = new CancelTransactionValidator().Validate(cancelTransactionRequest);

            if (!validationResult.IsValid)
                throw new RequestValidationException("Cancel Transaction mutation request validation failure.", validationResult.Errors);

            // If customer id or member number is null in request, then fetch it from customer MS
            if (string.IsNullOrEmpty(cancelTransactionRequest.CustomerId) || string.IsNullOrEmpty(cancelTransactionRequest.MemberNumber))
            {
                CustomerCustMS customerResponse = (await _customerProvider.GetCustomer(cancelTransactionRequest.CustomerId, cancelTransactionRequest.MemberNumber)).Customer;

                cancelTransactionRequest.CustomerId = customerResponse.CustomerId;
                cancelTransactionRequest.MemberNumber = customerResponse.LoyaltyNumber;
            }

            int cancelledTxnCount = cancelTransactionRequest.CancelTransactionDetails.Count();
            List<string> failedTxnIds = new List<string>();

            //get all trnsactions to handle tier point cancellation for ground partner
            var custTxns = (await _transactionQueryProvider.CustomerTransactionQuery($"{ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl)}{Common.Constants.Application.GetTransactionEndPoint}", cancelTransactionRequest.CustomerId)).transactionDocuments;
            var vredTxns = (await _vredQueryProvider.GetVredStatements(cancelTransactionRequest.MemberNumber)).Statements;

            //Fetch customer awards
            IEnumerable<Award> custAwards = (await _transactionQueryProvider.GetAwardDetails(cancelTransactionRequest.CustomerId)).Awards;

            await Parallel.ForEachAsync(cancelTransactionRequest.CancelTransactionDetails, async (txnDetail, CancellationToken) =>
            {
                try
                {
                    if ((string.IsNullOrEmpty(txnDetail.PartnerId) && string.IsNullOrEmpty(txnDetail.PartnerName)) || (string.IsNullOrEmpty(txnDetail.ProductId) && string.IsNullOrEmpty(txnDetail.ProductName)))
                        throw new RequestValidationException("Either 'Partner' or 'Product' is missing in the request");

                    PartnerProfileDetails partnerDetails = (await _partnersProfilesQueryProvider.GetPartnerProfileDetails()).FirstOrDefault(m => m.RowId == txnDetail.PartnerId || m.Name == txnDetail.PartnerName);
                    IEnumerable<ProductDetails> productDetails = await _productsQueryProvider.GetProductDetails();

                    VRedTransactionResponse vredResponse = new();
                    TransactionResponse cancelResponse = new();
                    bool txnExistsInLoops = custTxns.Any(x => x.TransactionId == txnDetail.TransactionId);
                    bool txnExistsInVred = vredTxns.Any(x => x.VredDetail?.TransactionId == txnDetail.TransactionId);

                    if (!Constants.Application.VredPartnerType.Contains(partnerDetails.PartnerType) || txnExistsInLoops)
                    {
                        string canxReason = string.Empty;

                        //cancellation reason based on type
                        if (Constants.Application.AutoCanxReasons.Contains(cancelTransactionRequest.CancellationReason))
                            canxReason = cancelTransactionRequest.CancellationReason;
                        else
                            canxReason = custTxns.First(x => x.TransactionId == cancelTransactionRequest.InitiatedTransactionId).TransactionType.Is(txnDetail.TransctionType) ? cancelTransactionRequest.CancellationReason : (txnDetail.TransctionType.Is(Common.Constants.Application.AccrualTransactionType) ? Constants.Application.DefaultAccrualCanxReason : Constants.Application.DefaultRedemptionCanxReason);

                        //Fetch award detail
                        Award? awardDetails = custAwards.FirstOrDefault(x => x.AwardId == custTxns.First(x => x.TransactionId == txnDetail.TransactionId).AwardId);

                        cancelResponse = await _transactionMutationProvider.CancelTransaction(TransactionMapper.MapToCancelTransactionRequest(txnDetail, partnerDetails.Name, cancelTransactionRequest, productDetails, canxReason, awardDetails));

                        if (cancelResponse.CommandStatus == Common.Entities.Enums.Status.OK && !string.IsNullOrEmpty(cancelResponse.Transaction.TransactionId))
                        {
                            bool compensatoryTxnExists = false;

                            for (int retry = 1; retry <= ConfigReader.Get<int>(Common.Constants.Environment.ValidateTxnRetryLimit) && !compensatoryTxnExists; retry++)
                            {
                                await Task.Delay(ConfigReader.Get<int>(Common.Constants.Environment.ValidateTxnDelayInMS), CancellationToken.None);
                                compensatoryTxnExists = (await _transactionQueryProvider.CustomerTransactionQuery($"{ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl)}{Common.Constants.Application.GetTransactionEndPoint}", cancelTransactionRequest.CustomerId)).transactionDocuments.Any(x => x.TransactionId == cancelResponse.Transaction.TransactionId);
                            }
                            
                            if (!compensatoryTxnExists)
                            {
                                cancelledTxnCount--;
                                failedTxnIds.Add(txnDetail.TransactionId);
                            }

                            if (compensatoryTxnExists && txnExistsInVred)
                            {
                                vredResponse = await _vredMutationProvider.CancelTransactionVred(TransactionMapper.MapToVredCancelRequest(cancelTransactionRequest, txnDetail, cancelResponse.Transaction.TransactionId, canxReason));
                            }
                        }
                    }
                    else
                    {
                        vredResponse = await _vredMutationProvider.CancelTransactionVred(TransactionMapper.MapToVredCancelRequest(cancelTransactionRequest, txnDetail, string.Empty, cancelTransactionRequest.CancellationReason));
                    }

                    if (vredResponse.Errors != null || cancelResponse.Errors != null)
                    {
                        //update counter and capture txn id's
                        cancelledTxnCount--;
                        failedTxnIds.Add(txnDetail.TransactionId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Exception occured in transaction cancellation mutation for transactionid {txnDetail.TransactionId}");

                    //update counter and capture txn id's
                    cancelledTxnCount--;
                    failedTxnIds.Add(txnDetail.TransactionId);
                }
            });

            return cancelledTxnCount < cancelTransactionRequest.CancelTransactionDetails.Count() ? $"{Constants.Application.CancelltionFailureMsg} - {string.Join("','", failedTxnIds)}" : Constants.Application.CancelltionSuccessMsg;
        }
    }
}
