using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Mappers;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.MemberCardLink;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.MemberMerge;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerMembership;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Reference;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction;
using VAA.EAPI.GraphQL.Domain.Loyalty.Validators;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Mutations
{
    [ExtendObjectType("Mutation")]
    public class MemberMergeMutations
    {
        private readonly ICustomerTransactionMutationProvider<PartnerMembershipDocumentResponse> _partnerMembershipMutationProvider;
        private readonly IVredCardDetailsQueryProvider _vredCardDetailsQueryProvider;
        private readonly IVredMutationProvider _vredMutationProvider;
        private readonly IPartnersProfilesQueryProvider _partnersProfilesQueryProvider;
        private readonly ICustomerTransactionQueryProvider _customerTransactionQueryProvider;
        private readonly IPartnerMembershipDocQueryProvider _partnerMembershipDocQueryProvider;
        private readonly ICustomerProvider _customerProvider;
        private readonly IProductsQueryProvider _productsQueryProvider;
        private readonly IMemberHouseholdQueryProvider _memberHouseholdQueryProvider;
        private readonly ISupplementarySilverAccountQueryProvider _supplementarySilverAccountQueryProvider;
        private readonly ICustomerTransactionMutationProvider<TransactionResponse> _transactionMutationProvider;
        private readonly IContactNoteMutationProvider<ContactNoteMutationResponse> _notesProvider;
        private readonly ILoyaltyMemberMgtProvider _mbrMgmtProvider;

        public MemberMergeMutations(ICustomerTransactionMutationProvider<PartnerMembershipDocumentResponse> partnerMembershipMutationProvider, ICustomerProvider customerProvider, IPartnersProfilesQueryProvider partnersProfilesQueryProvider, ILogger<MemberMergeMutations> logger, IVredCardDetailsQueryProvider vredCardDetailsQueryProvider, IVredMutationProvider vredMutationProvider, ICustomerTransactionQueryProvider customerTransactionQueryProvider, IPartnerMembershipDocQueryProvider partnerMembershipDocQueryProvider, IProductsQueryProvider productsQueryProvider, IMemberHouseholdQueryProvider memberHouseholdQueryProvider, ISupplementarySilverAccountQueryProvider supplementarySilverAccountQueryProvider, ICustomerTransactionMutationProvider<TransactionResponse> transactionMutationProvider, IContactNoteMutationProvider<ContactNoteMutationResponse> notesProvider, ILoyaltyMemberMgtProvider mbrMgmtProvider)
        {
            _partnerMembershipMutationProvider = partnerMembershipMutationProvider ?? throw new ArgumentNullException(nameof(partnerMembershipMutationProvider));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
            _partnersProfilesQueryProvider = partnersProfilesQueryProvider ?? throw new ArgumentNullException(nameof(partnersProfilesQueryProvider));
            _vredCardDetailsQueryProvider = vredCardDetailsQueryProvider ?? throw new ArgumentNullException(nameof(vredCardDetailsQueryProvider));
            _vredMutationProvider = vredMutationProvider ?? throw new ArgumentNullException(nameof(vredMutationProvider));
            _customerTransactionQueryProvider = customerTransactionQueryProvider ?? throw new ArgumentNullException(nameof(customerTransactionQueryProvider));
            _partnerMembershipDocQueryProvider = partnerMembershipDocQueryProvider ?? throw new ArgumentNullException(nameof(partnerMembershipDocQueryProvider));
            _productsQueryProvider = productsQueryProvider ?? throw new ArgumentNullException(nameof(productsQueryProvider));
            _memberHouseholdQueryProvider = memberHouseholdQueryProvider;
            _supplementarySilverAccountQueryProvider = supplementarySilverAccountQueryProvider;
            _transactionMutationProvider = transactionMutationProvider;
            _notesProvider = notesProvider;
            _mbrMgmtProvider = mbrMgmtProvider;
        }

        [Authorize(Roles = new[] { "Loyalty.MemberMerge" })]
        [GraphQLName("loyaltyMemberMerge")]
        [GraphQLDescription("Member Merge")]
        public async Task<MemberMergeResponse> MutateMemberMerge(MemberMergeMutationRequest memberMergeRequest)
        {
            var customerDetails = new CustomerCustMS();

            if (string.IsNullOrEmpty(memberMergeRequest.MergedCustomerId) && string.IsNullOrEmpty(memberMergeRequest.MergedMemberNumber))
                throw new RequestValidationException("Either 'Customer ID' or 'Member Number' must be specified");

            //fetch the merged member details based on customer ID/Member Number  
            var mergeAccountMemberDetails = (await _customerProvider.GetCustomer(memberMergeRequest.MergedCustomerId, memberMergeRequest.MergedMemberNumber)).Customer;

            memberMergeRequest.MergedCustomerId = mergeAccountMemberDetails.CustomerId;
            memberMergeRequest.MergedMemberNumber = mergeAccountMemberDetails.LoyaltyNumber;

            var validationResult = new MemberMergeValidator().Validate(memberMergeRequest);

            if (!validationResult.IsValid)
                throw new RequestValidationException("Member merge mutation request validation failure.", validationResult.Errors);

            #region check survivor member number is valid or not

            //fetch the survivor member details based on customer ID/Member Number  
            var survivorAccountMemberDetails = (await _customerProvider.GetCustomer(memberMergeRequest.SurvivorCustomerId, memberMergeRequest.SurvivorMemberNumber)).Customer;

            if (survivorAccountMemberDetails.Status != "Active")
            {
                throw new RequestValidationException("The survivor account is not active. Please update the member status to proceed further.");
            }
            #endregion


            #region compare survivor member details matching with merged member details 

            if (mergeAccountMemberDetails.FirstName != survivorAccountMemberDetails.FirstName || mergeAccountMemberDetails.LastName != survivorAccountMemberDetails.LastName || mergeAccountMemberDetails.DateOfBirth != survivorAccountMemberDetails.DateOfBirth || mergeAccountMemberDetails.AddressDetails.FirstOrDefault().PostCode != survivorAccountMemberDetails.AddressDetails.FirstOrDefault().PostCode)
            {
                throw new RequestValidationException("Unable to merge. The name, DOB and postcode have to match on both accounts, please check the details and try again.");
            }
            #endregion


            #region Check If any of the accounts (merged or survivor) is a part of HouseHold or Supplementary silver

            Task<MemberHouseholdResponse> mergedAccTask = _memberHouseholdQueryProvider.Query(mergeAccountMemberDetails.CustomerId);
            Task<MemberHouseholdResponse> survivorAccTask = _memberHouseholdQueryProvider.Query(survivorAccountMemberDetails.CustomerId);
            Task<SupplementarySilverAccountResponse> mergedAccSuppSilverTask = _supplementarySilverAccountQueryProvider.Query(mergeAccountMemberDetails.CustomerId);
            Task<SupplementarySilverAccountResponse> survivorAccSuppSilverTask = _supplementarySilverAccountQueryProvider.Query(survivorAccountMemberDetails.CustomerId);

            MemberHouseholdResponse mergedAccResponse = await mergedAccTask;
            MemberHouseholdResponse survivorAccResponse = await survivorAccTask;
            SupplementarySilverAccountResponse mergedAccSuppSilverResponse = await mergedAccSuppSilverTask;
            SupplementarySilverAccountResponse survivorAccSuppSilverResponse = await survivorAccSuppSilverTask;

            if (mergedAccResponse.HeadOfHousehold != null || mergedAccResponse.HouseholdMembers.Any() ||
                mergedAccSuppSilverResponse.SsaHeadMember?.MemberNumber != null || mergedAccSuppSilverResponse.SsaChildMember?.MemberNumber != null)
            {
                throw new RequestValidationException("The account is part of a HouseHold or Supplementary Silver");
            }

            if (survivorAccResponse.HeadOfHousehold != null || survivorAccResponse.HouseholdMembers.Any() ||
               survivorAccSuppSilverResponse.SsaHeadMember?.MemberNumber != null || survivorAccSuppSilverResponse.SsaChildMember?.MemberNumber != null)
            {
                throw new RequestValidationException("The survivor account is part of a HouseHold or Supplementary Silver");
            }
            #endregion


            #region check active credit cards

            //Retrieve merged account linked cards
            VredCardDetails cardDetails = await _vredCardDetailsQueryProvider.GetVredCardDetails(memberMergeRequest.MergedMemberNumber);
            if (cardDetails.CardLinks.Any(x => x.CancelledDateTime == null || x.CancelledDateTime > DateTime.UtcNow))
            {
                throw new RequestValidationException("There is an active credit card on the account you want to merge. Please take the necessary steps to move the credit card link to the active account before merging the accounts.");
            }
            #endregion


            #region check and update member account Vred balance

            //Retrieve Product and Partner details
            IEnumerable<ProductDetails> productDetails = _productsQueryProvider.GetProductDetails().Result;
            ProductDetails product = productDetails?.FirstOrDefault(x => x.ProductName.Is(Common.Constants.Application.VredPointsTransferProductName));
            var partnerDetails = _partnersProfilesQueryProvider.GetPartnerProfileDetails().Result?.FirstOrDefault(x => x.PartnerCode.Equals(Common.Constants.Application.VSOperatingAirlineCode, StringComparison.OrdinalIgnoreCase));

            int? usablePoints = cardDetails.Balance?.Usable.Parse(0);

            if (usablePoints > 0)
            {
                //Request to generate statement text
                string statementText = (await _mbrMgmtProvider.GenerateStatetmentText(MemberMergeMapper.MapToTransactionStatementTextRequest(memberMergeRequest, product, partnerDetails, Common.Constants.Application.AccrualTransactionType, survivorAccountMemberDetails.MemberAccountType))).StatementText;

                //Map Vred Accrual Transaction request parameters
                var survivorAccVredTransactionRequest = MemberMergeMapper.MapToVredTransactionRequest(memberMergeRequest, product, partnerDetails, usablePoints.ToString(), survivorAccountMemberDetails);
                //Assign statement text
                survivorAccVredTransactionRequest.StatementText = statementText;
                //create balance accrual to survivor account
                var accrualVredResponse = await _vredMutationProvider.PostTransactionToVred(survivorAccVredTransactionRequest);

                //Check for the errors
                if (accrualVredResponse.Errors != null)
                {
                    return MemberMergeMapper.MapToMemberMergeResponse(accrualVredResponse.Errors, memberMergeRequest);
                }

                //Map Transaction request parameters
                TransactionRequest vredBalanceTransactionData = MemberMergeMapper.MapToTransactionRequest(survivorAccVredTransactionRequest, memberMergeRequest, productDetails.FirstOrDefault().RowId, partnerDetails, string.Empty, string.Empty, string.Empty, "0", usablePoints.ToString(), statementText, Common.Constants.Application.AccrualTransactionType);
                //create txn in cosmos upon successful vred sync
                var accrualTxnResponse = await _transactionMutationProvider.CreateTransaction(vredBalanceTransactionData);

                //Check for the errors
                if (accrualTxnResponse.Errors != null)
                {
                    return MemberMergeMapper.MapToMemberMergeResponse(accrualTxnResponse.Errors, memberMergeRequest);
                }

                //Map Vred Redemption Transaction request parameters
                var mergedAccVredTransactionRequest = MemberMergeMapper.MapToVredTransactionRequest(memberMergeRequest, product, partnerDetails, usablePoints.ToString(), null);
                //Assign statement text
                mergedAccVredTransactionRequest.StatementText = statementText;
                //create balance reedemption to merged's account 
                var redeemptionVredResponse = await _vredMutationProvider.PostTransactionToVred(mergedAccVredTransactionRequest);

                //Check for the errors
                if (redeemptionVredResponse.Errors != null)
                {
                    return MemberMergeMapper.MapToMemberMergeResponse(redeemptionVredResponse.Errors, memberMergeRequest);
                }

                //Map Transaction request parameters
                TransactionRequest vredRedeemBalanceTransactionData = MemberMergeMapper.MapToTransactionRequest(mergedAccVredTransactionRequest, memberMergeRequest, productDetails.FirstOrDefault().RowId, partnerDetails, string.Empty, string.Empty, string.Empty, "0", usablePoints.ToString(), statementText, Common.Constants.Application.RedemptionTransactionType);
                //create txn in cosmos upon successful vred sync
                var redemptionTxnResponse = await _transactionMutationProvider.CreateTransaction(vredRedeemBalanceTransactionData);

                //Check for the errors
                if (redemptionTxnResponse.Errors != null)
                {
                    return MemberMergeMapper.MapToMemberMergeResponse(redemptionTxnResponse.Errors, memberMergeRequest);
                }
            }
            #endregion


            #region update the merged account status 

            //Map flying club member request parameters
            var mergedAccUpdateFCMRequest = MemberMergeMapper.MapToFCMRequest(memberMergeRequest, survivorAccountMemberDetails, null, null);
            //update flying club member document for merged account
            var flyingClubMemberResponse = await _customerTransactionQueryProvider.UpdateFlyingClubMember(mergedAccUpdateFCMRequest);

            //Check for the errors
            if (flyingClubMemberResponse.Errors != null)
            {
                return MemberMergeMapper.MapToMemberMergeResponse(flyingClubMemberResponse.Errors, memberMergeRequest);
            }
            #endregion


            #region create and update awards

            //Retrieve awards on merged account
            var mergedAccountAwardsList = await _customerTransactionQueryProvider.GetAwardDetails(memberMergeRequest.MergedCustomerId);
            var awards = mergedAccountAwardsList?.Awards?.Where(x => x.ExpiryDate != null && x.ExpiryDate.Value.Date >= DateTime.UtcNow.Date && (x.Status == AwardStatusEnum.Used.ToString() || x.Status == AwardStatusEnum.PartiallyUsed.ToString() || x.Status == AwardStatusEnum.Available.ToString()));

            foreach (var award in awards)
            {
                award.AwardName = productDetails?.FirstOrDefault(x => x.RowId == award.ProductId)?.ProductName;

                //Create vouchers in survivor account
                var survivorAccAwardRequest = MemberMergeMapper.MapToAwardRequest(memberMergeRequest, survivorAccountMemberDetails, award);
                var createResponse = await _customerTransactionQueryProvider.MutateAward(survivorAccAwardRequest, HttpMethod.Post);

                //Check for the errors
                if (createResponse.Errors != null)
                {
                    return MemberMergeMapper.MapToMemberMergeResponse(createResponse.Errors, memberMergeRequest);
                }

                //Update vouchers in merged account
                var mergedAccAwardRequest = MemberMergeMapper.MapToAwardRequest(memberMergeRequest, mergeAccountMemberDetails, award);
                var updateResponse = await _customerTransactionQueryProvider.MutateAward(mergedAccAwardRequest, HttpMethod.Patch);

                //Check for the errors
                if (updateResponse.Errors != null)
                {
                    return MemberMergeMapper.MapToMemberMergeResponse(updateResponse.Errors, memberMergeRequest);
                }
            }
            #endregion


            #region create VS and Non-VS tier points transactions

            //Retrieve transactions on merged account
            IEnumerable<Transaction> transactions = _customerTransactionQueryProvider.CustomerTransactionQuery($"{ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl)}{Common.Constants.Application.GetTransactionEndPoint}", memberMergeRequest.MergedCustomerId)?.Result.transactionDocuments;

            if (transactions != null && transactions.Any())
            {
                //Update status for merged account
                var mergedAccflyingClubMemberDoc = await _customerTransactionQueryProvider.GetFlyingClubMember(mergeAccountMemberDetails.CustomerId);

                #region update miles for survivor acc in flying club member document                    
                string totalVSFlownMiles = mergedAccflyingClubMemberDoc?.FlyingClubMemberDocuments.FirstOrDefault()?.MemberAttributes?.Where(x => x.AttributeName.ToUpper() == Common.Constants.Application.VSFlownMiles.ToUpper())?.FirstOrDefault()?.Value;

                if (!string.IsNullOrEmpty(totalVSFlownMiles))
                {
                    //Fetch survivor acc flying club document
                    var survivorAccflyingClubMemberDoc = await _customerTransactionQueryProvider.GetFlyingClubMember(survivorAccountMemberDetails.CustomerId);
                    //Map flying club member request parameters
                    var survivorAccUpdateFCMRequest = MemberMergeMapper.MapToFCMRequest(memberMergeRequest, survivorAccountMemberDetails, survivorAccflyingClubMemberDoc?.FlyingClubMemberDocuments.FirstOrDefault(), totalVSFlownMiles);
                    //update flying club member document for survivor account
                    var survivorAccflyingClubMemberResponse = await _customerTransactionQueryProvider.UpdateFlyingClubMember(survivorAccUpdateFCMRequest);

                    //Check for the errors
                    if (survivorAccflyingClubMemberResponse.Errors != null)
                    {
                        return MemberMergeMapper.MapToMemberMergeResponse(survivorAccflyingClubMemberResponse.Errors, memberMergeRequest);
                    }
                }
                #endregion

                #region accrual transaction 
                ProductDetails productDetailsTierPointsMerge = productDetails?.FirstOrDefault(x => x.ProductName.Is(Common.Constants.Application.TierPointsTransferProductName));
                //Request to generate statement text
                string StatementText = (await _mbrMgmtProvider.GenerateStatetmentText(MemberMergeMapper.MapToTransactionStatementTextRequest(memberMergeRequest, productDetailsTierPointsMerge, partnerDetails, Common.Constants.Application.AccrualTransactionType, survivorAccountMemberDetails.MemberAccountType))).StatementText;
                //Calculate VS tier points
                string totalVSTierPoints = MemberMergeMapper.CalculateTierPointsInLastOneYear(transactions, Common.Constants.Application.VSTierPoints);
                //Calculate non-VS tier points
                string totalNonVSTierPoints = MemberMergeMapper.CalculateTierPointsInLastOneYear(transactions, Common.Constants.Application.TierPointsType);
                //Map Transaction request parameters
                TransactionRequest transactionData = MemberMergeMapper.MapToTransactionRequest(null, memberMergeRequest, productDetailsTierPointsMerge.RowId, partnerDetails, totalVSFlownMiles, totalVSTierPoints, totalNonVSTierPoints, totalNonVSTierPoints, "0", StatementText, Common.Constants.Application.AccrualTransactionType);
                //create accrual for tier points and miles to survivor account
                var transactionResponse = await _transactionMutationProvider.CreateTransaction(transactionData);

                //Check for the errors
                if (transactionResponse.Errors != null)
                {
                    return MemberMergeMapper.MapToMemberMergeResponse(transactionResponse.Errors, memberMergeRequest);
                }
                #endregion
            }

            #endregion


            #region check centurion card links

            //Retrieve centurion card links on merged account
            var centurionCards = _partnerMembershipDocQueryProvider.GetPartnerMembershipDoc(memberMergeRequest.MergedCustomerId)?.Result?.PartnerMembershipDocuments.Where(x => (x.EndDate == null || x.EndDate.Value.Date >= DateTime.UtcNow.Date) && !string.IsNullOrEmpty(x.PartnerId));
            foreach (var card in centurionCards)
            {
                //Enable centurion card for survivor account
                var createCenturionCardRequest = MemberMergeMapper.MapToCenturionCardRequest(memberMergeRequest, survivorAccountMemberDetails, card, HttpMethod.Post);
                var createResponse = await _partnerMembershipMutationProvider.Mutate(createCenturionCardRequest);

                //Check for the errors
                if (createResponse.Errors != null)
                {
                    return MemberMergeMapper.MapToMemberMergeResponse(createResponse.Errors, memberMergeRequest);
                }

                //Change centurion card enddate for merged account
                var updateCenturionCardRequest = MemberMergeMapper.MapToCenturionCardRequest(memberMergeRequest, mergeAccountMemberDetails, card, HttpMethod.Patch);
                var updateResponse = await _partnerMembershipMutationProvider.Mutate(updateCenturionCardRequest);

                //Check for the errors
                if (updateResponse.Errors != null)
                {
                    return MemberMergeMapper.MapToMemberMergeResponse(updateResponse.Errors, memberMergeRequest);
                }
            }
            #endregion


            #region Create contact note in survivor account
            var outcome = await _notesProvider.Mutate((ContactNoteMutationRequest)memberMergeRequest);

            //Check for the errors
            if (outcome.Errors is not null)
            {
                return MemberMergeMapper.MapToMemberMergeResponse(outcome.Errors, memberMergeRequest);
            }
            #endregion


            return MemberMergeMapper.MapToMemberMergeResponse(null, memberMergeRequest);
        }
    }
}