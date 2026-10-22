using AventStack.ExtentReports;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Helpers;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.FrontendAPI;
using VAA.EAPI.Automation.MarketingLoyalty.UI.PageObjects;
using static VAA.EAPI.Automation.Core.UI.Factory.BrowserFactory;
using Traits = VAA.EAPI.Automation.MarketingLoyalty.UI.Constants.Traits;

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.Tests
{

    [Collection("FlightAccuralsTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class RetailTherapyTests : IDisposable, IClassFixture<ReportFixture>, IClassFixture<CosmosFixture>, IClassFixture<TokenFixture>
    {
        private ReportFixture reportFixture;
        private CosmosFixture cosmosFixture;
        private readonly IWebDriver webDriver;
        private readonly TokenFixture tokenFixture;

        PreRequisiteTestData preRequisiteTestData;

        public static string env = Config.Environment;
        private readonly MemberSearchPage memberSearchPage;
        private readonly MemberTransactionDetailPage memberTransactionDetailPage;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;
        private static string memberPassword = Application.LogInPassword;

        public RetailTherapyTests(CosmosFixture cosmosFixture, TokenFixture tokenFixture, ReportFixture reportFixture)
        {
            this.reportFixture = reportFixture;
            this.cosmosFixture = cosmosFixture;
            this.tokenFixture = tokenFixture;
            preRequisiteTestData = new();

            webDriver = GetBrowser(BrowserType.Chrome, ConfigReader.Get("PowerAppsUrl"));

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<Core.DB_Model.Entities.Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
            this.cosmosFixture.RegisterType<CustomerSearch>(cosmosEndpoint, customerCosmosDb, "CustomerSearch");
            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.tokenFixture.AddTokenForVRedFromKeyVault("vred");
            this.tokenFixture.AddTokenForAzureB2CFromKeyVault("azureb2c");
            this.tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");

            memberSearchPage = new MemberSearchPage(webDriver);
            memberTransactionDetailPage = new MemberTransactionDetailPage(webDriver);

        }

        [Trait("Category", Traits.RetailTherapyUIRegression)]
        [Fact, RunOrder(1)]
        public async Task TC01_RetrieveRetailTherapyEarnTxnSuccessScenario()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC01_RetrieveRetailTherapyEarnTxnSuccessScenario");

            #region Pre-Requisite for creating a txn for the member for the retrieval on PA
            #region PreRequisite to create an enrolment and Token generation before validations for the enrolment

            string[] titles = new string[] { "Mr", "Mrs", "Ms", "Miss", "Mstr" };
            Random random = new();
            string title = titles[random.Next(titles.Length)];
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomFirstName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@test.com";
            string postCode = "RH11 9LG";
            string country = "GB";
            string channelID = "WEB-MYA";
            string enroledBy = DataGenerator.RandomFirstName();
            string dateOfBirth = DateTime.UtcNow.AddDays(-6600).ToString("yyyy-MM-dd");

            HttpResponseMessage enrolmentResponse = await PreRequisiteTestData.MemberEnrolmentThrGraphql(channelID, title, firstName, lastName, emailId, postCode, country, dateOfBirth, enroledBy);

            JObject enrolmentResult = (JObject)JsonConvert.DeserializeObject<dynamic>(enrolmentResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(enrolmentResult["data"]["accountEnrolMember"]["customerId"] != null, "customer id should be not blank");

            string customerId = enrolmentResult["data"]["accountEnrolMember"]["customerId"].ToString();

            string fcMember = enrolmentResult["data"]["accountEnrolMember"]["memberDetails"]["memberNumber"].ToString();

            Assert.True(!string.IsNullOrEmpty(customerId), "customerId returned from enrolment response is null/empty.");

            #endregion

            #region generating B2C token for request

            GetCustomerDetailsFromFcNumber(customerId, out Core.DB_Model.Entities.Customer objCustomer, out long expiryValue);

            string tokenForMemberAccountDetails = preRequisiteTestData.GenerateLoginB2CTokenThroughMYAURL(fcMember, memberPassword);

            #endregion

            #region Creating - CreateEarnTransactionForRetailTherapy for loyalty accounts

            DateTime activityPastDate = DataGenerator.RandomPastDate(50);

            string activityDate = activityPastDate.ToString("MM/dd/yyyy");

            string orderNumber = DataGenerator.RandomString(6, true).ToUpper();

            int points = Int32.Parse(DataGenerator.RandomNumber(0, 99));

            HttpResponseMessage response = GraphQL.CreateEarnTransactionForRetailTherapy(tokenFixture.GetToken("graphqlv2"), tokenForMemberAccountDetails, activityDate, "RetailTherapy",
                                                                                            "TestingRetailTherapyTxns", objCustomer.LoyaltyMemberNumber, orderNumber, points);
            #endregion


            #region Validations

            dynamic createTransactionResult = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(createTransactionResult.data.loyaltyCreateEarnTransaction.orderId.ToString().Equals(orderNumber),
                $"The CreateEarnTransactionForRetailTherapy was not created as expected. Expected the orderNumber to be" +
                $" {orderNumber}  , but was {createTransactionResult.ToString()}");

            Assert.True(createTransactionResult.data.loyaltyCreateEarnTransaction.vaaTransactionId.ToString().Contains("AZ"),
                $"The CreateEarnTransactionForRetailTherapy doesnot contain vaaTransactionId with AZ number" +
                $" but was {createTransactionResult.ToString()}");

            string vredTransactionId = createTransactionResult.data.loyaltyCreateEarnTransaction.vaaTransactionId.ToString();

            #endregion

            #endregion- for Pre-Requisite for creating a txn for the member for the retrieval on PA

            #region PowerApps navigation

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, fcMember);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            bool flag = false;

            IEnumerable<IWebElement> txnList = webDriver.FindElements(By.XPath(ElementsLibrary.MemberProfile.MemberDetailTxnList));

            foreach (IWebElement element in txnList)
            {

                string txnType = element.FindElement(By.XPath("td[3]/div/div/span")).Text;

                string pointsOnPA = element.FindElement(By.XPath("td[11]/div/div/span")).Text;


                if (txnType.Equals("Accrual") && pointsOnPA.Equals(points.ToString()))
                {
                    string actualActivityDate = element.FindElement(By.XPath("td[1]/div/div/span")).Text;

                    string actualTxnDate = element.FindElement(By.XPath("td[2]/div/div/span")).Text;

                    string actualTxnType = element.FindElement(By.XPath("td[3]/div/div/span")).Text;

                    string actualPartnerCode = element.FindElement(By.XPath("td[4]/div/div/span")).Text;

                    string actualTierPoints = element.FindElement(By.XPath("td[10]/div/div/span")).Text;

                    string actualPoints = element.FindElement(By.XPath("td[11]/div/div/span")).Text;

                    string actualStatus = element.FindElement(By.XPath("td[12]/div/div/span")).Text;

                    string actualProduct = element.FindElement(By.XPath("td[13]/div/div/span")).Text;

                    Assert.True(activityPastDate.ToString("dd-MMM-yyyy").Equals(actualActivityDate),
                        $"Expected the activityDate to be {activityPastDate.ToString("dd-MMM-yyyy")} but found {actualActivityDate}.");

                    string currentDate = DateTime.UtcNow.ToString("dd-MMM-yyyy");

                    Assert.True(currentDate.Equals(actualTxnDate),
                        $"Expected the TransactionDate to be {currentDate} but found {actualTxnDate}.");

                    Assert.True("Accrual".Equals(actualTxnType),
                        $"Expected the TransactionType to be Accrual but found {actualTxnType}.");

                    Assert.True(!string.IsNullOrEmpty(actualTierPoints),
                      $"Expected valid Tier Points value but found null or empty value.(Txn Sync issue)");

                    Assert.True(!string.IsNullOrEmpty(actualPoints),
                     $"Expected valid Points value but found null or empty value.(Txn Sync issue)");

                    Assert.True(points.ToString().Equals(actualPoints),
                        $"Expected the points value to be {points.ToString()} but found {actualPoints}.");

                    Assert.True("0".Equals(actualTierPoints),
                            $"Expected the tier points value to be 0 but found {actualTierPoints}.");

                    Assert.True("Processed".Equals(actualStatus),
                        $"Expected the Transaction Status to be Processed but found {actualStatus}.");

                    Assert.True("VAA Duty Free".Equals(actualProduct),
                       $"Expected the Transaction Product to be Flight but found {actualProduct}.");

                    element.FindElement(By.XPath("td[2]/div/div/span")).Click();

                    memberTransactionDetailPage.RetrieveTxnFromMemberDetailsTransactionSection(logger);

                    Assert.True(memberTransactionDetailPage.RetrieveVredTxnId().Equals(vredTransactionId),
                            $"Expected the VredTransactionId text to be {vredTransactionId} but found {memberTransactionDetailPage.RetrieveVredTxnId()}");

                    Assert.True(memberTransactionDetailPage.RetrieveComment().Equals("TestingRetailTherapyTxns" + orderNumber),
                            $"Expected the comment tect on the PA to be {"TestingRetailTherapyTxns" + orderNumber} but found {memberTransactionDetailPage.RetrieveComment()}");

                    flag = true;

                    break;
                }
            }
            Assert.True(flag, "The Transaction not present in the Transaction history (Member Detail Page)");

            #endregion PowerApps Navigation
        }


        [Trait("Category", Traits.RetailTherapyUIRegression)]
        [Fact, RunOrder(2)]
        public async Task TC02_RetrieveRetailTherapyBurnTxnSuccessScenario()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC02_RetrieveRetailTherapyBurnTxnSuccessScenario");

            #region Pre-Requisite for creating a txn for the member for the retrieval on PA

            #region PreRequisite to create an enrolment and Token generation before validations for the enrolment

            string[] titles = new string[] { "Mr", "Mrs", "Ms", "Miss", "Mstr" };
            Random random = new();
            string title = titles[random.Next(titles.Length)];
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomFirstName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@test.com";
            string postCode = "RH11 9LG";
            string country = "GB";
            string channelID = "WEB-MYA";
            string enroledBy = DataGenerator.RandomFirstName();
            string dateOfBirth = DateTime.UtcNow.AddDays(-6600).ToString("yyyy-MM-dd");

            HttpResponseMessage enrolmentResponse = await PreRequisiteTestData.MemberEnrolmentThrGraphql(channelID, title, firstName, lastName, emailId, postCode, country, dateOfBirth, enroledBy);

            JObject enrolmentResult = (JObject)JsonConvert.DeserializeObject<dynamic>(enrolmentResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(enrolmentResult["data"]["accountEnrolMember"]["customerId"] != null, "customer id should be not blank");

            string customerId = enrolmentResult["data"]["accountEnrolMember"]["customerId"].ToString();

            string fcMember = enrolmentResult["data"]["accountEnrolMember"]["memberDetails"]["memberNumber"].ToString();

            Assert.True(!string.IsNullOrEmpty(customerId), "customerId returned from enrolment response is null/empty.");

            #endregion

            #region generating B2C token for request

            GetCustomerDetailsFromFcNumber(customerId, out Core.DB_Model.Entities.Customer objCustomer, out long expiryValue);

            string tokenForMemberAccountDetails = preRequisiteTestData.GenerateLoginB2CTokenThroughMYAURL(fcMember, memberPassword);

            #endregion


            #region awarding sufficient points to enable redemptions

            DateTime activityPastDate = DataGenerator.RandomPastDate(10);

            string activityDt = activityPastDate.ToString("dd-MMM-yyyy");

            int availablePoints = Convert.ToInt32(DataGenerator.RandomNumber(100, 200, true));

            HttpResponseMessage txnresponse = LoopsFrontEndAPIEndpoints.ManualCreditAdjustment(fcMember, "VS", "Authorised Points Credit", activityDt, "Test txn", availablePoints, 0, "AutomationScript", "AutomationScript@test.com", "AutomationScript", customerId).EnsureIsSuccessStatusCode();

            string txnResult = txnresponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            CreditAdjustmentResponse manualAdjustmentResponse = JsonConvert.DeserializeObject<CreditAdjustmentResponse>(txnResult);

            Assert.True(manualAdjustmentResponse is null, $"Pre-requsite Transaction for awarding pts to the member not processed with message {manualAdjustmentResponse?.message}");

            #endregion 

            #region Creating - CreateBurnTransactionForRetailTherapy for loyalty accounts and Validations

            DateTime activityFutureDate = DataGenerator.RandomFutureDate(5);

            string activityDate = activityFutureDate.ToString("MM/dd/yyyy");

            string orderNumber = DataGenerator.RandomString(6, true).ToUpper();

            int points = Int32.Parse(DataGenerator.RandomNumber(0, 50));

            HttpResponseMessage response = GraphQL.CreateBurnTransactionForRetailTherapy(tokenFixture.GetToken("graphqlv2"), tokenForMemberAccountDetails, activityDate, "RetailTherapy", "TestingRetailTherapyTxns",
                                                                                           objCustomer.LoyaltyMemberNumber, orderNumber, points);

            dynamic createTransactionResult = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(createTransactionResult.data.loyaltyCreateBurnTransaction.orderId.ToString().Equals(orderNumber),
                $"The CreateEarnTransactionForRetailTherapy was not created as expected. Expected the orderNumber to be" +
                $" {orderNumber}  , but was {createTransactionResult.ToString()}");

            Assert.True(createTransactionResult.data.loyaltyCreateBurnTransaction.vaaTransactionId.ToString().Contains("AZ"),
                $"The CreateEarnTransactionForRetailTherapy doesnot contain vaaTransactionId with AZ number" +
                $" but was {createTransactionResult.ToString()}");

            string vredTransactionId = createTransactionResult.data.loyaltyCreateBurnTransaction.vaaTransactionId.ToString();

            #endregion

            #endregion- for Pre-Requisite for creating a txn for the member for the retrieval on PA

            #region PowerApps Navigation

            memberSearchPage.SwitchToPowerAppIFrame();
            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, fcMember);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            bool flag = false;

            IEnumerable<IWebElement> txnList = webDriver.FindElements(By.XPath(ElementsLibrary.MemberProfile.MemberDetailTxnList));

            foreach (IWebElement element in txnList)
            {

                string txnType = element.FindElement(By.XPath("td[3]/div/div/span")).Text;

                string pointsOnPA = element.FindElement(By.XPath("td[11]/div/div/span")).Text;

                pointsOnPA = pointsOnPA.Has("(") ? pointsOnPA.Replace("(", "").Replace(")", "") : pointsOnPA;

                if (txnType.Equals("Redemption") && pointsOnPA.Equals(points.ToString()))
                {
                    string actualActivityDate = element.FindElement(By.XPath("td[1]/div/div/span")).Text;

                    string actualTxnDate = element.FindElement(By.XPath("td[2]/div/div/span")).Text;

                    string actualTxnType = element.FindElement(By.XPath("td[3]/div/div/span")).Text;

                    string actualPartnerCode = element.FindElement(By.XPath("td[4]/div/div/span")).Text;

                    string actualTierPoints = element.FindElement(By.XPath("td[10]/div/div/span")).Text;

                    actualTierPoints = actualTierPoints.Replace("(", "").Replace(")", "");

                    string actualPoints = element.FindElement(By.XPath("td[11]/div/div/span")).Text;

                    actualPoints = actualPoints.Replace("(", "").Replace(")", "");

                    string actualStatus = element.FindElement(By.XPath("td[12]/div/div/span")).Text;

                    string actualProduct = element.FindElement(By.XPath("td[13]/div/div/span")).Text;

                    Assert.True(activityFutureDate.ToString("dd-MMM-yyyy").Equals(actualActivityDate),
                        $"Expected the activityDate to be {activityFutureDate.ToString("dd-MMM-yyyy")} but found {actualActivityDate}.");

                    string currentDate = DateTime.UtcNow.ToString("dd-MMM-yyyy");

                    Assert.True(currentDate.Equals(actualTxnDate),
                        $"Expected the TransactionDate to be {currentDate} but found {actualTxnDate}.");

                    Assert.True("Redemption".Equals(actualTxnType),
                        $"Expected the TransactionType to be Accrual but found {actualTxnType}.");

                    Assert.True(!string.IsNullOrEmpty(actualTierPoints),
                      $"Expected valid Tier Points value but found null or empty value.(Txn Sync issue)");

                    Assert.True(!string.IsNullOrEmpty(actualPoints),
                     $"Expected valid Points value but found null or empty value.(Txn Sync issue)");

                    Assert.True(points.ToString().Equals(actualPoints),
                        $"Expected the points value to be {points.ToString()} but found {actualPoints}.");

                    Assert.True("0".Equals(actualTierPoints),
                            $"Expected the tier points value to be 0 but found {actualTierPoints}.");

                    Assert.True("Processed".Equals(actualStatus),
                        $"Expected the Transaction Status to be Processed but found {actualStatus}.");

                    Assert.True("VAA Duty Free".Equals(actualProduct),
                       $"Expected the Transaction Product to be Flight but found {actualProduct}.");

                    element.FindElement(By.XPath("td[2]/div/div/span")).Click();

                    memberTransactionDetailPage.RetrieveTxnFromMemberDetailsTransactionSection(logger);

                    Assert.True(memberTransactionDetailPage.RetrieveVredTxnId().Equals(vredTransactionId),
                            $"Expected the VredTransactionId text to be {vredTransactionId} but found {memberTransactionDetailPage.RetrieveVredTxnId()}");

                    Assert.True(memberTransactionDetailPage.RetrieveComment().Equals("TestingRetailTherapyTxns" + orderNumber),
                            $"Expected the comment tect on the PA to be {"TestingRetailTherapyTxns" + orderNumber} but found {memberTransactionDetailPage.RetrieveComment()}");

                    flag = true;

                    break;
                }
            }
            Assert.True(flag, "The Transaction not present in the Transaction history (Member Detail Page)");

            #endregion PowerApps Navigation
        }

        [Trait("Category", Traits.RetailTherapyUIRegression)]
        [Fact, RunOrder(3)]
        public async Task TC03_RetrieveRetailTherapyCancelTxnSuccessScenario()
        {

            ExtentTest logger = reportFixture.Current.CreateTest("TC03_RetrieveRetailTherapyCancelTxnSuccessScenario");

            #region PreRequisite to create an enrolment and Token generation before validations for the enrolment

            string[] titles = new string[] { "Mr", "Mrs", "Ms", "Miss", "Mstr" };
            Random random = new();
            string title = titles[random.Next(titles.Length)];
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomFirstName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@test.com";
            string postCode = "RH11 9LG";
            string country = "GB";
            string channelID = "WEB-MYA";
            string enroledBy = DataGenerator.RandomFirstName();
            string dateOfBirth = DateTime.UtcNow.AddDays(-6600).ToString("yyyy-MM-dd");

            HttpResponseMessage enrolmentResponse = await PreRequisiteTestData.MemberEnrolmentThrGraphql(channelID, title, firstName, lastName, emailId, postCode, country, dateOfBirth, enroledBy);

            JObject enrolmentResult = (JObject)JsonConvert.DeserializeObject<dynamic>(enrolmentResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(enrolmentResult["data"]["accountEnrolMember"]["customerId"] != null, "customer id should be not blank");

            string customerId = enrolmentResult["data"]["accountEnrolMember"]["customerId"].ToString();

            string fcMember = enrolmentResult["data"]["accountEnrolMember"]["memberDetails"]["memberNumber"].ToString();

            Assert.True(!string.IsNullOrEmpty(customerId), "customerId returned from enrolment response is null/empty.");

            #endregion

            #region generating B2C token for request

            GetCustomerDetailsFromFcNumber(customerId, out Core.DB_Model.Entities.Customer objCustomer, out long expiryValue);

            string tokenForMemberAccountDetails = preRequisiteTestData.GenerateLoginB2CTokenThroughMYAURL(fcMember, memberPassword);

            #endregion
            #region Creating - CreateEarnTransactionForRetailTherapy for loyalty accounts

            DateTime activityPastDate = DataGenerator.RandomPastDate(50);

            string activityDate = activityPastDate.ToString("MM/dd/yyyy");

            string orderNumber = DataGenerator.RandomString(6, true).ToUpper();

            int points = Int32.Parse(DataGenerator.RandomNumber(0, 99));

            HttpResponseMessage responseEarnTxn = GraphQL.CreateEarnTransactionForRetailTherapy(tokenFixture.GetToken("graphqlv2"), tokenForMemberAccountDetails, activityDate, "RetailTherapy", "TestingRetailTherapyTxns",
                                                                               objCustomer.LoyaltyMemberNumber, orderNumber, points).EnsureIsSuccessStatusCode();
            #endregion

            #region Validations

            dynamic createTransactionResult = JsonConvert.DeserializeObject<dynamic>(responseEarnTxn.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(createTransactionResult.data.loyaltyCreateEarnTransaction.orderId.ToString().Equals(orderNumber),
                $"The CreateEarnTransactionForRetailTherapy was not created as expected. Expected the orderNumber to be {orderNumber} ,but was {createTransactionResult.ToString()}");

            string transactionId = createTransactionResult.data.loyaltyCreateEarnTransaction.vaaTransactionId.ToString();

            #endregion

            #region Validations

            Assert.True(createTransactionResult.data.loyaltyCreateEarnTransaction.orderId.ToString().Equals(orderNumber),
                $"The CreateEarnTransactionForRetailTherapy was not created as expected. Expected the orderNumber to be {orderNumber}, but was {createTransactionResult.ToString()}");

            Assert.True(createTransactionResult.data.loyaltyCreateEarnTransaction.vaaTransactionId.ToString().Contains("AZ"),
                $"The CreateEarnTransactionForRetailTherapy doesnot contain vaaTransactionId with AZ number and actual is {createTransactionResult.ToString()}");

            string vredTransactionId = createTransactionResult.data.loyaltyCreateEarnTransaction.vaaTransactionId.ToString();

            #endregion

            #region cancel the earn txn created and Validations

            string cancellationReason = "Incorrect order placed";

            HttpResponseMessage cancelResponse = GraphQL.CancelTransactionForRetailTherapy(tokenFixture.GetToken("graphqlv2"), tokenForMemberAccountDetails, activityDate, cancellationReason, "RetailTherapy", objCustomer.LoyaltyMemberNumber, transactionId).EnsureIsSuccessStatusCode();

            dynamic cancelTransactionResult = JsonConvert.DeserializeObject<dynamic>(cancelResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());


            int retry = 3;

            while (retry > 0 && cancelTransactionResult.ToString().Contains("Invalid Transaction Id"))
            {
                Thread.Sleep(3000);

                cancelResponse = GraphQL.CancelTransactionForRetailTherapy(tokenFixture.GetToken("graphqlv2"), tokenForMemberAccountDetails, activityDate, cancellationReason,
                                                                                             "RetailTherapy", objCustomer.LoyaltyMemberNumber, transactionId).EnsureIsSuccessStatusCode();

                cancelTransactionResult = JsonConvert.DeserializeObject<dynamic>(cancelResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                retry--;
            }

            Assert.True(cancelTransactionResult.data.loyaltyCancelTransaction.message.ToString().Contains("Request processed successfully"),
                $"The loyaltyCancelTransactionForRetailTherapy was not created as expected. Expected the message to contain - Request processed successfully  , but was {createTransactionResult.ToString()}");


            #endregion

            #region PowerApps navigation

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, fcMember);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            bool flag = false;

            IEnumerable<IWebElement> txnList = webDriver.FindElements(By.XPath(ElementsLibrary.MemberProfile.MemberDetailTxnList));

            foreach (IWebElement element in txnList)
            {

                string txnType = element.FindElement(By.XPath("td[3]/div/div/span")).Text;

                string pointsOnPA = element.FindElement(By.XPath("td[11]/div/div/span")).Text;

                string actualActivityDate = element.FindElement(By.XPath("td[1]/div/div/span")).Text;

                if (txnType.Equals("Accrual") && pointsOnPA.Equals(points.ToString()) && actualActivityDate.Equals(activityPastDate.ToString("dd-MMM-yyyy")))
                {

                    string actualPartnerCode = element.FindElement(By.XPath("td[4]/div/div/span")).Text;

                    string actualTierPoints = element.FindElement(By.XPath("td[10]/div/div/span")).Text;

                    string actualStatus = element.FindElement(By.XPath("td[12]/div/div/span")).Text;

                    string actualProduct = element.FindElement(By.XPath("td[13]/div/div/span")).Text;

                    string actualStatementDescription = element.FindElement(By.XPath("td[14]/div/div/span")).Text;

                    Assert.True(!string.IsNullOrEmpty(actualTierPoints),
                      $"Expected valid Tier Points value but found null or empty value.(Txn Sync issue)");

                    Assert.True("0".Equals(actualTierPoints),
                            $"Expected the tier points value to be 0 but found {actualTierPoints}.");

                    Assert.True("Cancelled".Equals(actualStatus),
                        $"Expected the Transaction Status to be Processed but found {actualStatus}.");

                    Assert.True("VAA Duty Free".Equals(actualProduct),
                       $"Expected the Transaction Product to be Flight but found {actualProduct}.");

                    Assert.True("Virgin Atlantic Duty Free Shopping".Equals(actualStatementDescription),
                      $"Expected the Statement Description on PA: Virgin Atlantic Duty Free Shopping - but found {actualStatementDescription}.");

                    element.FindElement(By.XPath("td[2]/div/div/span")).Click();

                    memberTransactionDetailPage.RetrieveTxnFromMemberDetailsTransactionSectionWithCommentTextVisible(logger);

                    Assert.True(memberTransactionDetailPage.RetrieveVredTxnId().Equals(vredTransactionId),
                            $"Expected the VredTransactionId text to be {vredTransactionId} but found {memberTransactionDetailPage.RetrieveVredTxnId()}");

                    Assert.True(memberTransactionDetailPage.RetrieveComment().Equals("TestingRetailTherapyTxns" + orderNumber),
                            $"Expected the comment tect on the PA to be {"TestingRetailTherapyTxns" + orderNumber} but found {memberTransactionDetailPage.RetrieveComment()}");

                    flag = true;

                    break;
                }
            }
            Assert.True(flag, "The Transaction not present in the Transaction history (Member Detail Page)");

            #endregion PowerApps Navigation
        }

        private void GetCustomerDetailsFromFcNumber(string customerId, out Core.DB_Model.Entities.Customer objCustomer, out long expiryValue)
        {
            string strInputQueryForCustomerSearch = $"SELECT * FROM CustomerCollection c WHERE c.PartitionKey = '{customerId}' and c.DocType='Customer'";

            objCustomer = cosmosFixture.QueryItem<Core.DB_Model.Entities.Customer>(strInputQueryForCustomerSearch);
            int retry = 10;
            while (retry > 0 && objCustomer == null)
            {
                Thread.Sleep(5000);
                objCustomer = cosmosFixture.QueryItem<Core.DB_Model.Entities.Customer>(strInputQueryForCustomerSearch);
                retry--;
            }

            Assert.True(objCustomer != null, "Query from Cosmos DB for Customer Search is null after 10 re tries");

            expiryValue = ((DateTimeOffset)DateTime.UtcNow.AddDays(1)).ToUnixTimeSeconds();
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
