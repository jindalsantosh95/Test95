using AventStack.ExtentReports;
using Microsoft.Azure.Cosmos;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Helpers;
using VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary;
using VAA.EAPI.Automation.MarketingLoyalty.UI.PageObjects;
using static VAA.EAPI.Automation.Core.UI.Factory.BrowserFactory;
using FlyingClubMember = VAA.EAPI.Automation.Core.DB_Model.Entities.FlyingClubMember;
using Traits = VAA.EAPI.Automation.MarketingLoyalty.UI.Constants.Traits;

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.Tests
{

    [Collection("RedemptionTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class RedemptionTests : IDisposable, IClassFixture<ReportFixture>, IClassFixture<CosmosFixture>
    {
        private ReportFixture reportFixture;
        private CosmosFixture cosmosFixture;
        private IWebDriver webDriver;
        private MemberSearchPage memberSearchPage;
        private RedemptionPage redemptionPage;
        private MemberProfilePage memberProfilePage;
        private MyOffersPage myOffersPage;

        private readonly string customerTransactionCosmosDb;

        public string env = Config.Environment;

        public RedemptionTests(ReportFixture reportFixture, CosmosFixture cosmosFixture)
        {
            this.reportFixture = reportFixture;
            this.cosmosFixture = cosmosFixture;

            webDriver = GetBrowser(BrowserType.Chrome, ConfigReader.Get("PowerAppsUrl"));

            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<Transaction>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.cosmosFixture.RegisterType<Award>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            memberSearchPage = new MemberSearchPage(webDriver);

            redemptionPage = new RedemptionPage(webDriver);

            memberProfilePage = new MemberProfilePage(webDriver);

            myOffersPage = new MyOffersPage(webDriver);

        }

        [Trait("Category", Traits.RedemptionUIRegression)]
        [Theory, RunOrder(1)]
        [InlineData("Flight Redemption")]
        [InlineData("Flight Upgrade")]
        public void TC01_RedemptionOfPointsSuccessScenario(string product)
        {

            //manualAdjustmentTest.TC01_ManualAdjustmentForPointsSuccessScenario("VS");

            ExtentTest logger = reportFixture.Current.CreateTest($"TC01_RedemptionOfPointsSuccessScenarioForProduct{product}");

            string queryForTxnDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TransactionType = @txnType order by c.CreatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForTxnDoc).WithParameter("@docType", "Transaction").WithParameter("@txnType", "Accrual");

            Transaction txnDoc = cosmosFixture.CosmosQueryItem<Transaction>(paramQuery);

            string memberNumber = txnDoc.MemberNumber;

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            string memberPointBalance = webDriver.GetAttributeTextValue(MemberDetail.MemberPointBalance, "value");

            Assert.True(!memberPointBalance.Equals("0"),
                "The Member Point Balance is 0 .Redemption Transaction cannot be created");

            redemptionPage.NavigateToRedemptionPage();

            string points = DataGenerator.RandomNumber(1, 100, true);

            string activityDate = DataGenerator.RandomFutureDate(365).ToString("dd-MMM-yyyy");

            string partnerCode = "VS";

            string originAirportCode = "LHR";

            string destinationAirportCode = "JFK";

            string pnr = DataGenerator.RandomString(6, true).ToUpper();

            string bookingClass = "J";

            redemptionPage.EnterPartnerCode(partnerCode);

            redemptionPage.EnterProduct(product);

            redemptionPage.EnterActivityDate(activityDate);

            redemptionPage.EnterPoints(points);

            redemptionPage.EnterComments("Test Transaction");

            redemptionPage.EnterOriginAirportCode(originAirportCode);

            redemptionPage.EnterDestinationAirportCode(destinationAirportCode);

            redemptionPage.EnterPNR(pnr);

            redemptionPage.EnterBookingClass(bookingClass);

            redemptionPage.EnterNoOfPassengers("1");

            redemptionPage.EnterNoOfSegments("1");

            redemptionPage.ClickOnApplyRedemptionButton();

            string expectedMessage = "Successful Manual Redemption Applied";

            string actualMemberNumber = webDriver.GetElementText(Redemption.PopUpMessageContent + "/b[2]");

            string actualPointsRedeemed = webDriver.GetElementText(Redemption.PopUpMessageContent + "/b[3]");

            string actualActivityDate = webDriver.GetElementText(Redemption.PopUpMessageContent + "/b[4]");

            string actualPartnerCode = webDriver.GetElementText(Redemption.PopUpMessageContent + "/b[5]");

            string actualProduct = webDriver.GetElementText(Redemption.PopUpMessageContent + "/b[7]");

            string actualMessage = webDriver.GetElementText(Redemption.PopUpMessageContent + "/p/b");

            Assert.True(memberNumber.Equals(actualMemberNumber),
                $"Expected the FC Number on the Pop up to be {memberNumber} but found {actualMemberNumber}.");

            Assert.True(points.Equals(actualPointsRedeemed),
                $"Expected the Points Redeemed on the Pop up to be {points} but found {actualPointsRedeemed}.");

            Assert.True(activityDate.Equals(actualActivityDate),
                $"Expected the Activity Date on the Pop up to be {activityDate} but found {actualActivityDate}.");

            Assert.True(partnerCode.Equals(actualPartnerCode),
                $"Expected the PartnerCode on the Pop up to be {partnerCode} but found {actualPartnerCode}.");

            Assert.True(product.Equals(actualProduct),
                $"Expected the Product on the Pop up to be {product} but found {actualProduct}.");

            Assert.True(expectedMessage.Equals(actualMessage),
               $"Expected the Success Message on the Pop up to be {expectedMessage} but found {actualMessage}.");


        }



        [Trait("Category", Traits.RedemptionUIRegression)]
        [Theory, RunOrder(2)]
        [InlineData("product")]
        [InlineData("points")]
        [InlineData("activityDate")]
        [InlineData("origin")]
        [InlineData("destination")]
        [InlineData("bookingReference")]
        [InlineData("bookingClass")]
        [InlineData("noOfSegments")]
        [InlineData("noOfPassengers")]
        public void TC02_ApplyRedemptionButtonValidationWhenMandatoryFieldValuesAreMissing(string mandatoryField)
        {

            ExtentTest logger = reportFixture.Current.CreateTest($"TC02_ApplyRedemptionButtonValidationWhenMandatoryFieldValues-{mandatoryField}-AreMissing");

            string queryForTxnDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TransactionType = @txnType order by c.CreatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForTxnDoc).WithParameter("@docType", "Transaction").WithParameter("@txnType", "Accrual");

            Transaction txnDoc = cosmosFixture.CosmosQueryItem<Transaction>(paramQuery);

            string memberNumber = txnDoc.MemberNumber;

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            redemptionPage.NavigateToRedemptionPage();

            string points = DataGenerator.RandomNumber(1, 100, true);

            string product = "Flight Redemption";

            string activityDate = DataGenerator.RandomFutureDate(365).ToString("dd-MMM-yyyy");

            string partnerCode = "VS";

            string originAirportCode = "LHR";

            string destinationAirportCode = "JFK";

            string pnr = DataGenerator.RandomString(6, true).ToUpper();

            string bookingClass = "J";

            string noOfPassengers = "1";

            string noOfSegments = "1";


            switch (mandatoryField)
            {
                case "partnerCode":
                    partnerCode = "";
                    break;

                case "points":
                    points = "";
                    break;

                case "product":
                    product = "";
                    break;

                case "origin":
                    originAirportCode = "";
                    break;

                case "destination":
                    destinationAirportCode = "";
                    break;

                case "bookingReference":
                    pnr = "";
                    break;

                case "activityDate":
                    activityDate = "";
                    break;

                case "bookingClass":
                    bookingClass = "";
                    break;

                case "noOfSegments":
                    noOfSegments = "";
                    break;

                case "noOfPassengers":
                    noOfPassengers = "";
                    break;

                default:
                    break;

            }

            redemptionPage.EnterPartnerCode(partnerCode);

            redemptionPage.EnterProduct(product);

            redemptionPage.EnterActivityDate(activityDate);

            redemptionPage.EnterPoints(points);

            redemptionPage.EnterComments("Test Transaction");

            redemptionPage.EnterOriginAirportCode(originAirportCode);

            redemptionPage.EnterDestinationAirportCode(destinationAirportCode);

            redemptionPage.EnterPNR(pnr);

            redemptionPage.EnterBookingClass(bookingClass);

            redemptionPage.EnterNoOfPassengers(noOfPassengers);

            redemptionPage.EnterNoOfSegments(noOfSegments);

            Assert.True(redemptionPage.IsApplyRedemptionButtonDisabled(),
                $"Expected the Apply Button to be in disabled state when Mandatory field {mandatoryField} value is missing.");
        }


        [Trait("Category", Traits.RedemptionUIRegression)]
        [Theory, RunOrder(3)]
        [InlineData("Reward voucher redemption - Companion")]
        [InlineData("Reward voucher redemption - Upgrade")]
        public void TC03_NewVoucherRedemptionSuccessScenario(string voucherProduct)
        {

            ExtentTest logger = reportFixture.Current.CreateTest($"TC03_NewVoucherRedemptionSuccessScenario-{voucherProduct}");

            #region Fetching a Member who holds a Valid Voucher for redemption

            //Fetching only Redemption Eligible Vouchers(Awards) --Even VRedUSCreditCard Voucher -TBD
            string queryForAwardDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.ProductId not in ('1634561592','1634561747','1634561772','1-1ZKH','1718882825098','',null)  and c.Status in ('Available',' Partially Used') and IS_DEFINED(c.ExpiryDate) and c.ExpiryDate  not in (null,'') order by c.CreatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForAwardDoc).WithParameter("@docType", "Award");

            Award awardDoc = cosmosFixture.CosmosQueryItem<Award>(paramQuery);

            Assert.True(awardDoc is not null, "Didn't find Valid Award Doc for further Test Execution");

            string queryForVoucherDetails = $"Select ProductName from dbo.Product where RowId = '{awardDoc.ProductId}' ";

            string voucherName = (string)AzureReferenceDBHelper.RefDbQueryResponse(queryForVoucherDetails) ?? "";

            string voucherId = awardDoc.AwardNumber;

            string customerId = awardDoc.CustomerId;

            string queryForFlyingClubDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId ";

            QueryDefinition query = new QueryDefinition(query: queryForFlyingClubDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@customerId", customerId);

            FlyingClubMember flyingClubMemberDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(query);

            string memberNumber = flyingClubMemberDoc?.MemberNumber ?? "";

            string tierCode = flyingClubMemberDoc?.TierCode ?? "";

            #endregion

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, memberNumber);

            string[] currentUserDetails = webDriver.GetElementText(MemberSearch.CurrentUser)?.Split(' ');

            string currentLoggedInUser = currentUserDetails.Length > 0 ? $"{currentUserDetails[1]}.{currentUserDetails[2]}" : "";

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            string memberPointBalanceBeforeTxn = webDriver.GetAttributeTextValue(MemberDetail.MemberPointBalance, "value");

            Assert.True(memberPointBalanceBeforeTxn.TryParse<int>(out _), "Failed to read member point balance");

            redemptionPage.NavigateToRedemptionPage();

            #region Creating a Voucher Redemption Transaction and Validations

            string redemptionPoints = "0";

            string activityDate = DataGenerator.RandomFutureDate(365).ToString("dd-MMM-yyyy");

            string partnerCode = "VS";

            string originAirportCode = "LHR";

            string destinationAirportCode = "JFK";

            string pnr = DataGenerator.RandomString(6, true).ToUpper();

            string bookingClass = "J";

            string voucherPoints = tierCode.Is("Red") ? DataGenerator.RandomNumber(10000, 75000, true) : DataGenerator.RandomNumber(10000, 150000, true);

            redemptionPage.EnterPartnerCode(partnerCode);

            redemptionPage.EnterProduct(voucherProduct);

            redemptionPage.EnterVoucherProduct(voucherName);

            redemptionPage.EnterVoucherPoints(voucherPoints);

            redemptionPage.EnterActivityDate(activityDate);

            redemptionPage.EnterPoints(redemptionPoints);

            redemptionPage.EnterComments("Voucher Accrual Test Transaction");

            redemptionPage.EnterOriginAirportCode(originAirportCode);

            redemptionPage.EnterDestinationAirportCode(destinationAirportCode);

            redemptionPage.EnterPNR(pnr);

            redemptionPage.EnterBookingClass(bookingClass);

            redemptionPage.EnterNoOfPassengers("1");

            redemptionPage.EnterNoOfSegments("1");

            redemptionPage.ClickOnApplyRedemptionButton();

            string expectedMessage = "Successful Manual Redemption Applied";

            webDriver.WaitForElementToBeVisible(Redemption.PopUpMessageContent, 30);

            string actualAccrualTxnId = webDriver.FindElement(By.XPath(Redemption.PopUpMessageContent + "/b[1]")).Text;

            string actualMemberNumber = webDriver.FindElement(By.XPath(Redemption.PopUpMessageContent + "/b[3]")).Text;

            string actualPointsRedeemed = webDriver.FindElement(By.XPath(Redemption.PopUpMessageContent + "/b[4]")).Text;

            string actualActivityDate = webDriver.FindElement(By.XPath(Redemption.PopUpMessageContent + "/b[5]")).Text;

            string actualPartnerCode = webDriver.FindElement(By.XPath(Redemption.PopUpMessageContent + "/b[6]")).Text;

            string actualVoucherRedemptionProduct = webDriver.FindElement(By.XPath(Redemption.PopUpMessageContent + "/b[8]")).Text;

            string actualMessage = webDriver.FindElement(By.XPath(Redemption.PopUpMessageContent + "/p/b")).Text;

            Assert.True(!actualAccrualTxnId.IsNullOrEmpty(), $"Expected a valid voucher accrual Transaction Id but found {actualAccrualTxnId}.");

            Assert.True(memberNumber.Equals(actualMemberNumber), $"Expected the FC Number on the Pop up to be {memberNumber} but found {actualMemberNumber}.");

            Assert.True(redemptionPoints.Equals(actualPointsRedeemed), $"Expected the Points Redeemed on the Pop up to be {redemptionPoints} but found {actualPointsRedeemed}.");

            Assert.True(activityDate.Equals(actualActivityDate), $"Expected the Activity Date on the Pop up to be {activityDate} but found {actualActivityDate}.");

            Assert.True(partnerCode.Equals(actualPartnerCode), $"Expected the PartnerCode on the Pop up to be {partnerCode} but found {actualPartnerCode}.");

            Assert.True(voucherProduct.Equals(actualVoucherRedemptionProduct), $"Expected the Product on the Pop up to be {voucherProduct} but found {actualVoucherRedemptionProduct}.");

            Assert.True(expectedMessage.Equals(actualMessage), $"Expected the Success Message on the Pop up to be {expectedMessage} but found {actualMessage}.");

            redemptionPage.ClickOnClosePopupIcon();

            #endregion

            #region Validating the Created Voucher Accrual and Redemption Transactions on the Member Detail Page

            bool isVoucherAccrualTxnPresent = false;

            bool isVoucherRedemptionTxnPresent = false;

            DateTime currentDate = DateTime.UtcNow.Date;

            string expectedTransactionStatus = "Processed";//For Both Voucher Accrual and Redemption

            string expectedAccrualTxnStatementText = "Points credit for reward voucher redemption";

            string expectedRedemptionTxnStatementText = $"{voucherProduct},{voucherName.Split('-')?[1]}, Booking ref {pnr}, {originAirportCode}-{destinationAirportCode}";

            string expectedVoucherAccrualTxnProduct = "Voucher Points";

            webDriver.WaitForElementToBeVisible(MemberProfile.MemberDetailTxnList, 20);

            IEnumerable<IWebElement> txnList = webDriver.FindElements(By.XPath(MemberProfile.MemberDetailTxnList));

            SoftAssertHelper softAssert = new();

            foreach (IWebElement element in txnList)
            {
                string bookingReference = element.FindElement(By.XPath("td[7]/div/div/span")).Text;

                //Validating the presence of the Transaction both Voucher Accrual and Redemption
                if (pnr.Is(bookingReference))
                {
                    string actualTxnActivityDate = element.FindElement(By.XPath("td[1]/div/div/span")).Text;

                    string actualTxnDate = element.FindElement(By.XPath("td[2]/div/div/span")).Text;

                    string actualTxnType = element.FindElement(By.XPath("td[3]/div/div/span")).Text;

                    string actualTxnPartnerCode = element.FindElement(By.XPath("td[4]/div/div/span")).Text;

                    string actualTxnFlightNumber = element.FindElement(By.XPath("td[5]/div/div/span")).Text;

                    string actualTxnBookingClass = element.FindElement(By.XPath("td[6]/div/div/span")).Text;

                    string actualTxnOriginAirportCode = element.FindElement(By.XPath("td[8]/div/div/span")).Text;

                    string actualTxnDestinationAirportCode = element.FindElement(By.XPath("td[9]/div/div/span")).Text;

                    string actualTxnTierPoints = element.FindElement(By.XPath("td[10]/div/div/span")).Text;

                    string actualTxnPoints = element.FindElement(By.XPath("td[11]/div/div/span")).Text;

                    string actualTxnStatus = element.FindElement(By.XPath("td[12]/div/div/span")).Text;

                    string actualTxnProduct = element.FindElement(By.XPath("td[13]/div/div/span")).Text;

                    string actualTxnStatementText = element.FindElement(By.XPath("td[14]/div/div/span")).Text;

                    if (!actualTxnType.IsNullOrEmpty())
                    {
                        if (actualTxnType == "Redemption")
                        {
                            isVoucherRedemptionTxnPresent = true;

                            softAssert.True(actualTxnActivityDate == activityDate, $"For the Member {memberNumber}  Expected the Voucher Redemption Transaction activityDate \n to be {activityDate} \n but found {actualTxnActivityDate}.");

                            softAssert.True(actualTxnDate == currentDate.ToString("dd-MMM-yyyy"), $"For the Member {memberNumber} Expected the Voucher Redemption Transaction Date to be {currentDate.ToString("dd-MMM-yyyy")}  \n but found  {actualTxnDate}.");

                            softAssert.True(actualTxnPartnerCode == partnerCode, $"For the Member {memberNumber}  Expected the Voucher Redemption Transaction PartnerCode  \n to be  {partnerCode}  \n but found  {actualTxnPartnerCode}.");

                            softAssert.True(actualTxnBookingClass == bookingClass, $"For the Member {memberNumber}  Expected the Voucher Redemption Transaction BookingClass  \n to be  {bookingClass}  \n but found  {actualTxnBookingClass}.");

                            softAssert.True(actualTxnOriginAirportCode == originAirportCode, $"For the Member {memberNumber}  Expected the Voucher Redemption Transaction Origin Airport Code  \n to be  {originAirportCode}  \n but found  {actualTxnOriginAirportCode}.");

                            softAssert.True(actualTxnDestinationAirportCode == destinationAirportCode, $"For the Member {memberNumber} \n Expected the Voucher Redemption Transaction Destination Airport Code  \n to be  {destinationAirportCode}  \n but found  {actualTxnDestinationAirportCode}.");

                            softAssert.True(actualTxnTierPoints == "(0)", $"For the Member {memberNumber} Expected the Voucher Redemption Transaction Tier Points Value  \n to be  (0)  \n but found  {actualTxnTierPoints}.");

                            softAssert.True(actualTxnPoints == "(0)", $"For the Member {memberNumber} Expected the Voucher Redemption Transaction the Points Value  \n to be  (0)  \n but found  {actualTxnPoints}.");

                            softAssert.True(actualTxnStatus == expectedTransactionStatus, $"For the Member {memberNumber} Expected the Voucher Redemption Transaction Status  \n to be  {expectedTransactionStatus}  \n but found  {actualTxnStatus}.");

                            softAssert.True(actualTxnProduct == voucherProduct, $"For the Member {memberNumber} Expected the Voucher Redemption Transaction Product Type  \n to be  {voucherProduct}  \n but found  {actualTxnProduct}.");

                            softAssert.True(actualTxnStatementText == expectedRedemptionTxnStatementText, $"For the Member {memberNumber}  Expected the Voucher Redemption Transaction Statement Text  \n to be  {expectedRedemptionTxnStatementText}  \n but found  {actualTxnStatementText}.");


                        }
                        else if (actualTxnType == "Accrual")
                        {
                            isVoucherAccrualTxnPresent = true;

                            softAssert.True(actualTxnActivityDate == currentDate.ToString("dd-MMM-yyyy"), $"For the Member {memberNumber}  Expected the Voucher Accrual Transaction activityDate  \n to be  {currentDate.ToString("dd-MMM-yyyy")}  \n but found  {actualTxnActivityDate}.");

                            softAssert.True(actualTxnDate == currentDate.ToString("dd-MMM-yyyy"), $"For the Member {memberNumber}  Expected the Voucher Accrual Transaction Date  \n to be  {currentDate.ToString("dd-MMM-yyyy")}  \n but found  {actualTxnDate}.");

                            softAssert.True(actualTxnPartnerCode == partnerCode, $"For the Member {memberNumber}  Expected the Voucher Accrual Transaction PartnerCode  \n to be  {partnerCode}  \n but found  {actualTxnPartnerCode}.");

                            softAssert.True(actualTxnBookingClass == bookingClass, $"For the Member {memberNumber}  Expected the Voucher Accrual Transaction BookingClass  \n to be  {bookingClass}  \n but found  {actualTxnBookingClass}.");

                            softAssert.True(actualTxnOriginAirportCode == originAirportCode, $"For the Member {memberNumber} Expected the Voucher Accrual Transaction Origin Airport Code  \n to be  {originAirportCode}  \n but found  {actualTxnOriginAirportCode}.");

                            softAssert.True(actualTxnDestinationAirportCode == destinationAirportCode, $"For the Member {memberNumber}  Expected the Voucher Accrual Transaction Destination Airport Code  \n to be  {destinationAirportCode}  \n but found  {actualTxnDestinationAirportCode}.");

                            softAssert.True(actualTxnTierPoints == "0", $"For the Member {memberNumber} Expected the Voucher Accrual Transaction Tier Points Value  \n to be  0  \n but found  {actualTxnTierPoints}.");

                            softAssert.True(actualTxnPoints == voucherPoints, $"For the Member {memberNumber} Expected the Voucher Accrual Transaction the Points Value  \n to be  {voucherPoints}  \n but found  {actualTxnPoints}.");

                            softAssert.True(actualTxnStatus == expectedTransactionStatus, $"For the Member {memberNumber} Expected the Voucher Accrual Transaction Status  \n to be  {expectedTransactionStatus}  \n but found  {actualTxnStatus}.");

                            softAssert.True(actualTxnProduct == expectedVoucherAccrualTxnProduct, $"For the Member {memberNumber}  Expected the Voucher Accrual Transaction Product Type  \n to be  {expectedVoucherAccrualTxnProduct}  \n but found  {actualTxnProduct}.");

                            softAssert.True(actualTxnStatementText == expectedAccrualTxnStatementText, $"For the Member {memberNumber}  Expected the Voucher Accrual Transaction Statement Text  \n to be  {expectedAccrualTxnStatementText}  \n but found  {actualTxnStatementText}.");
                        }

                    }

                }

                if (isVoucherAccrualTxnPresent && isVoucherRedemptionTxnPresent)
                    break;
            }

            softAssert.True(isVoucherRedemptionTxnPresent, $"For the Member {memberNumber} \n The Voucher Redemption Transaction with the PNR {pnr} not found in the Member Detail Page");

            softAssert.True(isVoucherAccrualTxnPresent, $"For the Member {memberNumber} \nThe Voucher Accrual Transaction with the PNR {pnr} not found  in the Member Detail Page");

            memberProfilePage.ClickOnMenuBarOnMemberProfilePage(logger);

            memberProfilePage.ClickOnMemberDetailTab();

            string memberPointBalanceAfterTxn = webDriver.GetAttributeTextValue(MemberDetail.MemberPointBalance, "value");

            int actualTotalBalance = Convert.ToInt32(memberPointBalanceAfterTxn);

            int expectedTotalBalance = Convert.ToInt32(memberPointBalanceBeforeTxn) + Convert.ToInt32(voucherPoints);

            softAssert.True(expectedTotalBalance == actualTotalBalance, $"For the Member {memberNumber}  Expected The Member Point Balance  \n to be  {expectedTotalBalance}  \n but found  {actualTotalBalance}");

            #endregion

            #region Validating that the Voucher status is updated to Used in MyOffer Page and MyOffersAuditPage

            myOffersPage.NavigateToMyOffersPage(logger);

            webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads, 10);

            webDriver.WaitForElementToBeClickable(MyOffers.VoucherAddButton);

            string expectedVoucherUsageStatus = "Used";//Voucher Status to be used after successful Voucher Redemption transaction

            IEnumerable<IWebElement> vouchers = webDriver.FindElements(By.XPath(MyOffers.MyOffersPageDataTable));

            bool isVoucherFoundOnMyOffersPage = false;

            foreach (IWebElement element in vouchers)
            {
                string voucherNumber = element.FindElement(By.XPath("td[1]/div/div/span")).GetAttribute("value");

                if (voucherNumber.Is(voucherId))
                {
                    isVoucherFoundOnMyOffersPage = true;

                    string actualVoucherName = element.FindElement(By.XPath("td[2]/div/div/span")).Text;

                    string actualVoucherUsageStatus = element.FindElement(By.XPath("td[3]/div/div/span")).Text;

                    string actualVoucherUsedDate = element.FindElement(By.XPath("td[7]/div/div/span")).Text;

                    softAssert.True(actualVoucherName == voucherName, $"For the Member {memberNumber} After the successful Voucher Redemption \n In the My Offers Page for the Voucher Id {voucherId} \n Expected The VoucherName \n to be  {voucherName}  \n but found  {actualVoucherName}");

                    softAssert.True(actualVoucherUsageStatus == expectedVoucherUsageStatus, $"For the Member {memberNumber} After the successful Voucher Redemption \n In the My Offers Page for the Voucher Id {voucherId} \n Expected The Voucher Status \n to be  {expectedVoucherUsageStatus}  \n but found  {actualVoucherUsageStatus}");

                    softAssert.True(actualVoucherUsedDate == currentDate.ToString("dd-MMM-yyyy"), $"For the Member {memberNumber} After the successful Voucher Redemption \n In the My Offers Page for the Voucher Id {voucherId} \n Expected The Voucher UsedDate\n to be  {currentDate.ToString("dd-MMM-yyyy")}  \n but found  {actualVoucherUsedDate}");

                    webDriver.WaitForElementsToBeVisible($"{MyOffers.MyOffersPageDataTable}/td[1]");

                    webDriver.MoveToElement(element.FindElement(By.XPath("td[1]")));

                    element.FindElement(By.XPath("td[1]")).Click();

                    webDriver.WaitForElementToBeVisible(MyOffers.NavigateBackToMyOffersPageButton);

                    webDriver.WaitForElementToBePresent(MemberSearch.WaitUntilPALoads, 10);

                    webDriver.WaitForElementToBePresent(MyOffers.MyOffersAuditPageDataTable + "[1]/td[1]", 10);

                    IWebElement auditData = webDriver.FindElements(By.XPath(MyOffers.MyOffersAuditPageDataTable)).First();

                    string actualAuditVoucherStatus = auditData.FindElement(By.XPath("td[3]/div/div/span")).GetAttribute("value") ?? "";

                    string actualAuditVoucherUsedDate = auditData.FindElement(By.XPath("td[6]/div/div/span")).GetAttribute("value") ?? "";

                    string actualAuditVoucherUpdatedBy = auditData.FindElement(By.XPath("td[10]/div/div/span")).GetAttribute("value") ?? "";

                    softAssert.True(actualAuditVoucherStatus == expectedVoucherUsageStatus, $"For the Member {memberNumber} After the successful Voucher Redemption \n In the My Offers Page for the Voucher Id {voucherId} In the Audit Page \n Expected The Voucher UsedDate\n to be  {currentDate.ToString("dd-MMM-yyyy")}  \n but found  {actualAuditVoucherStatus}");

                    softAssert.True(actualAuditVoucherUsedDate == currentDate.ToString("dd-MMM-yyyy"), $"For the Member {memberNumber} After the successful Voucher Redemption \n In the My Offers Page for the Voucher Id {voucherId} In the Audit Page \n Expected The Voucher UsedDate\n to be  {currentDate.ToString("dd-MMM-yyyy")}  \n but found  {actualAuditVoucherUsedDate}");

                    softAssert.True(actualAuditVoucherUpdatedBy.Has(currentLoggedInUser), $"For the Member {memberNumber} After the successful Voucher Redemption \n In the My Offers Page for the Voucher Id {voucherId} In the Audit Page \n Expected The Voucher UpdatedBy\n to have  {currentLoggedInUser}  \n but found  {actualAuditVoucherUpdatedBy}");

                    break;

                }
            }

            softAssert.True(isVoucherFoundOnMyOffersPage, $" The Voucher with Voucher Id : {voucherId} of the Member {memberNumber} not found in the MyOffersPage though it is present in Cosmos DB)");

            #endregion

            softAssert.AssertAll();
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
