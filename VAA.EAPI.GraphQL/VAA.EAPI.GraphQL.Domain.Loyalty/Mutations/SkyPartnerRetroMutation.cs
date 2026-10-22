using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
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
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerRetro;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Reference;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction;
using VAA.EAPI.GraphQL.Domain.Loyalty.Validators;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Mutations
{
    [ExtendObjectType("Mutation")]
    public class SkyPartnerRetroMutation
    {
        private readonly ISkyPartnerRetroMutationProvider _skyPartnerRetroMutationProvider;
        private readonly IReferenceDataMutationProvider _referenceDataMutationProvider;
        private readonly IInRuleMutationProvider _inRuleMutationProvider;
        private readonly Infrastructure.Interfaces.ICustomerProvider _customerProvider;
        private readonly IProductsQueryProvider _productsQueryProvider;
        private readonly ICustomerTransactionMutationProvider<TransactionResponse> _transactionMutationProvider;
        private readonly IVredMutationProvider _vredMutationProvider;
        private readonly IPartnersProfilesQueryProvider _partnersProfilesQueryProvider;
        private readonly ILoyaltyMemberMgtProvider _mbrMgmtProvider;
        private readonly ICustomerTransactionQueryProvider _customerTransactionQueryProvider;

        public SkyPartnerRetroMutation(Infrastructure.Interfaces.ICustomerProvider customerProvider, ISkyPartnerRetroMutationProvider skyPartnerRetroMutationProvider, IProductsQueryProvider productsQueryProvider, ICustomerTransactionMutationProvider<TransactionResponse> transactionMutationProvider, IVredMutationProvider vredMutationProvider, IPartnersProfilesQueryProvider partnersProfilesQueryProvider, ILoyaltyMemberMgtProvider mbrMgmtProvider, IReferenceDataMutationProvider referenceDataMutationProvider, IInRuleMutationProvider inRuleMutationProvider, ICustomerTransactionQueryProvider customerTransactionQueryProvider)
        {
            _skyPartnerRetroMutationProvider = skyPartnerRetroMutationProvider ?? throw new ArgumentNullException(nameof(skyPartnerRetroMutationProvider));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
            _productsQueryProvider = productsQueryProvider ?? throw new ArgumentNullException(nameof(productsQueryProvider));
            _transactionMutationProvider = transactionMutationProvider ?? throw new ArgumentNullException(nameof(transactionMutationProvider));
            _vredMutationProvider = vredMutationProvider ?? throw new ArgumentNullException(nameof(vredMutationProvider));
            _partnersProfilesQueryProvider = partnersProfilesQueryProvider ?? throw new ArgumentNullException(nameof(partnersProfilesQueryProvider));
            _mbrMgmtProvider = mbrMgmtProvider ?? throw new ArgumentNullException(nameof(mbrMgmtProvider));
            _referenceDataMutationProvider = referenceDataMutationProvider ?? throw new ArgumentNullException(nameof(referenceDataMutationProvider));
            _inRuleMutationProvider = inRuleMutationProvider ?? throw new ArgumentNullException(nameof(inRuleMutationProvider));
            _customerTransactionQueryProvider = customerTransactionQueryProvider;
        }

        [Authorize(Roles = new[] { "Loyalty.SkyPartnerRetro.Create" })]
        [GraphQLName("loyaltyCreatePartnerRetro")]
        [GraphQLDescription("Create partner retro transactions")]
        public async Task<SkyPartnerRetroResponse> CreatePartnerRetroTxn(SkyPartnerRetroRequest partnerRetroRequest)
        {
            var validationResult = new SkyPartnerRetroValidator(HttpMethod.Post).Validate(partnerRetroRequest);
            var customerDetails = new CustomerCustMS();
            CabinClassDetailsResponse cabinClassDetails = new();

            if (!validationResult.IsValid)
                throw new RequestValidationException("SkyPartnerRetro mutation request validation failure.", validationResult.Errors);

            //fetch the customer details based on customer ID/Member Number  
            customerDetails = (await _customerProvider.GetCustomer(partnerRetroRequest.LoyTransaction.CustomerDetails.CustomerId, partnerRetroRequest.LoyTransaction.CustomerDetails.MemberNumber)).Customer;
            partnerRetroRequest.LoyTransaction.CustomerDetails = PartnerRetroMapper.MapCustomerDetailsToRequest(customerDetails, partnerRetroRequest.LoyTransaction.CustomerDetails);

            #region Get Flying activity details from Sky MS
            //Map Sky MS request parameters
            SkyPartnerMSRequest ticketDetailsRequest = PartnerRetroMapper.MapSkyPartnerRetroRequestToTicketDetails(partnerRetroRequest);

            //Get flying activity details
            FlyingActivityDetails flyingActivityDetails = await _skyPartnerRetroMutationProvider.GetFlyingActivityDetails(ticketDetailsRequest);

            //Check for the errors
            if (flyingActivityDetails.Errors is not null && !flyingActivityDetails.IsEligible)
            {
                return PartnerRetroMapper.MapToSkyPartnerRetroResponse(null, flyingActivityDetails.Errors);
            }
            #endregion

            #region Calculate Distance and airport country codes from Reference MS  

            //Get distance between airports & airport country codes
            //string airportsDistanceInMiles = await _referenceDataMutationProvider.GetSegmentsDistance(ticketDetailsRequest.ticketInfo.FlightSegmentOrigin, ticketDetailsRequest.ticketInfo.FlightSegmentDestination);
            AirportDistanceAndCountries airportsDistanceAndCountries = await _referenceDataMutationProvider.GetSegmentDistanceAndCountry(ticketDetailsRequest.ticketInfo.FlightSegmentOrigin, ticketDetailsRequest.ticketInfo.FlightSegmentDestination);


            if (flyingActivityDetails.SkyClaimResponse.CabinClassCode is null || string.IsNullOrEmpty(flyingActivityDetails.SkyClaimResponse.CabinClassCode))
            {
                cabinClassDetails.CabinClass = partnerRetroRequest.LoyTransaction.BookingDetails.FlightDetails.MarketedCabin;
            }

            //cabin class to code
            if (cabinClassDetails.CabinClass is not null)
            {
                cabinClassDetails.CabinClass = cabinClassDetails?.CabinClass switch
                {
                    string s when s.Contains("Business") => "C",
                    string s when s.Contains("Premium") => "W",
                    string s when s.Contains("First") => "F",
                    string s when s.Contains("Economy") => "Y",
                    _ => cabinClassDetails.CabinClass
                };
            }
            #endregion

            #region Calculate points from InRule MS 
            //Map InRule MS request parameters
            AirlineEarnedPointsRequest earnedPointsRequest = PartnerRetroMapper.MapToCalculateEarnedPointsRequest(partnerRetroRequest, flyingActivityDetails, airportsDistanceAndCountries, cabinClassDetails);

            //Calculate Points
            CreditPointsResponse points = await _inRuleMutationProvider.CalculatePoints(earnedPointsRequest);

            //Check for the errors
            if (points.Errors is not null)
            {
                return PartnerRetroMapper.MapToSkyPartnerRetroResponse(null, points.Errors);
            }

            //Point should be greater than zero
            if (Convert.ToInt32(points.SegmentResponses?.FirstOrDefault()?.PointsAward) <= 0)
                throw new RequestValidationException(Constants.Application.TxnNotQualifiedForPoints);
            #endregion


            #region Create Acrual transaction  
            //Fetch product & partner details from reference Ms            
            PartnerProfileDetails partnerDetails = (await _partnersProfilesQueryProvider.GetPartnerProfileDetails()).FirstOrDefault(m => string.Equals(m.PartnerCode.Trim(), flyingActivityDetails?.SkyClaimResponse?.OperatingCarrierCode.Trim(), StringComparison.OrdinalIgnoreCase));
            IEnumerable<ProductDetails> productDetails = await _productsQueryProvider.GetProductDetails();

            //If reference data is missing then throw error
            if (partnerDetails == null || productDetails == null)
                throw new RequestValidationException("Either 'Partner' or 'Product' does not exist in the system");

            //Check If member status is allowed for accrual
            if (!ConfigReader.Get(Common.Constants.Environment.MemberStatusesForAccrual).Split(',').Any(x => x.Is(RegexCompiled.Replace(partnerRetroRequest.LoyTransaction.CustomerDetails.MemberStatus, @"\s+", string.Empty))))
                throw new RequestValidationException("Member status is not valid for creation of accrual transaction");

            //Map Transaction request parameters
            Models.Transaction.TransactionRequest transactionData = PartnerRetroMapper.MapToAccrualTransactionRequest(partnerRetroRequest, flyingActivityDetails, points, productDetails.FirstOrDefault(x => x.ProductName == partnerRetroRequest.LoyTransaction.ProductName)?.RowId, partnerDetails, airportsDistanceAndCountries?.DistanceInMiles, cabinClassDetails?.CabinClass);

            #region Duplicate transaction check 

            //Checking for existing transactions for customer
            var existingTransactions = _customerTransactionQueryProvider.CustomerTransactionQuery($"{ConfigReader.Get(Common.Constants.Environment.CustTransactionMsUrl)}{Common.Constants.Application.GetTransactionEndPoint}", transactionData.TransactionDocument.CustomerId).Result;

            bool duplicateTransactionCheck = existingTransactions != null ? existingTransactions.transactionDocuments.Any(n => n.ProductId == transactionData.TransactionDocument.ProductId && n.StatusCode == transactionData.TransactionDocument.StatusCode && Convert.ToDateTime(n.ActivityDate).Date == Convert.ToDateTime(transactionData.TransactionDocument.ActivityDate).Date && n.FlightDetails.OriginAirportcode == transactionData.TransactionDocument.FlightDetails.OriginAirportcode && n.FlightDetails.DestinationAirport == transactionData.TransactionDocument.FlightDetails.DestinationAirport && n.TransactionType == transactionData.TransactionDocument.TransactionType) : false;

            if (duplicateTransactionCheck)
                throw new RequestValidationException("Duplicate Transaction found");

            #endregion

            //Request to generate statement text
            transactionData.TransactionDocument.StatementText = (await _mbrMgmtProvider.GenerateStatetmentText(TransactionMapper.MapToTransactionStatementRequest(transactionData.TransactionDocument, partnerDetails, productDetails.FirstOrDefault(x => x.RowId == transactionData?.TransactionDocument?.ProductId), null, null))).StatementText;

            //create txn in cosmos 
            var txnResponse = await _transactionMutationProvider.CreateTransaction(transactionData);

            //Check for the errors
            if (txnResponse.Errors != null)
            {
                return PartnerRetroMapper.MapToSkyPartnerRetroResponse(null, txnResponse.Errors);
            }

            if (txnResponse.CommandStatus == Common.Entities.Enums.Status.OK && !string.IsNullOrEmpty(txnResponse?.Transaction?.TransactionId))
            {

                //Create txn in vred upon successful loops response
                var vredResponse = await _vredMutationProvider.PostTransactionToVred(TransactionMapper.MapToVredTransactionRequest(new CreateTransactionMutationRequest(), transactionData?.TransactionDocument, partnerDetails, productDetails.FirstOrDefault(x => x.RowId == transactionData?.TransactionDocument?.ProductId), null, productDetails.FirstOrDefault(x => x.RowId == transactionData?.TransactionDocument?.ProductId)));

                //Check for the errors
                if (vredResponse.Errors != null)
                {
                    return PartnerRetroMapper.MapToSkyPartnerRetroResponse(null, vredResponse.Errors);
                }
            }
            #endregion

            #region Share Response with Sky Team

            string skyPartnerRetroCreditFeedbackEnabled = Environment.GetEnvironmentVariable(Common.Constants.Environment.SkyPartnerRetroCreditFeedbackEnabled) ?? "true";
            if (skyPartnerRetroCreditFeedbackEnabled.Is("true"))
            {
                //Map the Sky MS feedback request parameters
                SkyMSFeedbackRequest feedbackRequest = PartnerRetroMapper.MapToSkyMSFeedbackRequest(flyingActivityDetails, points, airportsDistanceAndCountries?.DistanceInMiles);

                //Get status feedback from Sky MS
                SkyMSSuccessResponse feedback = await _skyPartnerRetroMutationProvider.RetroCreditFeedback(feedbackRequest);

                //Check for the errors
                if (feedback.Errors is not null)
                {
                    return PartnerRetroMapper.MapToSkyPartnerRetroResponse(null, feedback.Errors);
                }
            }

            #endregion

            return PartnerRetroMapper.MapToSkyPartnerRetroResponse(txnResponse.Transaction, null);
        }
    }
}
