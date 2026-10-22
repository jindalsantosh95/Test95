using Newtonsoft.Json;
using VAA.EAPI.Automation.Book.Constants;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Book.Tests.NewBookFlow.FrontEnd;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Constants;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.UI.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.Search.Helpers;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.WebBFF.Confirmation
{
    [Collection("Bff_E2E_extensive")]
    public class ConfirmationPageValidations : IClassFixture<TokenFixture>, IClassFixture<ReportFixture>, IClassFixture<CosmosFixture>
    {
        private readonly TokenFixture tokenFixture;
        public string deltaHPPUrl = "https://delta-hpp.webdev.vholsinternal.co.uk/hpp?paymentReferenceId=";

        private readonly ITestOutputHelper output;
        private readonly CosmosFixture cosmosFixture;
        private static string docTypeCustomerSearch = "CustomerSearch";
        private static string docTypeCustomer = "Customer";
        private static string docTypeCustomerProfile = "CustomerProfile";
        private static string docTypeMembershipChange = "MembershipChange";
        private static string docTypePartnerMemberShip = "PartnerMembership";
        private static string docTypeFlyingClubMember = "FlyingClubMember";

        public ConfirmationPageValidations(TokenFixture tokenFixture, ReportFixture reportFixture, CosmosFixture cosmosFixture)
        {
            this.tokenFixture = tokenFixture;
            this.cosmosFixture = cosmosFixture;
            var cosmosEndpoint = "https://avadeuncmdbcustomer.documents.azure.com:443/";
            var cosmosEndpointForCustomerTrx = "https://avadeuncmdbcustomertransactions.documents.azure.com:443/";

            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<CustomerSearch>(cosmosEndpoint, "CustomerDB_Acc", "CustomerCollection");
            this.cosmosFixture.RegisterType<Customer>(cosmosEndpoint, "CustomerDB_Acc", "CustomerCollection");
            this.cosmosFixture.RegisterType<CustomerProfile>(cosmosEndpointForCustomerTrx, "CustomerTransactionsDB_Acc", "CustomerDetailsCollection");
            this.cosmosFixture.RegisterType<MembershipChange>(cosmosEndpointForCustomerTrx, "CustomerTransactionsDB_Acc", "CustomerDetailsCollection");
            this.cosmosFixture.RegisterType<PartnerMembership>(cosmosEndpointForCustomerTrx, "CustomerTransactionsDB_Acc", "CustomerDetailsCollection");
            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, "CustomerTransactionsDB_Acc", "CustomerDetailsCollection");
        }

        [Theory]
        [Trait("Category", Traits.BookBffRegression)]
        [InlineData("4319310000000092", "VISA", "Non3DS", LocationType.JFK, LocationType.LHR, 1, 0, 0, 0, FareFamilyType.ECONOMYDELIGHT)]
        [InlineData("4319310000000092", "VISA", "Non3DS", LocationType.LHR, LocationType.DEL, 1, 0, 0, 0, FareFamilyType.ECONOMYDELIGHT)]
        public async Task TC04_BasicEnrolmentValidation_ExUS(string cardNumber, string cardType, string threeDS, LocationType origin, LocationType destination, int numberOfadults, int numberOfteens, int numberOfchildren, int numberOfinfants, FareFamilyType fareFamily)
        {
            //Generating offer search BFF request
            var offerSearchBFFRequest = OfferSearchHelper.OfferSearchBFFRequest(JourneyType.OneWay, origin: origin, destination: destination, adults: numberOfadults, teens: numberOfteens, child: numberOfchildren, infants: numberOfinfants);

            //Post search BFF endpoint
            HttpResponseMessage offerSearchBFFResponse = WebBFFEndpoint.OfferSearchBFF(offerSearchBFFRequest).EnsureIsSuccessStatusCode();

            var result = offerSearchBFFResponse.EnsureIsSuccessStatusCode().ReadResponseAsString();

            //Getting the link url for the SummaryBff
            string summaryLink = string.Empty;
            var summaryLinks = OfferSearchHelper.FindAllSummaryLinksForSpecificCabinAndAirline(result, fareFamily, FlightType.VS);
            if (summaryLinks.Length > 0)
                summaryLink = summaryLinks[0];
            else
            {
                Assert.False(summaryLinks.Length == 0, $"No summary links found for {origin} and {destination} and {fareFamily}, {cardNumber}");
            }
            var url = "";
            var basketId = "";
            //Getting basketId from the response url
            basketId = summaryLink.Split('/')[5];
            var slice = summaryLink.Split('/')[7];
            var offerId = summaryLink.Split('/').LastOrDefault();

            //Need to build url for Summary link from returned url
            url = WebBFFHelper.BuildSummaryUrl(basketId, slice, offerId);

            //Post to summary endpoint taken from search result
            var summaryBffResponse = WebBFFEndpoint.SummaryBFF(url).EnsureIsSuccessStatusCode();

            Assert.True(WebBFFAssertions.ValidateFareFamily(summaryBffResponse.ReadResponseAsString(), fareFamily, out string fareFamilyType), fareFamilyType);

            //Get the basket from the BFF
            HttpResponseMessage getBasketResponseBff = WebBFFEndpoint.GetBasketBFF(basketId).EnsureIsSuccessStatusCode();

            dynamic getBasket = JsonConvert.DeserializeObject<dynamic>(await getBasketResponseBff.Content.ReadAsStringAsync());

            WebBFFHelper.UpdateAllPassengerDetails(getBasket, basketId);

            string contactDetailsLink = getBasket["contactDetails"]["link"].ToString();
            string tncsLink = getBasket["termsAndConditions"]["link"].ToString();

            //Updating valid contact details
            WebBFFHelper.UpdateValidContactDetails(basketId, contactDetailsLink);

            //Creating TnCs BFF request as accepted and posting it
            var updateTnCResponseBff = WebBFFHelper.UpdateTnCRequestBff(true);

            HttpResponseMessage updateTnCResult = WebBFFEndpoint.UpdateTnCBFF(basketId, tncsLink, updateTnCResponseBff).EnsureIsSuccessStatusCode();

            dynamic tncResponse = JsonConvert.DeserializeObject<dynamic>(await updateTnCResult.Content.ReadAsStringAsync());

            Assert.True(tncResponse.termsAndConditions.validation.ToString().Equals("VALID", StringComparison.OrdinalIgnoreCase), "VALID was not returned as expected for TnC");

            Assert.True(tncResponse.validation.ToString().Equals("VALID", StringComparison.OrdinalIgnoreCase), "VALID was not returned as expected for Basket");

            string leadPassengerFirstName = tncResponse["people"][0]["firstName"].Value;
            string leadPassengerSurname = tncResponse["people"][0]["lastName"].Value;

            dynamic paymentAmount = JsonConvert.DeserializeObject<dynamic>(await updateTnCResult.Content.ReadAsStringAsync());

            //This is the amount taken from the basket
            var amountToPay = paymentAmount.summary.totalPrice.Value;

            decimal amountToPayDecimal = (decimal)amountToPay;

            //Creating Payment Bff request and posting it
            var paymentRequest = WebBFFHelper.Payment();

            HttpResponseMessage submitPaymentResponseBff = WebBFFEndpoint.PostPaymentBFF(basketId, paymentRequest).EnsureIsSuccessStatusCode();

            dynamic submitPaymentAmountResult = JsonConvert.DeserializeObject<dynamic>(await submitPaymentResponseBff.Content.ReadAsStringAsync());

            //Get the hpp url from the above response
            var hppUrl = submitPaymentAmountResult.paymentSessions[0].paymentLink.Value;

            WebBFFHelper.HPPUrl(hppUrl, out string pri, out string orderNo);

            using var enterHppCardDetails = new NavigateToHostedPaymentPage(hppUrl);

            enterHppCardDetails.SubmitHppCardDetails("Name onCard", cardNumber, "1228", "123", "12 address line One", "addressline Two", "Town", "United States", "12345", state: "Alaska");

            //Confirmation BFF
            HttpResponseMessage confirmationResponse = WebBFFEndpoint.Confirmation(basketId).EnsureIsSuccessStatusCode();

            if (confirmationResponse.ToString().Contains("CREATE PNR SELL ERROR -- SPACE UNAVAILABLE"))
                return;

            confirmationResponse.EnsureIsSuccessStatusCode();

            dynamic confirmationResult = JsonConvert.DeserializeObject<dynamic>(await confirmationResponse.Content.ReadAsStringAsync());

            //Extract PNR
            var pnr = confirmationResult.summary.outboundFlights[0].sectors[0].pnr.Value;
            var priceCurrencyRes = confirmationResult.summary.priceCurrency.Value;
                        
            string strInputQueryForCustomerSearch = "SELECT * FROM CustomerCollection c WHERE c.FirstName = '" + leadPassengerFirstName + "' and c.DocType = '" + docTypeCustomerSearch + "'";

            CustomerSearch objCustomerSearch = cosmosFixture.QueryItem<CustomerSearch>(strInputQueryForCustomerSearch);

            int retry = 10;
            while (retry > 0 && objCustomerSearch == null)
            {
                Thread.Sleep(5000);
                objCustomerSearch = cosmosFixture.QueryItem<CustomerSearch>(strInputQueryForCustomerSearch);
                retry--;
            }

            Assert.True(objCustomerSearch != null, "Query from Cosmos DB for Customer Search is null after 5 re tries");

            var dbOPFirstName = objCustomerSearch.FirstName;
            var dbOPLastName = objCustomerSearch.LastName;
            var dbOPEmail = objCustomerSearch.EmailAddress;
            var dbOPMiddleName = objCustomerSearch.MiddleName;
            var dbOPCustomerIdSearch = objCustomerSearch.CustomerId;

            Assert.True(leadPassengerFirstName.Equals(dbOPFirstName), $"firstName - {leadPassengerFirstName} did not match db stored firstName {dbOPFirstName}");
            Assert.True(leadPassengerSurname.Equals(dbOPLastName), $"lastName - {leadPassengerFirstName} did not match db stored lastName {dbOPLastName}");
            Assert.Null(objCustomerSearch.DateOfBirth);

            string strInputQueryForCustomer = $"SELECT * FROM CustomerCollection c WHERE c.FirstName = '" + leadPassengerFirstName + "' and c.DocType = '" + docTypeCustomer + "' and c.CustomerId ='" + dbOPCustomerIdSearch + "'";

            Customer objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomer);

            retry = 10;
            while (retry > 0 && objCustomer == null)
            {
                Thread.Sleep(5000);
                objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomer);
                retry--;
            }

            Assert.True(objCustomer != null, "Query from Cosmos DB for Customer is null after 5 re tries");

            var dbOPDocTypeCS = objCustomer.DocType;
            var dbOPCustomerId = objCustomer.CustomerId;
            var dbOPMiddleNameCustomer = objCustomer.MiddleName;
            var createdByCustomer = objCustomer.CreatedBy;

            Assert.True(dbOPDocTypeCS.Equals(docTypeCustomer), $"docType - {dbOPDocTypeCS} did not match docType {docTypeCustomer}");
            Assert.True(dbOPCustomerIdSearch.Equals(dbOPCustomerId), $"CustomerID - {dbOPCustomerId} did not match db stored emailId {dbOPCustomerIdSearch}");
            Assert.True(objCustomer.CreatedBy.Equals(Application.WEBChannelId), $"createdBy - {createdByCustomer} did not match db stored createdBy {Application.WEBChannelId}");
            Assert.Null(objCustomer.DateOfBirth);
            Assert.True(objCustomer.Gender.Equals(string.Empty), $"Gender - {objCustomer.Gender} should be empty");
            Assert.True(objCustomer.EnrollmentChannel.Equals(Application.WEBChannelId), $"Channel - {Application.WEBChannelId} did not match db stored channel {objCustomer.EnrollmentChannel}");

            string strInputQueryForCustomerProfile = "SELECT * FROM CustomerDetailsCollection c WHERE c.PartitionKey = '" + dbOPCustomerIdSearch + "' and c.DocType = '" + docTypeCustomerProfile + "'";
            CustomerProfile objCustomerProfile = cosmosFixture.QueryItem<CustomerProfile>(strInputQueryForCustomerProfile);

            retry = 10;
            while (retry > 0 && objCustomerProfile == null)
            {
                Thread.Sleep(5000);
                objCustomerProfile = cosmosFixture.QueryItem<CustomerProfile>(strInputQueryForCustomerProfile);
                retry--;
            }

            Assert.True(objCustomerProfile != null, "Query from Cosmos DB for Customer Profile is null after 5 re tries");

            var dbOPDocTypeCP = objCustomerProfile.DocType;
            var dbOPCustomerIdProfile = objCustomerProfile.CustomerId;

            Assert.True(dbOPDocTypeCP.Equals(docTypeCustomerProfile), $"docType - {dbOPDocTypeCP} did not equal docType {docTypeCustomerProfile}");
            Assert.True(dbOPCustomerIdProfile.Equals(dbOPCustomerIdSearch), $"CustomerId - {dbOPCustomerIdProfile} did not equal PreferenceType {dbOPCustomerIdSearch}");
            Assert.True(objCustomerProfile.ReasonForTravel.Equals(string.Empty), $"ReasonForTravel - {objCustomerProfile.ReasonForTravel} should be blank");

            string strInputQueryForMembershipChange = "SELECT * FROM CustomerDetailsCollection c WHERE c.PartitionKey = '" + dbOPCustomerIdSearch + "' and c.DocType = '" + docTypeMembershipChange + "'";
            MembershipChange objMembershipChange = cosmosFixture.QueryItem<MembershipChange>(strInputQueryForMembershipChange);

            retry = 10;
            while (retry > 0 && objMembershipChange == null)
            {
                Thread.Sleep(5000);
                objMembershipChange = cosmosFixture.QueryItem<MembershipChange>(strInputQueryForMembershipChange);
                retry--;
            }

            Assert.True(objMembershipChange != null, "Query from Cosmos DB for membership Change is null after 5 re tries");

            var dbOPDocTypeMC = objMembershipChange.DocType;
            var dbOPCustomerIdMC = objMembershipChange.CustomerId;
            var dbOPTierCodeMC = objMembershipChange.TierCode;

            Assert.True(dbOPDocTypeMC.Equals(docTypeMembershipChange), $"docType - {dbOPDocTypeMC} did not equal docType {docTypeMembershipChange}");
            Assert.True(dbOPCustomerIdMC.Equals(dbOPCustomerIdSearch), $"CustomerId - {dbOPCustomerIdMC} did not equal PreferenceType {dbOPCustomerIdSearch}");
            Assert.True(dbOPTierCodeMC.Equals(Application.GlassTierCode), $"TierCode - {dbOPCustomerIdMC} did not equal Tier code {Application.GlassTierCode}");

            string strInputQueryForPartnerMembership = "SELECT * FROM CustomerDetailsCollection c WHERE c.PartitionKey = '" + dbOPCustomerIdSearch + "' and c.DocType = '" + docTypePartnerMemberShip + "'";
            PartnerMembership objPartnerMembership = cosmosFixture.QueryItem<PartnerMembership>(strInputQueryForPartnerMembership);

            retry = 10;
            while (retry > 0 && objPartnerMembership == null)
            {
                Thread.Sleep(5000);
                objPartnerMembership = cosmosFixture.QueryItem<PartnerMembership>(strInputQueryForPartnerMembership);
                retry--;
            }

            Assert.True(objPartnerMembership != null, "Query from Cosmos DB for Partner Membership is null after 5 re tries");

            var dbOPDocTypePM = objPartnerMembership.DocType;
            var dbOPCustomerIdPM = objPartnerMembership.CustomerId;

            Assert.True(dbOPDocTypePM.Equals(docTypePartnerMemberShip), $"docType - {dbOPDocTypePM} did not equal docType {docTypePartnerMemberShip}");
            Assert.True(dbOPCustomerIdPM.Equals(dbOPCustomerIdSearch), $"CustomerId - {dbOPCustomerIdPM} did not equal PreferenceType {dbOPCustomerIdSearch}");

            string strInputQueryForFlyingClubMember = "SELECT * FROM CustomerDetailsCollection c WHERE c.PartitionKey = '" + dbOPCustomerIdSearch + "' and c.DocType = '" + docTypeFlyingClubMember + "'";

            FlyingClubMember objFlyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(strInputQueryForFlyingClubMember);

            retry = 10;
            while (retry > 0 && objFlyingClubMember == null)
            {
                Thread.Sleep(5000);
                objFlyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(strInputQueryForFlyingClubMember);
                retry--;
            }

            Assert.True(objFlyingClubMember != null, "Query from Cosmos DB for Customer Flynig Club member is null after 5 re tries");

            var dbOPDocTypeFM = objFlyingClubMember.DocType;
            var dbOPCustomerIdFM = objFlyingClubMember.CustomerId;
            var dbOPTierCodeFM = objFlyingClubMember.TierCode;

            Assert.True(dbOPDocTypeFM.Equals(docTypeFlyingClubMember), $"docType - {dbOPDocTypeFM} did not equal docType {docTypeFlyingClubMember}");
            Assert.True(dbOPCustomerIdFM.Equals(dbOPCustomerIdSearch), $"CustomerId - {dbOPCustomerIdFM} did not equal PreferenceType {dbOPCustomerIdSearch}");
            Assert.True(dbOPTierCodeFM.Equals(Application.GlassTierCode), $"TierCode - {dbOPTierCodeFM} did not equal Tier code {Application.GlassTierCode}");
        }
    }
}