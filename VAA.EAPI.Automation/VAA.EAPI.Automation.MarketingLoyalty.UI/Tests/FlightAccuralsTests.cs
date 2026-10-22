using AventStack.ExtentReports;
using Microsoft.Azure.Cosmos;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.UI.ElementsLibrary;
using VAA.EAPI.Automation.MarketingLoyalty.UI.PageObjects;
using static VAA.EAPI.Automation.Core.UI.Factory.BrowserFactory;
using FlyingClubMember = VAA.EAPI.Automation.Core.DB_Model.Entities.FlyingClubMember;
using Traits = VAA.EAPI.Automation.MarketingLoyalty.UI.Constants.Traits;

namespace VAA.EAPI.Automation.MarketingLoyalty.UI.Tests
{
    [Collection("FlightAccuralsTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class FlightAccuralsTests : IDisposable, IClassFixture<ReportFixture>, IClassFixture<CosmosFixture>
    {
        private ReportFixture reportFixture;
        private CosmosFixture cosmosFixture;
        private IWebDriver webDriver;
        private MemberSearchPage memberSearchPage;
        private FlightAccuralsPage flightAccrualsPage;

        private readonly string customerTransactionCosmosDb;

        public string env = Config.Environment;

        public FlightAccuralsTests(ReportFixture reportFixture, CosmosFixture cosmosFixture)
        {
            this.reportFixture = reportFixture;
            this.cosmosFixture = cosmosFixture;

            webDriver = GetBrowser(BrowserType.Chrome, ConfigReader.Get("PowerAppsUrl"));

            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.cosmosFixture.RegisterType<Transaction>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            memberSearchPage = new MemberSearchPage(webDriver);

            flightAccrualsPage = new FlightAccuralsPage(webDriver);
        }


        [Trait("Category", Traits.FlightAccrualsUIRegression)]
        [Fact, RunOrder(1)]
        public void TC01_FlightAccrualSuccessScenario()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC01_FlightAccrualSuccessScenario");

            string queryForFcmDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.TierCode not in ('Glass') order by c.CreatedOn desc";

            FlyingClubMember flyingClubMemberDoc = cosmosFixture.QueryItem<FlyingClubMember>(queryForFcmDoc);

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, flyingClubMemberDoc.MemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            flightAccrualsPage.ClickOnMenuBarOnMemberProfilePage();

            DateTime randomDate = DataGenerator.RandomPastDate(182);

            string ticketIssueDate = randomDate.ToString("dd-MMM-yyyy");

            string activityDate = randomDate.AddDays(1).ToString("dd-MMM-yyyy");

            string bookingReferenceNumber = DataGenerator.RandomString(6, true).ToUpper();

            string bookingClass = "J";

            string flightNumber = DataGenerator.RandomNumber(111, 999, true);

            string originAirportCode = "LHR";

            string destinationAirportCode = "JFK";

            string ticketNumber = DataGenerator.RandomNumber(93230000, 93239999, true) + DataGenerator.RandomNumber(00000, 99999);

            flightAccrualsPage.EnterBookingReferenceNumber(bookingReferenceNumber);

            flightAccrualsPage.EnterBookingClass(bookingClass);

            flightAccrualsPage.EnterTicketIssueDate(ticketIssueDate);

            flightAccrualsPage.EnterActivityDate(activityDate);

            flightAccrualsPage.EnterFlightNumber(flightNumber);

            flightAccrualsPage.EnterOriginAiportCode(originAirportCode);

            flightAccrualsPage.EnterDestinationAiportCode(destinationAirportCode);

            flightAccrualsPage.EnterTicketNumber(ticketNumber);

            flightAccrualsPage.EnterComment("Test Transaction");

            flightAccrualsPage.ClickOnSubmitButton();

            string[] text = flightAccrualsPage.GetMessageFromThePopUpScreen();

            string expectedMessage = "Flight accrual transaction created successfully.";

            Assert.True(text[0].Has(expectedMessage),
                "Flight Accrual transaction was not created for the member");

            flightAccrualsPage.CloseThePopUp();

            bool flag = false;

            IEnumerable<IWebElement> txnList = webDriver.FindElements(By.XPath(ElementsLibrary.MemberProfile.MemberDetailTxnList));

            foreach (IWebElement element in txnList)
            {

                string pnr = element.FindElement(By.XPath("td[7]/div/div/span")).Text;
                //Validating the presence of newly created BookingReference present on member detail page transaction list

                if (pnr.Equals(bookingReferenceNumber))
                {
                    string actualActivityDate = element.FindElement(By.XPath("td[1]/div/div/span")).Text;

                    string actualTxnDate = element.FindElement(By.XPath("td[2]/div/div/span")).Text;

                    string actualTxnType = element.FindElement(By.XPath("td[3]/div/div/span")).Text;

                    string actualPartnerCode = element.FindElement(By.XPath("td[4]/div/div/span")).Text;

                    string actualFlightNumber = element.FindElement(By.XPath("td[5]/div/div/span")).Text;

                    string actualBookingClass = element.FindElement(By.XPath("td[6]/div/div/span")).Text;

                    string actualOriginAirportCode = element.FindElement(By.XPath("td[8]/div/div/span")).Text;

                    string actualDestinationAirportCode = element.FindElement(By.XPath("td[9]/div/div/span")).Text;

                    string actualTierPoints = element.FindElement(By.XPath("td[10]/div/div/span")).Text;

                    string actualPoints = element.FindElement(By.XPath("td[11]/div/div/span")).Text;

                    string actualStatus = element.FindElement(By.XPath("td[12]/div/div/span")).Text;

                    string actualProduct = element.FindElement(By.XPath("td[13]/div/div/span")).Text;

                    Assert.True(activityDate.Equals(actualActivityDate),
                        $"Expected the activityDate to be {activityDate} but found {actualActivityDate}.");

                    string currentDate = DateTime.UtcNow.ToString("dd-MMM-yyyy");

                    Assert.True(currentDate.Equals(actualTxnDate),
                        $"Expected the TransactionDate to be {currentDate} but found {actualTxnDate}.");

                    Assert.True("Accrual".Equals(actualTxnType),
                        $"Expected the TransactionType to be Accrual but found {actualTxnType}.");

                    Assert.True("VS".Equals(actualPartnerCode),
                        $"Expected the PartnerCode to be VS but found {actualPartnerCode}.");

                    Assert.True(flightNumber.Equals(actualFlightNumber),
                        $"Expected the flightNumber to be {flightNumber} but found {actualFlightNumber}.");

                    Assert.True(bookingClass.Equals(actualBookingClass),
                      $"Expected the BookingClass to be {bookingClass} but found {actualBookingClass}.");

                    Assert.True(originAirportCode.Equals(actualOriginAirportCode),
                      $"Expected the OriginAirportCode to be {originAirportCode} but found {actualOriginAirportCode}.");

                    Assert.True(destinationAirportCode.Equals(actualDestinationAirportCode),
                      $"Expected the DestinationAirportCode to be {destinationAirportCode} but found {actualDestinationAirportCode}.");

                    Assert.True(!string.IsNullOrEmpty(actualTierPoints),
                      $"Expected valid Tier Points value but found null or empty value.(Txn Sync issue)");

                    Assert.True(!string.IsNullOrEmpty(actualPoints),
                     $"Expected valid Points value but found null or empty value.(Txn Sync issue)");

                    Assert.True("Processed".Equals(actualStatus),
                        $"Expected the Transaction Status to be Processed but found {actualStatus}.");

                    Assert.True("Flight".Equals(actualProduct),
                       $"Expected the Transaction Product to be Flight but found {actualProduct}.");

                    flag = true;
                }
            }

            Assert.True(flag, "The Transaction not present in the Transaction history (Member Detail Page)");
        }

        [Trait("Category", Traits.FlightAccrualsUIRegression)]
        [Theory, RunOrder(2)]
        [InlineData("bookingReference")]
        [InlineData("activityDate")]
        [InlineData("flightNumber")]
        [InlineData("bookingClass")]
        [InlineData("ticketIssueDate")]
        [InlineData("originAirportCode")]
        [InlineData("destinationAirportCode")]
        public void TC02_CreateButtonValidationWhenMandatoryFieldValuesAreMissing(string mandatoryField)
        {
            ExtentTest logger = reportFixture.Current.CreateTest($"TC02_CreateButtonValidationWhenMandatoryField-{mandatoryField}-ValuesAreMissing");

            string queryForFcmDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.TierCode not in ('Glass') order by c.CreatedOn desc";

            FlyingClubMember flyingClubMemberDoc = cosmosFixture.QueryItem<FlyingClubMember>(queryForFcmDoc);

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, flyingClubMemberDoc.MemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            flightAccrualsPage.ClickOnMenuBarOnMemberProfilePage();

            DateTime randomDate = DataGenerator.RandomPastDate(182);

            string ticketIssueDate = randomDate.ToString("dd-MMM-yyyy");

            string activityDate = randomDate.AddDays(1).ToString("dd-MMM-yyyy");

            string bookingReferenceNumber = DataGenerator.RandomString(6, true).ToUpper();

            string bookingClass = "J";

            string flightNumber = DataGenerator.RandomNumber(111, 999, true);

            string originAirportCode = "LHR";

            string destinationAirportCode = "JFK";

            string ticketNumber = DataGenerator.RandomNumber(93230000, 93239999, true) + DataGenerator.RandomNumber(00000, 99999);

            switch (mandatoryField)
            {
                case "bookingReference":
                    bookingReferenceNumber = "";
                    break;

                case "activityDate":
                    activityDate = "";
                    break;

                case "ticketIssueDate":
                    ticketIssueDate = "";
                    break;

                case "originAirportCode":
                    originAirportCode = "";
                    break;

                case "destinationAirportCode":
                    destinationAirportCode = "";
                    break;

                case "flightNumber":
                    flightNumber = "";
                    break;

                case "bookingClass":
                    bookingClass = "";
                    break;

                default:
                    break;
            }

            flightAccrualsPage.EnterBookingReferenceNumber(bookingReferenceNumber);

            flightAccrualsPage.EnterBookingClass(bookingClass);

            flightAccrualsPage.EnterTicketIssueDate(ticketIssueDate);

            flightAccrualsPage.EnterActivityDate(activityDate);

            flightAccrualsPage.EnterFlightNumber(flightNumber);

            flightAccrualsPage.EnterOriginAiportCode(originAirportCode);

            flightAccrualsPage.EnterDestinationAiportCode(destinationAirportCode);

            flightAccrualsPage.EnterTicketNumber(ticketNumber);

            flightAccrualsPage.EnterComment("Test Transaction");

            Assert.True(flightAccrualsPage.IsCreateButtonDisabled(),
               $"Expected the Create Transaction Button to be in disabled state but found enabled when mandatory field {mandatoryField} value is missed.");
        }


        [Trait("Category", Traits.FlightAccrualsUIRegression)]
        [Fact, RunOrder(3)]
        public void TC03_ErrorMessageValidationWhenEnteredInvalidBookingClass()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC03_ErrorMessageValidationWhenEnteredInvalidBookingClass");

            string queryForFcmDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.TierCode not in ('Glass') order by c.CreatedOn desc";

            FlyingClubMember flyingClubMemberDoc = cosmosFixture.QueryItem<FlyingClubMember>(queryForFcmDoc);

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, flyingClubMemberDoc.MemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            flightAccrualsPage.ClickOnMenuBarOnMemberProfilePage();

            DateTime randomDate = DataGenerator.RandomPastDate(182);

            string ticketIssueDate = randomDate.ToString("dd-MMM-yyyy");

            string activityDate = randomDate.AddDays(1).ToString("dd-MMM-yyyy");

            string bookingReferenceNumber = DataGenerator.RandomString(6, true).ToUpper();

            string bookingClass = "F";

            string flightNumber = DataGenerator.RandomNumber(111, 999, true);

            string originAirportCode = "LHR";

            string destinationAirportCode = "JFK";

            string ticketNumber = DataGenerator.RandomNumber(93230000, 93239999, true) + DataGenerator.RandomNumber(00000, 99999);

            flightAccrualsPage.EnterBookingReferenceNumber(bookingReferenceNumber);

            flightAccrualsPage.EnterBookingClass(bookingClass);

            flightAccrualsPage.EnterTicketIssueDate(ticketIssueDate);

            flightAccrualsPage.EnterActivityDate(activityDate);

            flightAccrualsPage.EnterFlightNumber(flightNumber);

            flightAccrualsPage.EnterOriginAiportCode(originAirportCode);

            flightAccrualsPage.EnterDestinationAiportCode(destinationAirportCode);

            flightAccrualsPage.EnterTicketNumber(ticketNumber);

            flightAccrualsPage.EnterComment("Test Transaction");

            flightAccrualsPage.ClickOnSubmitButton();

            string[] text = flightAccrualsPage.GetMessageFromThePopUpScreen();

            string expectedMessage = "Operating Booking Class Not Found";

            Assert.True(text[1].Has(expectedMessage),
                $"Expected Error Message was {expectedMessage} but found {text[1]} ");
        }

        [Trait("Category", Traits.FlightAccrualsUIRegression)]
        [Fact, RunOrder(4)]
        public void TC04_ErrorMessageValidationWhenEnteredInvalidOriginAndDestinationCombination()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC03_ErrorMessageValidationWhenEnteredInvalidBookingClass");

            string queryForFcmDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.TierCode not in ('Glass') order by c.CreatedOn desc";

            FlyingClubMember flyingClubMemberDoc = cosmosFixture.QueryItem<FlyingClubMember>(queryForFcmDoc);

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, flyingClubMemberDoc.MemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            flightAccrualsPage.ClickOnMenuBarOnMemberProfilePage();

            DateTime randomDate = DataGenerator.RandomPastDate(182);

            string ticketIssueDate = randomDate.ToString("dd-MMM-yyyy");

            string activityDate = randomDate.AddDays(1).ToString("dd-MMM-yyyy");

            string bookingReferenceNumber = DataGenerator.RandomString(6, true).ToUpper();

            string bookingClass = "J";

            string flightNumber = DataGenerator.RandomNumber(111, 999, true);

            string originAirportCode = "DEL";

            string destinationAirportCode = "SFO";

            string ticketNumber = DataGenerator.RandomNumber(93230000, 93239999, true) + DataGenerator.RandomNumber(00000, 99999);

            flightAccrualsPage.EnterBookingReferenceNumber(bookingReferenceNumber);

            flightAccrualsPage.EnterBookingClass(bookingClass);

            flightAccrualsPage.EnterTicketIssueDate(ticketIssueDate);

            flightAccrualsPage.EnterActivityDate(activityDate);

            flightAccrualsPage.EnterFlightNumber(flightNumber);

            flightAccrualsPage.EnterOriginAiportCode(originAirportCode);

            flightAccrualsPage.EnterDestinationAiportCode(destinationAirportCode);

            flightAccrualsPage.EnterTicketNumber(ticketNumber);

            flightAccrualsPage.EnterComment("Test Transaction");

            flightAccrualsPage.ClickOnSubmitButton();

            string[] text = flightAccrualsPage.GetMessageFromThePopUpScreen();

            string expectedMessage = "Bookable Activity not found";

            Assert.True(text[1].Has(expectedMessage),
                $"Expected Error Message was {expectedMessage} but found {text[1]} ");
        }


        [Trait("Category", Traits.FlightAccrualsUIRegression)]
        [Theory, RunOrder(5)]
        [InlineData("bookingReference")]
        [InlineData("activityDate")]
        [InlineData("flightNumber")]
        [InlineData("ticketIssueDate")]
        [InlineData("originAirportCode destinationAirportCode")]
        [InlineData("activityDate ticketIssueDate")]
        [InlineData("ticketNumber")]
        public void TC05_ErrorMessageValidationWhenFieldValuesAreInvalid(string field)
        {
            ExtentTest logger = reportFixture.Current.CreateTest($"TC05_ErrorMessageValidationWhenField-{field}-ValuesAreInvalid");

            string queryForFcmDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = 'FlyingClubMember' and c.TierCode not in ('Glass') order by c.CreatedOn desc";

            FlyingClubMember flyingClubMemberDoc = cosmosFixture.QueryItem<FlyingClubMember>(queryForFcmDoc);

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, flyingClubMemberDoc.MemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            flightAccrualsPage.ClickOnMenuBarOnMemberProfilePage();

            DateTime randomDate = DataGenerator.RandomPastDate(182);

            string ticketIssueDate = randomDate.ToString("dd-MMM-yyyy");

            string activityDate = randomDate.AddDays(1).ToString("dd-MMM-yyyy");

            string bookingReferenceNumber = DataGenerator.RandomString(6, true).ToUpper();

            string bookingClass = "J";

            string flightNumber = DataGenerator.RandomNumber(111, 999, true);

            string originAirportCode = "LHR";

            string destinationAirportCode = "JFK";

            string ticketNumber = DataGenerator.RandomNumber(93230000, 93239999, true) + DataGenerator.RandomNumber(00000, 99999);

            string expectedErrorMessage = string.Empty;

            switch (field)
            {
                case "bookingReference":
                    bookingReferenceNumber = "DE4RT";
                    expectedErrorMessage = "Please provide appropriate booking reference number Ex. A1B2C3";
                    break;

                case "activityDate":
                    activityDate = "31-Dec-2025";
                    expectedErrorMessage = "Activity date cannot be future date";
                    break;

                case "ticketIssueDate":
                    ticketIssueDate = "31-Dec-2025";
                    expectedErrorMessage = "Ticket issue date cannot be future date";
                    break;

                case "originAirportCode destinationAirportCode":
                    originAirportCode = "LHR";
                    destinationAirportCode = "LHR";
                    expectedErrorMessage = "The origination airport and destination airport should be different";
                    break;

                case "activityDate ticketIssueDate":
                    activityDate = "05-Nov-2023";
                    ticketIssueDate = "06-Nov-2023";
                    expectedErrorMessage = "Activity date cannot be earlier than Ticket issue date";
                    break;

                case "flightNumber":
                    flightNumber = "SE3";
                    expectedErrorMessage = "Please provide appropriate flight number. Ex. 0301";
                    break;

                case "ticketNumber":
                    ticketNumber = "93225452613";
                    expectedErrorMessage = "The ticket number must be 13 digits long. Ex. 1234567899999";
                    break;

                default:
                    break;
            }

            flightAccrualsPage.EnterBookingReferenceNumber(bookingReferenceNumber);

            flightAccrualsPage.EnterBookingClass(bookingClass);

            flightAccrualsPage.EnterFlightNumber(flightNumber);

            flightAccrualsPage.EnterTicketNumber(ticketNumber);

            flightAccrualsPage.EnterOriginAiportCode(originAirportCode);

            flightAccrualsPage.EnterDestinationAiportCode(destinationAirportCode);

            flightAccrualsPage.EnterTicketIssueDate(ticketIssueDate);

            flightAccrualsPage.EnterActivityDate(activityDate);

            webDriver.SwitchTo().DefaultContent();

            webDriver.WaitForElementToBePresent(FlightAccurals.ToastWrngMsg, 5);

            string toastErrorMessage;
            try
            {
                toastErrorMessage = webDriver.FindElement(By.XPath(FlightAccurals.ToastWrngMsg)).Text;
            }
            catch (Exception)
            {
                toastErrorMessage = string.Empty;
            }
            finally
            {
                memberSearchPage.SwitchToPowerAppIFrame();
            }

            string actualErrorMessage = string.Empty;

            switch (field)
            {
                case "bookingReference":
                    actualErrorMessage = flightAccrualsPage.GetErrorMessage(FlightAccurals.BookingRefWrngMsg);
                    break;

                case "flightNumber":
                    actualErrorMessage = flightAccrualsPage.GetErrorMessage(FlightAccurals.FlightNumberWrngMsg);
                    break;

                case "ticketNumber":
                    actualErrorMessage = flightAccrualsPage.GetErrorMessage(FlightAccurals.TicketNumberWrngMsg);
                    break;

                default:
                    actualErrorMessage = toastErrorMessage;
                    break;

            }

            Assert.True(flightAccrualsPage.IsCreateButtonDisabled(),
               $"Expected the Create Transaction Button to be in disabled state but found enabled when field {field} value is invalid.");

            Assert.True(expectedErrorMessage.Equals(actualErrorMessage),
               $"Expected the Error Message to be in {expectedErrorMessage}  but found {actualErrorMessage} when field {field} value is Invalid.");

        }

        [Trait("Category", Traits.FlightAccrualsUIRegression)]
        [Fact, RunOrder(6)]
        public void TC06_ErrorMessageValidationForDuplicateTransactionCreation()
        {
            ExtentTest logger = reportFixture.Current.CreateTest("TC06_ErrorMessageValidationForDuplicateTransactionCreation");

            string queryForTxnDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TransactionType = @transactionType  and c.ProductId = @productId and c.TransactionChannel = @txnChannel and c.FlightDetails.RetroFlag = @flag and c.PartnerId = @partnerId and c.StatusCode = @statusCode order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForTxnDoc).WithParameter("@docType", "Transaction").WithParameter("@transactionType", "Accrual").WithParameter("@productId", "1-1Z9D").WithParameter("@txnChannel", "PAUSER").WithParameter("@flag", "Y").WithParameter("@partnerId", "1-1XVO").WithParameter("@statusCode", "Processed");

            Transaction txnDoc = cosmosFixture.CosmosQueryItem<Transaction>(parameterizedQuery);

            Assert.True(txnDoc is not null,
               "Didn't find a valid txnDoc for the test execution ");

            memberSearchPage.SwitchToPowerAppIFrame();

            memberSearchPage.WaitForPowerAppsToBeLoadedCompletely(logger);

            memberSearchPage.EnterFCMemberNumberAndSearch(logger, txnDoc.MemberNumber);

            memberSearchPage.ClickOnMemberNumberToNavigateToMemberProfile(logger);

            flightAccrualsPage.ClickOnMenuBarOnMemberProfilePage();

            string ticketIssueDate = txnDoc.FlightDetails.BookingDate.Value.ToString("dd-MMM-yyyy");

            string activityDate = txnDoc.ActivityDate.Value.ToString("dd-MMM-yyyy");

            string bookingReferenceNumber = txnDoc.FlightDetails.ExternalReservationNumber;

            string bookingClass = txnDoc.FlightDetails.BookingClass;

            string flightNumber = txnDoc.FlightDetails.OperatingFlightNumber;

            string originAirportCode = txnDoc.FlightDetails.OriginAirportcode;

            string destinationAirportCode = txnDoc.FlightDetails.DestinationAirport;

            flightAccrualsPage.EnterBookingReferenceNumber(bookingReferenceNumber);

            flightAccrualsPage.EnterBookingClass(bookingClass);

            flightAccrualsPage.EnterTicketIssueDate(ticketIssueDate);

            flightAccrualsPage.EnterActivityDate(activityDate);

            flightAccrualsPage.EnterFlightNumber(flightNumber);

            flightAccrualsPage.EnterOriginAiportCode(originAirportCode);

            flightAccrualsPage.EnterDestinationAiportCode(destinationAirportCode);

            flightAccrualsPage.EnterComment("Test Transaction");

            flightAccrualsPage.ClickOnSubmitButton();

            string[] text = flightAccrualsPage.GetMessageFromThePopUpScreen();

            string expectedMessage = "Duplicate Activity Found";

            Assert.True(text[1].Has(expectedMessage),
                "Flight Accrual transaction was not created for the member");

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
