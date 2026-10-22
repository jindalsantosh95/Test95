using OpenQA.Selenium;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.UI.Factory;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MyAccount.Helpers;
using VAA.EAPI.Automation.MyAccount.Models.Enrolment;
using VAA.EAPI.Automation.MyAccount.UI.PageObjects;
using VAA.EAPI.Infrastructure.AzureStorage;
using static VAA.EAPI.Automation.Core.UI.Factory.BrowserFactory;

namespace VAA.EAPI.Automation.MyAccount.UI.Tests
{
    public class MyAccountEnrolmentTests : IDisposable, IClassFixture<ReportFixture>, IClassFixture<CosmosFixture>, IClassFixture<TableStorageService<OtpRequestTableEntity>>
    {

        private ReportFixture reportFixture;
        private CosmosFixture cosmosFixture;
        private IWebDriver webDriver;
        private EnrolmentPage enrolmentPage;
        private OtpGenerationPage otpGenerationPage;
        private OtpValidationPage otpValidationPage;
        private readonly TableStorageService<OtpRequestTableEntity> otpRequestTableEntityTableStorageService;

        public MyAccountEnrolmentTests(ReportFixture reportFixture, CosmosFixture cosmosFixture, TableStorageService<OtpRequestTableEntity> otpRequestTableEntityTableStorageService)
        {
            this.reportFixture = reportFixture;
            this.cosmosFixture = cosmosFixture;
            this.otpRequestTableEntityTableStorageService = otpRequestTableEntityTableStorageService;

            webDriver = GetBrowser(BrowserType.Chrome, ConfigReader.Get("MyAccountUrl"));

            enrolmentPage = new EnrolmentPage(webDriver);

            otpGenerationPage = new OtpGenerationPage(webDriver);

            otpValidationPage = new OtpValidationPage(webDriver);

            var cosmosEndpoint = "https://avadeuncmdbcustomer.documents.azure.com:443/";
            var cosmosEndpointForCustomerTrx = "https://avadeuncmdbcustomertransactions.documents.azure.com:443/";

            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<Customer>(cosmosEndpoint, "CustomerDB_Acc", "CustomerCollection");
            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, "CustomerTransactionsDB_Acc", "CustomerDetailsCollection");
        }


        [Trait("Category", "EnrolmentUIRegression")]
        [Fact]
        public async void MyAccountEnrolmentSuccess()
        {

            var logger = reportFixture.Current.CreateTest("MyAccountEnrolmentSuccess");

            Random random = new();

            var startdate = DateTime.Now.AddYears(-50);

            var enddate = DateTime.Now.AddYears(-18);

            var range = Convert.ToInt32(enddate.Subtract(startdate).TotalDays);

            string dob = startdate.AddDays(random.Next(range)).ToString("dd-MM-yyyy");

            string emailAddress = DataGenerator.RandomString(15) + "@mail.com";

            string firstName = $"First{DataGenerator.RandomString(5)}";

            string lastName = $"Last{DataGenerator.RandomString(5)}";

            string gender = "Undisclosed";

            string houseNo = "72";

            string postCode = "SO14 0LT";

            string country = "United Kingdom";

            string countryCode = "GB";

            otpGenerationPage.EnterEmailId(logger, emailAddress);

            otpGenerationPage.ClickOnNextButton(logger);

            string successMessageFromUi = otpValidationPage.GetSentOtpSuccessMessage(logger);

            Assert.True(successMessageFromUi.Equals("We’ve sent a 6 digit code to:"), $"After Otp generation success message is wrong message from UI is - {successMessageFromUi}");

            int OTP = await EnrolmentHelper.GetOTPByEmailId(otpRequestTableEntityTableStorageService, emailAddress.ToLower());

            Assert.True(OTP != 0, "OTP should not be zero");

            Assert.True(OTP.ToString().Length.Equals(6), $"OTP is not in 6 digit, OTP is {OTP}");

            otpValidationPage.EnterOtp(logger, OTP);

            otpValidationPage.ClickOnVerifyButton(logger);

            string verificationMessageFromUi = enrolmentPage.GetVerificationMessage(logger);

            Assert.True(verificationMessageFromUi.Equals("Verification successful"), $"After Otp validation success message is wrong message from UI is - {verificationMessageFromUi}");

            enrolmentPage.EnterMemberDetails(logger, firstName, lastName, dob);

            enrolmentPage.SelectGender(logger, gender);

            enrolmentPage.AddAddress(logger, country, houseNo, postCode);

            enrolmentPage.SelectCountryCode(logger, countryCode);

            enrolmentPage.EnterPhoneNumber(logger, DataGenerator.RandomNumber(10));

            enrolmentPage.EnterPassword(logger, "Virgin123");

            enrolmentPage.CheckboxForFlyingClub(logger);

            enrolmentPage.SelectTermsAndCondition(logger);

            enrolmentPage.CreateMyAccountEnrolment(logger);

            string FcNumber = enrolmentPage.GetFcNumberFromUi(logger).Split(":").GetValue(1).ToString().Trim();

            Assert.True(FcNumber != null, $"FC Number should not be null -- {FcNumber}");

            string strInputQueryForCustomerSearch = $"SELECT * FROM CustomerCollection c WHERE c.LoyaltyMemberNumber = '{FcNumber}' and c.DocType='Customer'";

            Customer objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomerSearch);

            int retry = 10;
            while (retry > 0 && objCustomer == null)
            {
                Thread.Sleep(5000);
                objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomerSearch);
                retry--;
            }

            Assert.True(objCustomer != null, "Query from Cosmos DB for Customer Search is null after 10 re tries");

            Assert.True(objCustomer.EnrollmentChannel.Equals("WEB-MYA"), "Channel Id is not same in Cosmos DB");

            Assert.True(objCustomer.FirstName.Equals(firstName), $"first Name is not same in Cosmos DB, actual is {objCustomer.FirstName} and expected {firstName}");

            Assert.True(objCustomer.LastName.Equals(lastName), $"Last Name is not same in Cosmos DB, actual is {objCustomer.LastName} and expected {lastName}");

            string strInputQueryForFlyingClubMember = $"SELECT * FROM CustomerDetailsCollection c WHERE c.MemberNumber = '{FcNumber}' and c.DocType = 'FlyingClubMember'";

            FlyingClubMember objFlyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(strInputQueryForFlyingClubMember);

            retry = 10;
            while (retry > 0 && objFlyingClubMember == null)
            {
                Thread.Sleep(2000);
                objFlyingClubMember = cosmosFixture.QueryItem<FlyingClubMember>(strInputQueryForFlyingClubMember);
                retry--;
            }

            Assert.True(objFlyingClubMember != null, "Query from Cosmos DB for Customer Flynig Club member is null after 10 re tries");

            Assert.True(objFlyingClubMember.CreatedBy.Equals("WEB-MYA"), $"Created by in Flying Club document is wrong {objFlyingClubMember.CreatedBy}");

            Assert.True(objFlyingClubMember.TierCode.Equals("Red"), $"Tier Code is different in Cosmos DB - {objFlyingClubMember.TierCode}");
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
