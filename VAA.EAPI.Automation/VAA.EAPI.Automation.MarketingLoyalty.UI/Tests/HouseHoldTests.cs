using AventStack.ExtentReports;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;
using VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary;
using VAA.EAPI.Automation.MarketingLoyalty.UI.PageObjects;
using static VAA.EAPI.Automation.Core.UI.Factory.BrowserFactory;
using Customer = VAA.EAPI.Automation.Core.DB_Model.Entities.Customer;
using FlyingClubMember = VAA.EAPI.Automation.Core.DB_Model.Entities.FlyingClubMember;
using Traits = VAA.EAPI.Automation.MarketingLoyalty.UI.Constants.Traits;

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.Tests
{
    [Collection("HouseHoldTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class HouseHoldTests : IDisposable, IClassFixture<ReportFixture>, IClassFixture<CosmosFixture>, IClassFixture<TokenFixture>
    {
        private readonly ReportFixture reportFixture;
        private readonly CosmosFixture cosmosFixture;
        private readonly MemberSearchPage memberSearchPage;
        private readonly HouseholdPage householdPage;
        private readonly IWebDriver webDriver;
        private readonly MemberProfilePage memberProfilePage;
        private readonly TokenFixture tokenFixture;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        public string env = Config.Environment;

        private static string backendAPIEnv = ConfigReader.Get("BackendAPIEnv") ?? "";

        public HouseHoldTests(ReportFixture reportFixture, CosmosFixture cosmosFixture, TokenFixture tokenFixture)
        {
            this.reportFixture = reportFixture;
            this.cosmosFixture = cosmosFixture;
            this.tokenFixture = tokenFixture;

            if (!backendAPIEnv.Is("Project-DEV"))
                this.tokenFixture.AddTokenForCustomerApiFromKeyVault("customer");

            webDriver = GetBrowser(BrowserType.Chrome, ConfigReader.Get("PowerAppsUrl"));

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.cosmosFixture.RegisterType<Group>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));

            this.cosmosFixture.RegisterType<Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

            this.cosmosFixture.RegisterType<CustomerSearch>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

            memberSearchPage = new MemberSearchPage(webDriver);

            memberProfilePage = new MemberProfilePage(webDriver);

            householdPage = new HouseholdPage(webDriver);

        }


        [Trait("Category", Traits.HouseHoldUIRegression)]
        [Fact, RunOrder(1)]
        public void TC01_RetrieveHouseholdDetailsForLinkedHoHGoldAccount()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC01_RetrieveHouseholdDetailsForLinkedHoHGoldAccount");

            #region  PreRequisite - Fetching the Parent and Child Account Details for household Customers

            string parentCustomerId = string.Empty;

            string parentMemberNumber = string.Empty;

            string queryForFCMDoc = "SELECT Top 5 * FROM  c where c.DocType='FlyingClubMember' and c.MemberType='Household' and c.HeadOfGroupMemberNumber='' and c.GroupId!='' order by c.LastUpdatedOn desc";

            IEnumerable<FlyingClubMember> flyingClubDocument = cosmosFixture.QueryItems<FlyingClubMember>(queryForFCMDoc);

            Assert.True(flyingClubDocument is not null, $"No Document available for the Searched Query {queryForFCMDoc}.");

            Dictionary<string, string>? childCustomerIds = new();

            foreach (FlyingClubMember memberProfile in flyingClubDocument)
            {
                parentCustomerId = memberProfile.CustomerId;
                parentMemberNumber = memberProfile.MemberNumber;

                string queryForGroupDocument = $"SELECT * FROM  c where c.DocType='Group' and c.HeadOfGroupCustomerId= '{parentCustomerId}' order by c.LastUpdatedOn desc ";

                IEnumerable<GroupHistory> groupHistory = cosmosFixture.QueryItem<Group>(queryForGroupDocument).GroupHistory;

                IEnumerable<GroupHistory> activeChilds = groupHistory.Where(c => !c.CustomerId.Equals(memberProfile.CustomerId) && string.IsNullOrEmpty(c.ExitDate));

                foreach (GroupHistory history in activeChilds)
                {
                    childCustomerIds.Add(history.CustomerId, "");
                }

                if (childCustomerIds.Any())
                {
                    break;
                }
            }

            Assert.True(!string.IsNullOrEmpty(parentCustomerId),
                        "Pre-requeiste not met, data not present for HoH parent Search ");

            Assert.True(childCustomerIds.Any(),
            "Pre-requeiste not met, no Parent account found with active child members ");

            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, parentMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMenuBarOnMemberProfilePage(logger);

            householdPage.ClickOnRelationShipTab(logger);

            Assert.True(!householdPage.IsLinkButtonDisabled(),
                "Expected the Link button to be enabled but found disabled.");

            Assert.True(!householdPage.IsDeLinkButtonDisabled(),
                "Expected the DeLink button to be enabled but found disabled.");

        }


        [Trait("Category", Traits.HouseHoldUIRegression)]
        [Fact, RunOrder(2)]
        public void TC02_LinkHouseholdAccountSuccessScenario()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC02_LinkHouseholdAccountSuccessScenario");

            string parentCustomerId = string.Empty;

            string childCustomerId = string.Empty;

            #region  PreRequisite - Fetching the Parent and Child Account Details for household Customers

            string queryForParentFCMDoc = "SELECT Top 1 * FROM  c where c.DocType= @docType and c.MemberType= @memberType and c.Status= @status and c.HeadOfGroupMemberNumber='' and c.TierCode in (@tierCode,@tierCode1) and c.TermsAndConditionsFlag = @flag order by c.LastUpdatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForParentFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@status", "Active").WithParameter("@memberType", "Individual").WithParameter("@tierCode", "Gold").WithParameter("@tierCode1", "Silver").WithParameter("@flag", "Y");

            FlyingClubMember parentFlyingClubDocument = cosmosFixture.CosmosQueryItem<FlyingClubMember>(paramQuery);

            parentCustomerId = parentFlyingClubDocument?.CustomerId;

            string parentMemberNumber = parentFlyingClubDocument?.MemberNumber;

            Assert.True(parentFlyingClubDocument is not null, "No Valid Parent Member Found for household linking");

            string queryForChildFCMDoc = "SELECT Top 1 * FROM  c where c.DocType = @docType and c.MemberType = @memberType and c.Status = @status and c.HeadOfGroupMemberNumber = '' and c.TierCode != @tierCode and c.TermsAndConditionsFlag = @flag and c.MemberNumber != @parentMemberNumber order by c.LastUpdatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForChildFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@memberType", "Individual").WithParameter("@status", "Active").WithParameter("@tierCode", "Glass").WithParameter("@flag", "Y").WithParameter("@parentMemberNumber", parentMemberNumber);

            FlyingClubMember childFlyingClubDocument = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            childCustomerId = childFlyingClubDocument?.CustomerId;

            string childMemberNumber = childFlyingClubDocument?.MemberNumber;

            Assert.True(childFlyingClubDocument is not null, "No Valid Child Member Found for household linking");

            Assert.True(!string.IsNullOrEmpty(parentCustomerId),
                "The parent CustomerId value returned is Null or Empty");

            Assert.True(!string.IsNullOrEmpty(childCustomerId),
                    "The child CustomerId value returned is Null or Empty");

            #endregion

            #region Fetching the Customer Document of Parent

            string queryForCustomerDoc = $"SELECT * FROM  c where c.DocType= @docType and c.PartitionKey= @partitionKey order by c.LastUpdatedOn desc";

            QueryDefinition customerParamQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", parentCustomerId);

            Address addressDetails = cosmosFixture.CosmosQueryItem<Customer>(customerParamQuery).AddressDetails.FirstOrDefault(x => x.Primary == "Y");

            ContactDetails emailDetails = cosmosFixture.CosmosQueryItem<Customer>(customerParamQuery).ContactDetails.FirstOrDefault(x => x.Primary == "Y" && x.ContactType == "Email");

            Assert.True(addressDetails is not null, $"No Primary Address Found for Parent Member {parentMemberNumber}");

            Assert.True(addressDetails.Valid == "Y", $"Address Valid Flag is not Y for Parent Member {parentMemberNumber}");

            Assert.True(emailDetails is not null, $"No Primary Email Found for Parent Member {parentMemberNumber}");

            Assert.True(emailDetails.Valid == "Y", $"Email Valid Flag is not Y for Parent Member {parentMemberNumber}");

            string? parentaddressId = addressDetails.AddressId;
            string? addressLine1 = addressDetails.AddressLine1;
            string? addressLine2 = addressDetails.AddressLine2;
            string? addressLine3 = addressDetails.AddressLine3;
            string? company = addressDetails.LandlordOrCompanyName;
            string? postalCode = addressDetails.PostCode;
            string? city = addressDetails.City;
            string? country = addressDetails.Country;

            Assert.True(!string.IsNullOrEmpty(parentaddressId), $"The Address Id value is Null or Empty for Parent Member {parentMemberNumber}");

            #endregion

            #region Fetching the Customer Document of ChildAccounts

            string queryForChildCustomerDoc = $"SELECT top 1 * FROM  c where c.DocType=@docType and c.PartitionKey= @partitionKey order by c.LastUpdatedOn desc";

            QueryDefinition childCustomerParamQuery = new QueryDefinition(query: queryForChildCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", childCustomerId);

            Address childAddressDetails = cosmosFixture.CosmosQueryItem<Customer>(childCustomerParamQuery).AddressDetails.FirstOrDefault(x => x.Primary == "Y");

            ContactDetails childEmailDetails = cosmosFixture.CosmosQueryItem<Customer>(childCustomerParamQuery).ContactDetails.FirstOrDefault(x => x.Primary == "Y" && x.ContactType == "Email");

            Assert.True(childEmailDetails is not null, $"No Primary Email Found for Child Member {childMemberNumber}");

            Assert.True(childEmailDetails.Valid == "Y", $"Email Valid Flag is not Y for Child Member {childMemberNumber}");

            Assert.True(childAddressDetails is not null, $"No Primary Address Found for Child Member {childMemberNumber}");

            string? childaddressId = childAddressDetails.AddressId;

            Assert.True(!string.IsNullOrEmpty(childaddressId), $"The Address Id value is Null or Empty for Child Member {childMemberNumber}");

            #endregion

            #region Calling Update Address API

            HttpResponseMessage response = LoopsBackendAPIEndpoints.Customer.UpdateMemberAddress(childCustomerId, childaddressId, tokenFixture.GetToken("customer"),
                                    addressLine1, addressLine2, addressLine3, company,
                                    city, null, postalCode, "", country, "");

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);

            string expectedResponse = "Details have been saved successfully";

            Assert.True(result.ToString().Contains(expectedResponse),
                $"Expected the message to be {expectedResponse} but found {result.ToString()}");

            #endregion

            #region linking of parent and child accounts on the Powerapps UI

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, parentMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMenuBarOnMemberProfilePage(logger);

            householdPage.ClickOnRelationShipTab(logger);

            Assert.True(!householdPage.IsLinkButtonDisabled(),
                "Expected the Link button to be enabled  but found disabled.");

            Assert.True(householdPage.IsDeLinkButtonDisabled(),
                "Expected the DeLink button to be disabled but found enabled.");

            householdPage.ClickOnLinkButton();

            householdPage.EnterChildMemberNumber(childMemberNumber);

            householdPage.ClickOnChildMemberSearchButton();

            Assert.True(householdPage.IsValidateAndLinkButtonEnabled(),
                                    "Expected the validate and Link button to be Enabled but found disabled");

            householdPage.ClickOnValidateAndLink();

            string actualMessage = householdPage.GetPopUpMessage();

            string expectedMessage = "Household linking successful.";

            Assert.True(actualMessage.Has(expectedMessage),
                "household linking was successful");

            householdPage.ClosePopUp();

            Assert.True(householdPage.IsChildAccountPresentInHousehold(childMemberNumber),
                            "Child Member not visible on the Relationship page");

            //added validations for Linkedby and LinkedOn attributes

            Assert.True(householdPage.ChildAccountPresentWithLinkedByAttribute(),
                            "For Child Member linkedBy attribute on the relationship page is missing.");

            Assert.True(householdPage.ChildAccountPresentWithLinkedOnAttribute(),
                            "For Child Member linkedOn attribute on the relationship page is missing.");

            #endregion

        }

        [Trait("Category", Traits.HouseHoldUIRegression)]
        [Fact, RunOrder(3)]
        public void TC03_DelinkLinkHouseholdAccountSuccessScenario()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC03_DelinkLinkHouseholdAccountSuccessScenario");


            #region - PreRequisite - Fetching the Test Data from Cosmos DB


            string headAccountMemberNumber = string.Empty;
            string subOrdinateAccountMemberNumber = string.Empty;

            string queryForFCMDoc = $"SELECT Top 1 * FROM  c where c.DocType='FlyingClubMember'  and c.Status='Active' and  c.MemberType='Individual' and c.HeadOfGroupMemberNumber!='' order by c.LastUpdatedOn desc";

            FlyingClubMember flyingClubDocument = cosmosFixture.QueryItem<FlyingClubMember>(queryForFCMDoc);

            Assert.True(flyingClubDocument is not null, $"No Document available for the Searched Query {queryForFCMDoc}.");

            headAccountMemberNumber = flyingClubDocument.HeadOfGroupMemberNumber;

            subOrdinateAccountMemberNumber = flyingClubDocument.MemberNumber;

            string queryForCustomerSearchDoc = $"SELECT Top 1 * FROM  c where c.DocType='CustomerSearch'  and c.LoyaltyMemberNumber='{headAccountMemberNumber}'";

            CustomerSearch customer = cosmosFixture.QueryItem<CustomerSearch>(queryForCustomerSearchDoc);

            Assert.True(customer is not null, $"No Document available for the Searched Query {queryForCustomerSearchDoc}.");

            #endregion

            #region - delinking the household account on powerApps

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, headAccountMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMenuBarOnMemberProfilePage(logger);

            householdPage.ClickOnRelationShipTab(logger);

            Assert.True(!householdPage.IsLinkButtonDisabled(),
                "Expected the Link button to be enabled  but found disabled.");

            Assert.True(!householdPage.IsDeLinkButtonDisabled(),
                "Expected the DeLink button to be enabled but found disabled.");

            householdPage.ClickOnDelinkChildMember(subOrdinateAccountMemberNumber);

            string actualMessage = householdPage.GetPopUpMessage();

            string expectedMessage = "Household delinking successful";

            Assert.True(actualMessage.Has(expectedMessage),
                "Household delinking successful");

            householdPage.ClosePopUp();

            Assert.True(householdPage.IsChildAccountPresentInHouseholdHistory(subOrdinateAccountMemberNumber),
                            "Child Member is not visible on the Relationship page after delinking");

            //added validations for Linkedby and LinkedOn attributes

            Assert.True(householdPage.ChildAccountPresentWithLinkedByAttributeInHouseholdHistory(),
                            "For Child Member linkedBy attribute on the relationship page is missing.");

            Assert.True(householdPage.ChildAccountPresentWithLinkedOnAttributeInHouseholdHistory(),
                            "For Child Member linkedOn attribute on the relationship page is missing.");

            #endregion
        }

        [Trait("Category", Traits.HouseHoldUIRegression)]
        [Fact, RunOrder(4)]
        public void TC04_ValidateThatTheLinkButtonIsDisabledForRedTierMembers()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC04_ValidateThatTheLinkButtonIsDisabledForRedTierMembers");

            #region Test Data

            string queryForFcmDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode <> @tierCode order by c.CreatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForFcmDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "Glass");

            FlyingClubMember flyingClubMemberDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(paramQuery);

            string memberNumber = flyingClubMemberDoc.MemberNumber;

            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMenuBarOnMemberProfilePage(logger);

            householdPage.ClickOnRelationShipTab(logger);

            Assert.True(householdPage.IsLinkButtonDisabled(), "Expected the link button for the Red Member to be in disabled State but found Enabled");

        }

        [Trait("Category", Traits.HouseHoldUIRegression)]
        [Fact, RunOrder(5)]
        public void TC05_ErrorMessageValidationOnLinkButtonForSilverOrGoldMembersIfMemberAgeIsLessThan18Years()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC05_ErrorMessageValidationOnLinkButtonForSilverOrGoldMembersIfMemberAgeIsLessThan18Years");

            string memberNumber = "9106548424";// Valid Silver/Gold Member with Age < 18years

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMenuBarOnMemberProfilePage(logger);

            householdPage.ClickOnRelationShipTab(logger);

            householdPage.ClickOnLinkButton();

            string expectedMessage = "The Head of Household account you are trying to link members to must be over 18 years of age.";

            string actualMessage = householdPage.GetPopUpMessage();

            Assert.True(expectedMessage.Equals(actualMessage.Replace("\r\n ", "")),
                $"Expected error message to be {expectedMessage} but found {actualMessage}.");

        }

        [Trait("Category", Traits.HouseHoldUIRegression)]
        [Fact, RunOrder(6)]
        public void TC06_ValidateErrorMessageOnLinkButtonForSilverorGoldTierMembersIfTermsAndConditionsAreNotAccepted()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC06_ValidateErrorMessageOnLinkButtonForSilverorGoldTierMembersIfTermsAndConditionsAreNotAccepted");

            #region Test Data

            string queryForFcmDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode in ( @tierCode , @tierCode1)  and c.TermsAndConditionsFlag != @flag order by c.LastUpdatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForFcmDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "Gold").WithParameter("@tierCode1", "Silver").WithParameter("@flag", "Y");

            FlyingClubMember flyingClubMemberDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(paramQuery);

            Assert.True(flyingClubMemberDoc is not null, "Didn't find valid test data for the test execution");

            string memberNumber = flyingClubMemberDoc.MemberNumber;

            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMenuBarOnMemberProfilePage(logger);

            householdPage.ClickOnRelationShipTab(logger);

            householdPage.ClickOnLinkButton();

            string actualMessage = householdPage.GetPopUpMessage();

            string expectedMessage = "Terms and Conditions have not been signed on the Head of Household account you are trying to link members to.";

            Assert.True(actualMessage.Equals(expectedMessage), $"Expected error message to be {expectedMessage} but found {actualMessage}.");

        }

        [Trait("Category", Traits.HouseHoldUIRegression)]
        [Fact, RunOrder(7)]
        public void TC07_ErrorMessageValidationOnLinkButtonForSilverOrGoldMembersIfMemberEmailAddressIsInvalid()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC07_ErrorMessageValidationOnLinkButtonForSilverOrGoldMembersIfMemberEmailAddressIsInvalid");

            string memberNumber = "9106542294";// Valid Silver/Gold Member whose Current Email Valid Flag is not Y

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMenuBarOnMemberProfilePage(logger);

            householdPage.ClickOnRelationShipTab(logger);

            householdPage.ClickOnLinkButton();

            string expectedMessage = "Household owner has invalid email address.";

            string actualMessage = householdPage.GetPopUpMessage();

            Assert.True(actualMessage.Equals(expectedMessage), $"Expected error message to be {expectedMessage} but found {actualMessage}.");

        }

        [Trait("Category", "HouseholdUIRegression-Not-A-Pipeline-Candidate")]
        [Fact, RunOrder(8)]
        public void TC08_ValidateErrorMessageOnLinkButtonForSilverorGoldTierMembersIfMemberStatusIsNotActive()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC08_ValidateErrorMessageOnLinkButtonForSilverorGoldTierMembersIfMemberStatusIsNotActive");

            #region Test Data

            string queryForFcmDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode in ( @tierCode , @tierCode1)  and c.Status != @status and c.GroupID = '' and c.HeadOfGroupMemberNumber = '' and c.TermsAndConditionsFlag  = @flag order by c.LastUpdatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForFcmDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "Gold").WithParameter("@tierCode1", "Silver").WithParameter("@status", "Active").WithParameter("@flag", "Y");

            FlyingClubMember flyingClubMemberDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(paramQuery);

            Assert.True(flyingClubMemberDoc is not null, "Didn't find valid test data for the test execution");

            string memberNumber = flyingClubMemberDoc.MemberNumber;

            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMenuBarOnMemberProfilePage(logger);

            householdPage.ClickOnRelationShipTab(logger);

            householdPage.ClickOnLinkButton();

            string actualMessage = householdPage.GetPopUpMessage();

            string expectedMessage = "Household owner account status is not active.";

            Assert.True(actualMessage.Equals(expectedMessage), $"Expected error message to be {expectedMessage} but found {actualMessage}.");

        }

        [Trait("Category", "HouseholdUIRegression-Not-A-Pipeline-Candidate")]
        [Fact, RunOrder(9)]
        public void TC09_ValidateErrorMessageOnLinkMemberScreenForSilverorGoldTierMembersIfChildMemberNumberEnteredIsInvalid()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC09_ValidateErrorMessageOnLinkMemberScreenForSilverorGoldTierMembersIfChildMemberNumberEnteredIsInvalid");

            #region Test Data

            string queryForFcmDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode in ( @tierCode , @tierCode1)  and c.Status = @status and c.GroupID = '' and c.HeadOfGroupMemberNumber = '' and c.TermsAndConditionsFlag  = @flag order by c._ts desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForFcmDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "Gold").WithParameter("@tierCode1", "Silver").WithParameter("@status", "Active").WithParameter("@flag", "Y");

            FlyingClubMember flyingClubMemberDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(paramQuery);

            Assert.True(flyingClubMemberDoc is not null, "Didn't find valid test data for the test execution");

            string memberNumber = flyingClubMemberDoc.MemberNumber;

            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMenuBarOnMemberProfilePage(logger);

            householdPage.ClickOnRelationShipTab(logger);

            householdPage.ClickOnLinkButton();

            householdPage.EnterChildMemberNumber("1232424143");

            householdPage.ClickOnChildMemberSearchButton();

            string actualMessage = householdPage.GetLinkPopUpErrorMessage();

            string expectedMessage = "No valid results to display";

            Assert.True(actualMessage.Equals(expectedMessage), $"Expected error message to be {expectedMessage} but found {actualMessage}.");

            Assert.True(!householdPage.IsValidateAndLinkButtonEnabled(), "Expected the ValidateAndLink Button to be in disabled State but found Enabled.");

        }

        [Trait("Category", "HouseholdUIRegression-Not-A-Pipeline-Candidate")]
        [Fact, RunOrder(10)]
        public void TC10_ValidateErrorMessageOnLinkMemberScreenForSilverorGoldTierMembersIfChildMemberNumberEnteredIsAlreadyLinkedToTheSameParent()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC10_ValidateErrorMessageOnLinkMemberScreenForSilverorGoldTierMembersIfChildMemberNumberEnteredIsAlreadyLinkedToTheSameParent");

            #region Test Data

            string queryForGroupDocument = "SELECT TOP 5 * FROM CustomerDetailsCollection c WHERE  c.DocType = @docType order by c.CreatedOn desc";

            QueryDefinition groupDocParamQuery = new QueryDefinition(query: queryForGroupDocument).WithParameter("@docType", "Group");

            IEnumerable<Group> groupDocs = cosmosFixture.CosmosQueryItems<Group>(groupDocParamQuery);

            string parentCustomerId = string.Empty;

            string childCustomerId = string.Empty;

            childCustomerId = groupDocs?.FirstOrDefault(x =>
            {
                parentCustomerId = x.HeadOfGroupCustomerId;

                return x.GroupHistory.Any(c => string.IsNullOrWhiteSpace(c.ExitDate));

            }).GroupHistory.First(c => string.IsNullOrWhiteSpace(c.ExitDate)).CustomerId;


            Assert.True(!string.IsNullOrEmpty(childCustomerId), "Child CustomerId Is Empty. Test Data Not found");

            Assert.True(!string.IsNullOrEmpty(parentCustomerId), "Parent CustomerId Is Empty. Test Data Not found");

            string queryForCustomerDoc = "SELECT * FROM CustomerCollection c where c.DocType= @docType and c.PartitionKey = @customerId";

            QueryDefinition customerDocParamQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@customerId", parentCustomerId);

            Customer custDoc = cosmosFixture.CosmosQueryItem<Customer>(customerDocParamQuery);

            string parentMemberNumber = custDoc.LoyaltyMemberNumber;

            customerDocParamQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@customerId", childCustomerId);

            custDoc = cosmosFixture.CosmosQueryItem<Customer>(customerDocParamQuery);

            string childMemberNumber = custDoc.LoyaltyMemberNumber;


            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, parentMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMenuBarOnMemberProfilePage(logger);

            householdPage.ClickOnRelationShipTab(logger);

            householdPage.ClickOnLinkButton();

            householdPage.EnterChildMemberNumber(childMemberNumber);

            householdPage.ClickOnChildMemberSearchButton();

            householdPage.ClickOnValidateAndLink();

            string actualMessage = householdPage.GetPopUpMessage();

            string expectedMessage = "The account is already linked to this HouseHold account.";

            Assert.True(actualMessage.Equals(expectedMessage), $"Expected error message to be {expectedMessage} but found {actualMessage}.");

        }

        [Trait("Category", "HouseholdUIRegression-Not-A-Pipeline-Candidate")]
        [Fact, RunOrder(11)]
        public void TC11_ValidateErrorMessageOnLinkMemberScreenForSilverorGoldTierMembersIfChildMemberNumberEnteredIsAlreadyAPartOfAnotherHousehold()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC11_ValidateErrorMessageOnLinkMemberScreenForSilverorGoldTierMembersIfChildMemberNumberEnteredIsAlreadyAPartOfAnotherHousehold");

            #region Test Data

            string queryForFcmDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode in ( @tierCode , @tierCode1)  and c.Status = @status and c.GroupID = '' and c.HeadOfGroupMemberNumber = '' and c.TermsAndConditionsFlag  = @flag order by c._ts desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForFcmDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "Gold").WithParameter("@tierCode1", "Silver").WithParameter("@status", "Active").WithParameter("@flag", "Y");

            FlyingClubMember flyingClubMemberDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(paramQuery);

            Assert.True(flyingClubMemberDoc is not null, "Didn't find valid test data for the test execution");

            string parentMemberNumber = flyingClubMemberDoc.MemberNumber;

            string queryForChildFcmDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode != @tierCode  and c.Status = @status and c.GroupID = '' and c.HeadOfGroupMemberNumber not in ( '',null)  and c.TermsAndConditionsFlag  = @flag order by c._ts desc";

            QueryDefinition childParamQuery = new QueryDefinition(query: queryForChildFcmDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "Glass").WithParameter("@status", "Active").WithParameter("@flag", "Y");

            flyingClubMemberDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(childParamQuery);

            string childMemberNumber = flyingClubMemberDoc.MemberNumber;

            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, parentMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMenuBarOnMemberProfilePage(logger);

            householdPage.ClickOnRelationShipTab(logger);

            householdPage.ClickOnLinkButton();

            householdPage.EnterChildMemberNumber(childMemberNumber);

            householdPage.ClickOnChildMemberSearchButton();

            householdPage.ClickOnValidateAndLink();

            string actualMessage = householdPage.GetPopUpMessage();

            string expectedMessage = "The member you are trying to link is already linked to a Household.";

            Assert.True(actualMessage.Equals(expectedMessage), $"Expected error message to be {expectedMessage} but found {actualMessage}.");

        }

        [Trait("Category", "HouseholdUIRegression-Not-A-Pipeline-Candidate")]
        [Fact, RunOrder(12)]
        public void TC12_ValidateErrorMessageOnLinkMemberScreenForSilverorGoldTierMembersIfChildMemberNumberStatusIsNotValid()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC12_ValidateErrorMessageOnLinkMemberScreenForSilverorGoldTierMembersIfChildMemberNumberStatusIsNotValid");

            #region Test Data

            string queryForFcmDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode in ( @tierCode , @tierCode1)  and c.Status = @status and c.GroupID = '' and c.HeadOfGroupMemberNumber = '' and c.TermsAndConditionsFlag  = @flag order by c._ts desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForFcmDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "Gold").WithParameter("@tierCode1", "Silver").WithParameter("@status", "Active").WithParameter("@flag", "Y");

            FlyingClubMember flyingClubMemberDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(paramQuery);

            Assert.True(flyingClubMemberDoc is not null, "Didn't find valid test data for the test execution");

            string parentMemberNumber = flyingClubMemberDoc.MemberNumber;

            string queryForChildFcmDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode !=  @tierCode  and c.Status not in ( @status , @status1 ,@status2 , @status3) and c.GroupID = '' and c.HeadOfGroupMemberNumber = ''  and c.TermsAndConditionsFlag  = @flag  order by c._ts desc";

            QueryDefinition childParamQuery = new QueryDefinition(query: queryForChildFcmDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "Glass").WithParameter("@status", "Active").WithParameter("@status1", "Active Pending").WithParameter("@status2", "Frozen").WithParameter("@status3", "Suspended").WithParameter("@flag", "Y");

            flyingClubMemberDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(childParamQuery);

            string childMemberNumber = flyingClubMemberDoc.MemberNumber;

            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, parentMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMenuBarOnMemberProfilePage(logger);

            householdPage.ClickOnRelationShipTab(logger);

            householdPage.ClickOnLinkButton();

            householdPage.EnterChildMemberNumber(childMemberNumber);

            householdPage.ClickOnChildMemberSearchButton();

            householdPage.ClickOnValidateAndLink();

            string actualMessage = householdPage.GetPopUpMessage();

            string expectedMessage = "The status of the account you are trying to link is not valid";

            Assert.True(actualMessage.Equals(expectedMessage), $"Expected error message to be {expectedMessage} but found {actualMessage}.");

        }

        [Trait("Category", "HouseholdUIRegression-Not-A-Pipeline-Candidate")]
        [Fact, RunOrder(13)]
        public void TC13_ValidateErrorMessageOnLinkMemberScreenForSilverorGoldTierMembersIfChildMemberAddressDoesNotMatchWithParentAddress()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC13_ValidateErrorMessageOnLinkMemberScreenForSilverorGoldTierMembersIfChildMemberAddressDoesNotMatchWithParentAddress");

            #region Test Data

            string queryForFcmDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode in ( @tierCode , @tierCode1)  and c.Status = @status and c.GroupID = '' and c.HeadOfGroupMemberNumber = '' and c.TermsAndConditionsFlag  = @flag order by c._ts desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForFcmDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "Gold").WithParameter("@tierCode1", "Silver").WithParameter("@status", "Active").WithParameter("@flag", "Y");

            FlyingClubMember flyingClubMemberDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(paramQuery);

            Assert.True(flyingClubMemberDoc is not null, "Didn't find valid test data for the test execution");

            string parentMemberNumber = flyingClubMemberDoc.MemberNumber;

            string queryForChildFcmDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode != @tierCode  and c.Status in ( @status , @status1 ,@status2 , @status3) and c.GroupID = '' and c.HeadOfGroupMemberNumber = ''  and c.TermsAndConditionsFlag  = @flag  order by c._ts desc";

            QueryDefinition childParamQuery = new QueryDefinition(query: queryForChildFcmDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "Glass").WithParameter("@status", "Active").WithParameter("@status1", "Active Pending").WithParameter("@status2", "Frozen").WithParameter("@status3", "Suspended").WithParameter("@flag", "Y");

            flyingClubMemberDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(childParamQuery);

            string childMemberNumber = flyingClubMemberDoc.MemberNumber;

            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, parentMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMenuBarOnMemberProfilePage(logger);

            householdPage.ClickOnRelationShipTab(logger);

            householdPage.ClickOnLinkButton();

            householdPage.EnterChildMemberNumber(childMemberNumber);

            householdPage.ClickOnChildMemberSearchButton();

            householdPage.ClickOnValidateAndLink();

            string actualMessage = householdPage.GetPopUpMessage();

            string expectedMessage = "The address of the linked member and the Head of Household must match.";

            Assert.True(actualMessage.Equals(expectedMessage), $"Expected error message to be {expectedMessage} but found {actualMessage}.");

        }

        [Trait("Category", "HouseholdUIRegression-Not-A-Pipeline-Candidate")]
        [Fact, RunOrder(14)]
        public void TC14_ValidateErrorMessageOnLinkMemberScreenForSilverorGoldTierMembersIfChildMemberEmailAddressIsInvalid()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC14_ValidateErrorMessageOnLinkMemberScreenForSilverorGoldTierMembersIfChildMemberEmailAddressIsInvalid");

            #region Test Data

            string queryForFcmDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode in ( @tierCode , @tierCode1)  and c.Status = @status and c.GroupID = '' and c.HeadOfGroupMemberNumber = '' and c.TermsAndConditionsFlag  = @flag order by c._ts desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForFcmDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "Gold").WithParameter("@tierCode1", "Silver").WithParameter("@status", "Active").WithParameter("@flag", "Y");

            FlyingClubMember flyingClubMemberDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(paramQuery);

            Assert.True(flyingClubMemberDoc is not null, "Didn't find valid test data for the test execution");

            string parentMemberNumber = flyingClubMemberDoc.MemberNumber;

            string childMemberNumber = "9106564728"; //Valid FC Member Whose Current Email Valid Flag is not Y

            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, parentMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMenuBarOnMemberProfilePage(logger);

            householdPage.ClickOnRelationShipTab(logger);

            householdPage.ClickOnLinkButton();

            householdPage.EnterChildMemberNumber(childMemberNumber);

            householdPage.ClickOnChildMemberSearchButton();

            householdPage.ClickOnValidateAndLink();

            string actualMessage = householdPage.GetPopUpMessage();

            string expectedMessage = "The email address of member account you are trying to link is invalid.";

            Assert.True(actualMessage.Equals(expectedMessage), $"Expected error message to be {expectedMessage} but found {actualMessage}.");

        }

        [Trait("Category", "HouseholdUIRegression-Not-A-Pipeline-Candidate")]
        [Fact, RunOrder(15)]
        public void TC15_ValidateErrorMessageOnLinkMemberScreenForSilverorGoldTierMembersIfChildMemberHasNotAcceptedFCTermsAndConditions()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC15_ValidateErrorMessageOnLinkMemberScreenForSilverorGoldTierMembersIfChildMemberHasNotAcceptedFCTermsAndConditions");

            #region Test Data

            string queryForFcmDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode in ( @tierCode , @tierCode1)  and c.Status = @status and c.GroupID = '' and c.HeadOfGroupMemberNumber = '' and c.TermsAndConditionsFlag  = @flag order by c._ts desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForFcmDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "Gold").WithParameter("@tierCode1", "Silver").WithParameter("@status", "Active").WithParameter("@flag", "Y");

            FlyingClubMember flyingClubMemberDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(paramQuery);

            Assert.True(flyingClubMemberDoc is not null, "Didn't find valid test data for the test execution");

            string parentMemberNumber = flyingClubMemberDoc.MemberNumber;

            string childMemberNumber = "9106562912"; //Valid FC Member Whose FC Terms and Conditions is not Y

            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, parentMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMenuBarOnMemberProfilePage(logger);

            householdPage.ClickOnRelationShipTab(logger);

            householdPage.ClickOnLinkButton();

            householdPage.EnterChildMemberNumber(childMemberNumber);

            householdPage.ClickOnChildMemberSearchButton();

            householdPage.ClickOnValidateAndLink();

            string actualMessage = householdPage.GetPopUpMessage();

            string expectedMessage = "Terms and Conditions have not been signed on the member account you are trying to link members to.";

            Assert.True(actualMessage.Equals(expectedMessage), $"Expected error message to be {expectedMessage} but found {actualMessage}.");
        }

        [Trait("Category", "HouseholdUIRegression-Not-A-Pipeline-Candidate")]
        [Fact, RunOrder(16)]
        public void TC16_ValidateErrorMessageOnLinkMemberScreenForSilverorGoldTierMembersWhileLinkingNewChildWhenTheHeadAccountHas9ActiveChildAccountsAlreadyLinkedToHimself()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC16_ValidateErrorMessageOnLinkMemberScreenForSilverorGoldTierMembersWhileLinkingNewChildWhenTheHeadAccountHas9ActiveChildAccountsAlreadyLinkedToHimself");

            #region Test Data

            string queryForFcmDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode in ( @tierCode , @tierCode1)  and c.Status = @status and c.GroupID = '' and c.HeadOfGroupMemberNumber = '' and c.TermsAndConditionsFlag  = @flag order by c._ts desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForFcmDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "Gold").WithParameter("@tierCode1", "Silver").WithParameter("@status", "Active").WithParameter("@flag", "Y");

            FlyingClubMember flyingClubMemberDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(paramQuery);

            Assert.True(flyingClubMemberDoc is not null, "Didn't find valid test data for the test execution");

            string parentMemberNumber = flyingClubMemberDoc.MemberNumber;

            string childMemberNumber = ""; //Valid HOh Member whose has 9 active child accounts linked

            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, parentMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMenuBarOnMemberProfilePage(logger);

            householdPage.ClickOnRelationShipTab(logger);

            householdPage.ClickOnLinkButton();

            householdPage.EnterChildMemberNumber(childMemberNumber);

            householdPage.ClickOnChildMemberSearchButton();

            householdPage.ClickOnValidateAndLink();

            string actualMessage = householdPage.GetPopUpMessage();

            string expectedMessage = "A household cannot exceed more than 10 linked members.";

            Assert.True(actualMessage.Equals(expectedMessage), $"Expected error message to be {expectedMessage} but found {actualMessage}.");

        }

        [Trait("Category", Traits.HouseHoldUIRegression)]
        [Fact, RunOrder(17)]
        public void TC17_ValidateThatTheDeLinkButtonIsDisabledForSilverAndGoldMembersWhenNoActiveChildMembersAreLinked()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC17_ValidateThatTheDeLinkButtonIsDisabledForSilverAndGoldMembersWhenNoActiveChildMembersAreLinked");

            #region Test Data

            string queryForFcmDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode in ( @tierCode , @tierCode1)  and c.Status = @status and c.GroupID = '' and c.HeadOfGroupMemberNumber = '' and c.TermsAndConditionsFlag  = @flag order by c._ts desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForFcmDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "Gold").WithParameter("@tierCode1", "Silver").WithParameter("@status", "Active").WithParameter("@flag", "Y");

            FlyingClubMember flyingClubMemberDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(paramQuery);

            Assert.True(flyingClubMemberDoc is not null, "Didn't find valid test data for the test execution");

            string memberNumber = flyingClubMemberDoc.MemberNumber;

            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMenuBarOnMemberProfilePage(logger);

            householdPage.ClickOnRelationShipTab(logger);

            Assert.True(householdPage.IsDeLinkButtonDisabled(), "Expected the Delink button for the Silver or Gold Member to be in disabled State but found Enabled");

        }

        [Trait("Category", Traits.HouseHoldUIRegression)]
        [Fact, RunOrder(18)]
        public void TC18_ValidatePromptMessageWhenMemberTriesToDelinkAChildAccountWhoseAssociationPeriodIsLessThanAYearForSuperAndIntermediatePlusUser()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC18_ValidatePromptMessageWhenMemberTriesToDelinkAChildAccountWhoseAssociationPeriodIsLessThanAYearForSuperAndIntermediatePlusUser");

            #region Test Data

            string queryForFCMDoc = "SELECT Top 1 * FROM  c where c.DocType = @docType  and c.Status = @status  and  c.MemberType = @memberType and c.HeadOfGroupMemberNumber!='' order by c.LastUpdatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@status", "Active").WithParameter("@memberType", "Individual");

            FlyingClubMember flyingClubDocument = cosmosFixture.CosmosQueryItem<FlyingClubMember>(paramQuery);

            if (flyingClubDocument is null)
            {
                throw new Xunit.Sdk.XunitException($"No Document available for the Searched Query {queryForFCMDoc}.");
            }

            string headAccountMemberNumber = flyingClubDocument.HeadOfGroupMemberNumber;

            string subOrdinateAccountMemberNumber = flyingClubDocument.MemberNumber;

            string queryForCustomerSearchDoc = "SELECT Top 1 * FROM  c where c.DocType='CustomerSearch'  and c.LoyaltyMemberNumber= @headAccountMemberNumber";

            QueryDefinition CustomerParamQuery = new QueryDefinition(query: queryForCustomerSearchDoc).WithParameter("@headAccountMemberNumber", headAccountMemberNumber);

            CustomerSearch customer = cosmosFixture.CosmosQueryItem<CustomerSearch>(CustomerParamQuery);

            if (customer is null)
            {
                throw new Xunit.Sdk.XunitException($"No Document available for the Searched Query {queryForCustomerSearchDoc}.");
            }

            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, headAccountMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMenuBarOnMemberProfilePage(logger);

            householdPage.ClickOnRelationShipTab(logger);

            householdPage.ClickOnDeLinkButton();

            string actualMessage = householdPage.GetPopUpMessage();

            string expectedMessage = "The account should be linked for a minimum of 12 months. Please select the override box and delink the account if it was linked in error.";

            Assert.True(actualMessage.Equals(expectedMessage), $"Expected error message to be {expectedMessage} but found {actualMessage}.");

        }

        [Trait("Category", "HouseholdUIRegression-Not-A-Pipeline-Candidate")]
        [Fact, RunOrder(19)]
        public void TC19_ValidateThatTheOverrideFlagIsDisabledForSuperAndIntermediatePlusUsersWhentheActiveChildAssociationPeriodIsEqualtoOrGreaterThan1year()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC19_ValidateThatTheOverrideFlagIsDisabledForSuperAndIntermediatePlusUsersWhentheActiveChildAssociationPeriodIsEqualtoOrGreaterThan1year");

            string parentMemberNumber = "1026070589";//A valid HoH Member

            string childMemberNumber = string.Empty;

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, parentMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMenuBarOnMemberProfilePage(logger);

            householdPage.ClickOnRelationShipTab(logger);

            int countOfActiveChildMembers = webDriver.FindElements(By.XPath(Household.MemberIdActive)).Count;

            for (int i = 1; i <= countOfActiveChildMembers; i++)
            {
                DateTime associationDate = DateTime.Parse(webDriver.GetElementText(Household.MemberAssociationDateActive + "[" + i + "]"));

                DateTime currentDate = DateTime.UtcNow.Date;

                childMemberNumber = webDriver.GetElementText(Household.MemberIdActive + "[" + i + "]");

                int diff = Convert.ToInt32(currentDate.Subtract(associationDate).TotalDays);

                if (diff >= 365)
                {
                    string btnStatus = webDriver.GetAttributeTextValue(Household.MemberOverrideCheckBoxActive + "[" + i + "]", "disabled");

                    Assert.True(btnStatus.Equals("true"), $"Expected the Override Flag Checkbox status to be disabled but found Enabled for the Child {childMemberNumber}.");
                }

            }

        }

        [Trait("Category", Traits.HouseHoldUIRegression)]
        [Fact, RunOrder(20)]
        public void TC20_ValidateThatAddressUpdateButtonOnProfileUpdatePageIsDisabledForActiveChildMembers()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC20_ValidateThatAddressUpdateButtonOnProfileUpdatePageIsDisabledForActiveChildMembers");

            #region Test Data

            string queryForGroupDocument = "SELECT TOP 5 * FROM CustomerDetailsCollection c WHERE  c.DocType = @docType order by c.CreatedOn desc";

            QueryDefinition groupDocParamQuery = new QueryDefinition(query: queryForGroupDocument).WithParameter("@docType", "Group");

            IEnumerable<Group> groupDocs = cosmosFixture.CosmosQueryItems<Group>(groupDocParamQuery);

            string childCustomerId = string.Empty;

            childCustomerId = groupDocs?.FirstOrDefault(x => x.GroupHistory.Any(c => string.IsNullOrEmpty(c.ExitDate)))?.GroupHistory.FirstOrDefault(c => string.IsNullOrEmpty(c.ExitDate)).CustomerId;

            Assert.True(!string.IsNullOrEmpty(childCustomerId), "Child CustomerId Is Empty. Test Data Not found");

            string queryForCustomerDoc = "SELECT * FROM CustomerCollection c where c.DocType= @docType and c.PartitionKey = @customerId";

            QueryDefinition customerDocParamQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@customerId", childCustomerId);

            Customer custDoc = cosmosFixture.CosmosQueryItem<Customer>(customerDocParamQuery);

            string childMemberNumber = custDoc.LoyaltyMemberNumber;


            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, childMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMenuBarOnMemberProfilePage(logger);

            memberProfilePage.ClickOnMemberProfileEditButtonToNavigateToProfileUpdatePage(logger);

            bool isAddressUpdateButtonDisabled = memberProfilePage.IsAddressDetailsUpdateButtonDisabled();

            Assert.True(isAddressUpdateButtonDisabled,
                $"For the Household Active Child Member {childMemberNumber} Expected the Address Update/Edit Button in disabled State but found enabled ");

        }

        [Trait("Category", "HouseholdUIRegression-Not-A-Pipeline-Candidate")]
        [Fact, RunOrder(21)]
        public void TC21_ValidatePromptMessageWhenMemberTriesToDelinkAChildAccountFromHoHAccountForIntermediateAndReadAndLightPlusUser()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC21_ValidatePromptMessageWhenMemberTriesToDelinkAChildAccountFromHoHAccountForForIntermediateAndReadAndLightPlusUser");

            #region Test Data

            string queryForFCMDoc = "SELECT Top 1 * FROM  c where c.DocType = @docType  and c.Status = @status  and  c.MemberType = @memberType and c.HeadOfGroupMemberNumber !='' and c.GroupId != ''order by c.LastUpdatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@status", "Active").WithParameter("@memberType", "Individual");

            FlyingClubMember flyingClubDocument = cosmosFixture.CosmosQueryItem<FlyingClubMember>(paramQuery);

            Assert.True(flyingClubDocument is not null, $"No Document available for the Searched Query {queryForFCMDoc}.");

            string headAccountMemberNumber = flyingClubDocument.HeadOfGroupMemberNumber;

            string queryForCustomerSearchDoc = "SELECT Top 1 * FROM  c where c.DocType='CustomerSearch'  and c.LoyaltyMemberNumber= @headAccountMemberNumber";

            QueryDefinition CustomerParamQuery = new QueryDefinition(query: queryForCustomerSearchDoc).WithParameter("@headAccountMemberNumber", headAccountMemberNumber);

            CustomerSearch customer = cosmosFixture.CosmosQueryItem<CustomerSearch>(CustomerParamQuery);

            Assert.True(customer is not null, $"No Document available for the Searched Query {queryForCustomerSearchDoc}.");

            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, headAccountMemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            memberProfilePage.ClickOnMenuBarOnMemberProfilePage(logger);

            householdPage.ClickOnRelationShipTab(logger);

            householdPage.ClickOnDeLinkButton();

            string actualMessage = householdPage.GetPopUpMessage();

            string expectedMessage = "You do not have enough privileges to delink this account. Please contact Customer Solutions to get the account delinked.";

            Assert.True(actualMessage.Equals(expectedMessage), $"Expected error message to be {expectedMessage} but found {actualMessage}.");

        }

        public void Dispose()
        {
            // Flush the report
            reportFixture.Current.Flush();
            // Close browser
            webDriver.CloseBrowser();
        }
    }
}
