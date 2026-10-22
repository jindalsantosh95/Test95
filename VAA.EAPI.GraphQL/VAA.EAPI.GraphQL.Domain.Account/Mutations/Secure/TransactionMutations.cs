using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Mappers;
using VAA.EAPI.GraphQL.Domain.Account.Models.Transaction;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Entities.VRedMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Mutations.Secure
{
    [ExtendObjectType("Mutation")]
    public class TransactionMutations
    {
        private readonly ITransactionMutationProvider _transactionMutationProvider;
        private readonly IVredPointProvider _vredPointProvider;
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly IActivityQueryProvider _activityQueryProvider;
        private readonly IRefDataService<ProductDetails> _productRefService;
        private readonly IRefDataService<PartnerProfileDetails> _partnerProfileService;

        public TransactionMutations(ITransactionMutationProvider transactionMutationProvider, ITokenB2CProvider<string> tokenB2CProvider, IVredPointProvider vredPointProvider, IActivityQueryProvider activityQueryProvider, IRefDataService<ProductDetails> productRefService, IRefDataService<PartnerProfileDetails> partnerProfileService)
        {
            _transactionMutationProvider = transactionMutationProvider ?? throw new ArgumentNullException(nameof(transactionMutationProvider));
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
            _vredPointProvider = vredPointProvider ?? throw new ArgumentNullException(nameof(vredPointProvider));
            _activityQueryProvider = activityQueryProvider ?? throw new ArgumentNullException(nameof(activityQueryProvider));
            _productRefService = productRefService ?? throw new ArgumentNullException(nameof(productRefService));
            _partnerProfileService = partnerProfileService ?? throw new ArgumentNullException(nameof(partnerProfileService));
        }

        [Authorize(Roles = new[] { "Loyalty.Transaction.Create.Burn" })]
        [GraphQLName("loyaltyCreateBurnTransaction")]
        [GraphQLDescription("Creates Burn Transaction")]
        public async Task<TransactionMutationResponse> CreateBurnTransaction(TransactionMutationRequest transactionRequest)
        {
            string memberNumber = string.Empty;

            var jwtPrincipalFeature = AsyncContext.Current.Context.GetJwtPrincipalFeature();
            if (!(jwtPrincipalFeature?.AccessToken.OtherToken.IsNullOrWhiteSpace() ?? true))
            {
                var tokenResponse = await _tokenB2CProvider.ValidateTokenAsync(jwtPrincipalFeature.AccessToken.OtherToken);
                var claim = Json.Deserialize<TokenB2CResponse>(tokenResponse);
                memberNumber = claim?.FcNumber;
            }


            if (memberNumber.IsNullOrEmpty())
                throw new RequestValidationException("Missing Member Claim");

            var vredResponse = await _vredPointProvider.VredPointBalance(memberNumber);

            if (Convert.ToInt32(vredResponse.Usable) >= transactionRequest.Points)
            {

                var productDetailsTask = _productRefService.Query();
                var partnersTask = _partnerProfileService.Query();

                var product = (await productDetailsTask).FirstOrDefault(p => p.ProductCategory == "Manual Adjustment" && p.ProductName.Is(Common.Constants.Application.RetailTherapyProductName));
                var partner = (await partnersTask).FirstOrDefault(x => x.PartnerCode.Equals(Common.Constants.Application.RetailTherapyPartnerCode, StringComparison.OrdinalIgnoreCase));

                var burnTransactionRequest = TransactionMapper.MapToBurnTransactionResuest(transactionRequest, memberNumber, product, partner);

                var transactionResponse = await _transactionMutationProvider.CreateBurnTransaction(burnTransactionRequest);

                return MapToTransactionResponse(transactionResponse, transactionRequest);
            }
            else
                throw new RequestValidationException("Insufficient Point balance");
        }

        [Authorize(Roles = new[] { "Loyalty.Transaction.Create.Earn" })]
        [GraphQLName("loyaltyCreateEarnTransaction")]
        [GraphQLDescription("Creates Earn Transaction")]
        public async Task<TransactionMutationResponse> CreateEarnTransaction(TransactionMutationRequest transactionRequest)
        {
            //Omnevo is a trusted 3rd party sending member number in the request for the Earn journey, they are responsible for making sure transactions are applied to the correct Flying club account
            string memberNumber = transactionRequest.MemberNumber;

            if (memberNumber.IsNullOrEmpty())
                throw new RequestValidationException("Missing Member Number");

            if (transactionRequest.ActivityDate <= DateTime.Today)
            {
                var productDetailsTask = _productRefService.Query();
                var partnersTask = _partnerProfileService.Query();

                var product = (await productDetailsTask).FirstOrDefault(p => p.ProductCategory == "Manual Adjustment" && p.ProductName.Is(Common.Constants.Application.RetailTherapyProductName));
                var partner = (await partnersTask).FirstOrDefault(x => x.PartnerCode.Equals(Common.Constants.Application.RetailTherapyPartnerCode, StringComparison.OrdinalIgnoreCase));

                var earnTransactionRequest = TransactionMapper.MapToEarnTransactionResuest(transactionRequest, memberNumber, product, partner);

                var transactionResponse = await _transactionMutationProvider.CreateEarnTransaction(earnTransactionRequest);

                return MapToTransactionResponse(transactionResponse, transactionRequest);
            }
            else
                throw new RequestValidationException("Invalid Activity Date");
        }

        [Authorize(Roles = new[] { "Loyalty.Transaction.Cancel" })]
        [GraphQLName("loyaltyCancelTransaction")]
        [GraphQLDescription("Cancel Retail Therapy Transaction")]
        public async Task<TransactionCancelResponse> CancelTransaction(CancelTransactionMutation cancelRequest)
        {
            string memberNumber = cancelRequest?.MemberNumber;

            var jwtPrincipalFeature = AsyncContext.Current.Context.GetJwtPrincipalFeature();
            if (!(jwtPrincipalFeature?.AccessToken.OtherToken.IsNullOrWhiteSpace() ?? true))
            {
                var tokenResponse = await _tokenB2CProvider.ValidateTokenAsync(jwtPrincipalFeature.AccessToken.OtherToken);
                var claim = Json.Deserialize<TokenB2CResponse>(tokenResponse);
                memberNumber = claim?.FcNumber;
            }

            if (memberNumber.IsNullOrEmpty())
                throw new RequestValidationException("Missing Member Claim");

            var transaction = (await _activityQueryProvider.GetVredStatements(memberNumber)).Statements.Select(t => t.VredDetail).Where(d => d?.TransactionId == cancelRequest.TransactionId)?.FirstOrDefault();

            if (transaction != null)
            {
                if (transaction.TransactionStatus != Common.Constants.Application.TransactionStatus)
                {
                    var cancelransactionRequest = (CancellationRequest)transaction;

                    cancelransactionRequest.CancellationReason = cancelRequest.CancellationReason;

                    var transactionResponse = await _transactionMutationProvider.CancelTransaction(cancelransactionRequest);

                    return MapCancelResponse(transactionResponse);
                }
                else
                    throw new RequestValidationException("Transaction Already Cancelled");
            }
            else
                throw new RequestValidationException("Invalid Transaction Id");
        }

        private static TransactionMutationResponse GetMockTransactionResponse(TransactionMutationRequest request)
        {
            return new TransactionMutationResponse
            {
                OrderId = request.OrderId,
                VaaTransactionId = "177999557773506945"
            };
        }

        private TransactionMutationResponse MapToTransactionResponse(VredTransactionResponse transactionData, TransactionMutationRequest createRequest)
        {
            if (transactionData.Errors is not null)
            {
                return new TransactionMutationResponse()
                {
                    Errors = transactionData.Errors
                };
            }

            var vreTransactionDetails = Json.Deserialize<VredTransactionSuccessResponse>(transactionData.Value);

            return new TransactionMutationResponse()
            {
                VaaTransactionId = vreTransactionDetails.PartnerTransactionId,
                OrderId = createRequest.OrderId
            };
        }

        private TransactionCancelResponse MapCancelResponse(VredTransactionResponse transactionData)
        {
            if (transactionData.Errors is not null)
            {
                return new TransactionCancelResponse()
                {
                    Errors = transactionData.Errors
                };
            }
            else
            {
                var vreTransactionDetails = Json.Deserialize<VredTransactionSuccessResponse>(transactionData.Value);
                return new TransactionCancelResponse()
                {
                    Message = "Request processed successfully"
                };
            }
        }
    }
}