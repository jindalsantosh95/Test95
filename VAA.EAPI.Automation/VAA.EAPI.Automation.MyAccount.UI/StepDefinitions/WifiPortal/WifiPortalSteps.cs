using AventStack.ExtentReports;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.Core.UI.Factory;
using VAA.EAPI.Automation.Core.UI.Xunit.Features;
using VAA.EAPI.Automation.Core.UI.Xunit.Features.Atttributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MyAccount.Helpers;
using VAA.EAPI.Automation.MyAccount.Models.Enrolment;
using VAA.EAPI.Automation.MyAccount.UI.ElementsLibrary;
using VAA.EAPI.Automation.MyAccount.UI.PageObjects;
using VAA.EAPI.Infrastructure.AzureStorage;
using static VAA.EAPI.Automation.Core.UI.Factory.BrowserFactory;

namespace VAA.EAPI.Automation.MyAccount.UI.StepDefinitions.WifiPortal
{
    public class WifiPortalSteps : IDisposable, IScenarioFailureHandler
    {
        private readonly IWebDriver _webDriver;
        private readonly WifiSignInPage _signInPage;
        private readonly WifiSignUpPage _signUpPage;
        private readonly WifiOtpPage _otpPage;
        private readonly WifiPasswordResetPage _passwordResetPage;
        private readonly TableStorageService<OtpRequestTableEntity> _otpTableService;
        private readonly MemoryStoreFixture _memoryStore;
        private readonly ReportFixture _reportFixture;
        private readonly ExtentTest _featureNode;
        private readonly ExtentTest _logger;

        private string _email;
        private string _password;
        private string _firstName;
        private string _lastName;
        private string _dob;
        private int _previousOtp;

        public WifiPortalSteps(string featureName, string scenarioName, ReportFixture reportFixture, MemoryStoreFixture memoryStore, TableStorageService<OtpRequestTableEntity> otpTableService)
        {
            _reportFixture = reportFixture;
            _memoryStore = memoryStore;
            _otpTableService = otpTableService;

            var featureKey = $"ExtentFeature_{featureName}";
            _memoryStore.SetOnce(featureKey, () => _reportFixture.Current.CreateTest(featureName));
            _featureNode = _memoryStore.Get<ExtentTest>(featureKey);
            _logger = _featureNode.CreateNode(scenarioName);

            _memoryStore.SetOnce("MYA-Password", () => Vault.Default.GetSecretValue("MYA-Password"));
            _memoryStore.SetOnce("MYA-WifiOtp-Email", () => Vault.Default.GetSecretValue("MYA-WifiOtp-Email"));
            _memoryStore.SetOnce("MYA-WifiOtp-Password", () => Vault.Default.GetSecretValue("MYA-WifiOtp-Password"));

            if (!string.IsNullOrWhiteSpace(ConfigReader.Get("BROWSERSTACK_USERNAME")))
            {
                _webDriver = GetBrowser(BrowserType.BrowserStack, Constants.WifiPortal.BaseUrl,
                    bsBrowser: ConfigReader.Get("BROWSERSTACK_BROWSER") ?? "Chrome",
                    bsBrowserVersion: ConfigReader.Get("BROWSERSTACK_BROWSER_VERSION") ?? "latest",
                    bsOs: ConfigReader.Get("BROWSERSTACK_OS") ?? "Windows",
                    bsOsVersion: ConfigReader.Get("BROWSERSTACK_OS_VERSION") ?? "11",
                    bsDevice: ConfigReader.Get("BROWSERSTACK_DEVICE"));
            }
            else
            {
                _webDriver = GetBrowser(BrowserType.Chrome, Constants.WifiPortal.BaseUrl, headless: true);
            }

            _signInPage = new WifiSignInPage(_webDriver);
            _signUpPage = new WifiSignUpPage(_webDriver);
            _otpPage = new WifiOtpPage(_webDriver);
            _passwordResetPage = new WifiPasswordResetPage(_webDriver);
        }

        // ── Sign In Steps ──

        [Given("I accept the cookie banner on the WiFi portal")]
        public void AcceptCookieBanner()
        {
            _signInPage.ClickYesIAgree(_logger);
            _webDriver.LogScreenshot(_logger, "Landing page loaded");
            _webDriver.CheckAccessibility(_logger);
        }

        [Given("I have valid OTP sign-in credentials")]
        public async Task LoadOtpCredentials()
        {
            _email = _memoryStore.Get<string>("MYA-WifiOtp-Email");
            _password = _memoryStore.Get<string>("MYA-WifiOtp-Password");
            _previousOtp = await EnrolmentHelper.GetOTPByEmailId(_otpTableService, _email.ToLower());
            _logger.Log(Status.Info, $"Loaded OTP credentials for: {_email}");
        }

        [Given("I have a Gold loyalty account")]
        public void LoadGoldLoyaltyAccount()
        {
            var loyaltyDetails = RetrieveFromStorageTable.FetchLoyaltyAccountInfoExcludingPoints("Gold");
            _email = loyaltyDetails["email"];
            _password = _memoryStore.Get<string>("MYA-Password");
            _logger.Log(Status.Info, $"Loaded Gold loyalty account: {_email}");
        }

        [When("I enter my email and password")]
        public void EnterEmailAndPassword()
        {
            _signInPage.EnterEmailOrLoyaltyAccountNumber(_logger, _email);
            _signInPage.EnterPassword(_logger, _password);
            _webDriver.LogScreenshot(_logger, "Credentials entered");
        }

        [When("I enter my loyalty account number and password")]
        public void EnterLoyaltyAccountAndPassword()
        {
            var loyaltyDetails = RetrieveFromStorageTable.FetchLoyaltyAccountInfoExcludingPoints("Gold");
            _signInPage.EnterEmailOrLoyaltyAccountNumber(_logger, loyaltyDetails["rowKey"]);
            _signInPage.EnterPassword(_logger, _password);
            _webDriver.LogScreenshot(_logger, "Loyalty account credentials entered");
        }

        [And("I click Log In")]
        public void ClickLogIn()
        {
            _signInPage.ClickLogIn(_logger);
        }

        [And("I verify with the OTP code")]
        public async Task VerifyWithOtp()
        {
            int otp = 0;
            await Core.Xunit.RetryHelper.RunWithRetryAsync(async () =>
            {
                otp = await EnrolmentHelper.GetOTPByEmailId(_otpTableService, _email.ToLower());
                Assert.True(otp != 0, "OTP should not be zero");
                Assert.True(otp != _previousOtp, $"OTP has not changed yet (still {_previousOtp})");
            }, maxRetries: 20, delay: 3000);

            _webDriver.CheckAccessibility(_logger);
            _otpPage.EnterOtp(_logger, otp);
            _webDriver.LogScreenshot(_logger, "OTP entered");
            _otpPage.ClickVerify(_logger);
        }

        [Then("I should see the connected message")]
        public void AssertConnectedMessage()
        {
            string connectedMessage = _signInPage.GetConnectedMessage(_logger);
            _webDriver.LogScreenshotOnPass(_logger, "Connected message displayed");
            Assert.False(string.IsNullOrEmpty(connectedMessage), "Connected message should be displayed");
        }

        // ── Sign Up Steps ──

        [And("I navigate to the sign up page")]
        public void NavigateToSignUp()
        {
            _signInPage.ClickJoinNow(_logger);
            _webDriver.LogScreenshot(_logger, "Sign up page loaded");
            _webDriver.CheckAccessibility(_logger);
        }

        [When("I enter my first name, last name and date of birth as an adult")]
        public void EnterAdultDetails()
        {
            _firstName = $"First{DataGenerator.RandomString(5)}";
            _lastName = $"Last{DataGenerator.RandomString(5)}";
            _dob = DateTime.Now.AddYears(-25).ToString("dd/MM/yyyy");
            _email = DataGenerator.RandomString(10) + "@example.com";

            _signUpPage.EnterFirstName(_logger, _firstName);
            _signUpPage.EnterLastName(_logger, _lastName);
            _signUpPage.EnterDateOfBirth(_logger, _dob);
        }

        [When("I enter my first name, last name and date of birth as under 18")]
        public void EnterUnder18Details()
        {
            _firstName = $"First{DataGenerator.RandomString(5)}";
            _lastName = $"Last{DataGenerator.RandomString(5)}";
            _dob = DateTime.Now.AddYears(-15).ToString("dd/MM/yyyy");

            _signUpPage.EnterFirstName(_logger, _firstName);
            _signUpPage.EnterLastName(_logger, _lastName);
            _signUpPage.EnterDateOfBirth(_logger, _dob);
            _webDriver.LogScreenshot(_logger, "Details entered");
        }

        [And("I click Next")]
        public void ClickNext()
        {
            _signUpPage.Next(_logger);
        }

        [And("I enter my email address")]
        public void EnterSignUpEmail()
        {
            _signUpPage.EnterEmail(_logger, _email);
            _webDriver.LogScreenshot(_logger, "Sign up details entered");
        }

        [And("I submit the sign up form")]
        public void SubmitSignUp()
        {
            _signUpPage.Submit(_logger);
        }

        [And("I verify with the signup OTP code")]
        public async Task VerifyWithSignupOtp()
        {
            int otp = 0;
            await Core.Xunit.RetryHelper.RunWithRetryAsync(async () =>
            {
                otp = await EnrolmentHelper.GetOTPByEmailId(_otpTableService, _email.ToLower());
                Assert.True(otp != 0, "OTP should not be zero");
            }, maxRetries: 20, delay: 3000);

            _otpPage.EnterOtp(_logger, otp);
            _webDriver.LogScreenshot(_logger, "OTP entered for sign up");
            _otpPage.ClickVerify(_logger);
        }

        [And("I click Connect Now")]
        public void ClickConnectNow()
        {
            _signUpPage.ConnectNow(_logger);
        }

        [Then("I should see the connected message after sign up")]
        public void AssertConnectedAfterSignUp()
        {
            string connectedMessage = _signUpPage.GetConnectedMessage(_logger);
            _webDriver.LogScreenshotOnPass(_logger, "Sign up successful - connected");
            Assert.False(string.IsNullOrEmpty(connectedMessage), "Connected message should be displayed after sign up");
        }

        // ── Password Reset Steps ──

        [When("I navigate to the password reset page")]
        public async Task NavigateToPasswordReset()
        {
            _previousOtp = await EnrolmentHelper.GetOTPByEmailId(_otpTableService, _email.ToLower());
            _signInPage.ClickPasswordReset(_logger);
            _webDriver.LogScreenshot(_logger, "Password reset page loaded");
            _webDriver.CheckAccessibility(_logger);
        }

        [And("I enter my email for password reset")]
        public void EnterPasswordResetEmail()
        {
            _passwordResetPage.EnterEmail(_logger, _email);
            _webDriver.LogScreenshot(_logger, "Email entered for password reset");
        }

        [And("I submit the password reset email")]
        public void SubmitPasswordResetEmail()
        {
            _passwordResetPage.SubmitEmail(_logger);
        }

        [And("I enter a new password")]
        public void EnterNewPassword()
        {
            _webDriver.CheckAccessibility(_logger);
            _passwordResetPage.EnterNewPassword(_logger, "Starlink123");
        }

        [And("I confirm the new password")]
        public void ConfirmNewPassword()
        {
            _passwordResetPage.ConfirmNewPassword(_logger, "Starlink123");
            _webDriver.LogScreenshot(_logger, "New password entered");
        }

        [And("I click Change Password")]
        public void ClickChangePassword()
        {
            _passwordResetPage.ClickChangePassword(_logger);
        }

        // ── Sign In Validation Steps ──

        [When("I tab out of the email field without entering anything")]
        public void TabOutOfEmptyEmail()
        {
            _signInPage.ClearEmail(_logger);
        }

        [When("I enter an invalid email address")]
        public void EnterInvalidEmail()
        {
            _signInPage.EnterEmailOrLoyaltyAccountNumber(_logger, "not-an-email!@#");
        }

        [And("I tab out of the email field")]
        public void TabOutOfEmail()
        {
            _signInPage.TabOutOfEmail(_logger);
        }

        [When("I enter a valid email address")]
        public void EnterValidEmail()
        {
            _signInPage.EnterEmailOrLoyaltyAccountNumber(_logger, "test@example.com");
        }

        [And("I clear the password field and tab away")]
        public void ClearPasswordAndTab()
        {
            _signInPage.EnterPassword(_logger, "a");
            _signInPage.ClearPassword(_logger);
        }

        [When("I enter a non-existent email and password")]
        public void EnterNonExistentCredentials()
        {
            _signInPage.EnterEmailOrLoyaltyAccountNumber(_logger, "nonexistent@example.com");
            _signInPage.EnterPassword(_logger, "Password123!");
        }

        [Then("I should see an email validation error")]
        public void AssertEmailValidationError()
        {
            string error = _signInPage.GetEmailValidationError(_logger);
            _webDriver.LogScreenshot(_logger, "Email validation error displayed");
            Assert.False(string.IsNullOrEmpty(error), "Email validation error should be displayed");
        }

        [Then("I should see a password validation error")]
        public void AssertPasswordValidationError()
        {
            string error = _signInPage.GetPasswordValidationError(_logger);
            _webDriver.LogScreenshot(_logger, "Password validation error displayed");
            Assert.False(string.IsNullOrEmpty(error), "Password validation error should be displayed");
        }

        [Then("I should see an account not found error")]
        public void AssertAccountNotFoundError()
        {
            string error = _signInPage.GetAccountNotFoundError(_logger);
            _webDriver.LogScreenshot(_logger, "Account not found error displayed");
            Assert.False(string.IsNullOrEmpty(error), "Account not found error should be displayed");
        }

        // ── Sign Up Validation Steps ──

        [When("I clear the first name field and tab away")]
        public void ClearFirstNameAndTab()
        {
            _signUpPage.EnterFirstName(_logger, "a");
            _signUpPage.ClearFirstName(_logger);
        }

        [When("I clear the last name field and tab away")]
        public void ClearLastNameAndTab()
        {
            _signUpPage.EnterLastName(_logger, "a");
            _signUpPage.ClearLastName(_logger);
        }

        [When("I clear the date of birth field and tab away")]
        public void ClearDateOfBirthAndTab()
        {
            _signUpPage.EnterDateOfBirth(_logger, "01/01/2000");
            _signUpPage.ClearDateOfBirth(_logger);
        }

        [Then("I should see a first name validation error")]
        public void AssertFirstNameValidationError()
        {
            string error = _signUpPage.GetValidationError(_logger, WifiSignUp.firstNameRequiredError, "First name required");
            _webDriver.LogScreenshot(_logger, "First name validation error");
            Assert.False(string.IsNullOrEmpty(error), "First name validation error should be displayed");
        }

        [Then("I should see a last name validation error")]
        public void AssertLastNameValidationError()
        {
            string error = _signUpPage.GetValidationError(_logger, WifiSignUp.lastNameRequiredError, "Last name required");
            _webDriver.LogScreenshot(_logger, "Last name validation error");
            Assert.False(string.IsNullOrEmpty(error), "Last name validation error should be displayed");
        }

        [Then("I should see a date of birth validation error")]
        public void AssertDateOfBirthValidationError()
        {
            // The field may show "Date of birth is required" or "Enter a valid date" depending on
            // whether the field was dirty before clearing. Accept either validation message.
            try
            {
                string error = _signUpPage.GetValidationError(_logger, WifiSignUp.dobRequiredError, "Date of birth required");
                _webDriver.LogScreenshot(_logger, "Date of birth validation error");
                Assert.False(string.IsNullOrEmpty(error), "Date of birth validation error should be displayed");
            }
            catch (NoSuchElementException)
            {
                string error = _signUpPage.GetValidationError(_logger, WifiSignUp.dobInvalidError, "Date of birth invalid");
                _webDriver.LogScreenshot(_logger, "Date of birth validation error");
                Assert.False(string.IsNullOrEmpty(error), "Date of birth validation error should be displayed");
            }
        }

        // ── Password Reset Validation Steps ──

        [Given("I am on the password reset entry screen")]
        public async Task NavigateToPasswordResetScreen()
        {
            var loyaltyDetails = RetrieveFromStorageTable.FetchLoyaltyAccountInfoExcludingPoints("Gold");
            _email = loyaltyDetails["email"];

            int previousOtp = await EnrolmentHelper.GetOTPByEmailId(_otpTableService, _email.ToLower());

            _signInPage.ClickYesIAgree(_logger);
            _signInPage.ClickPasswordReset(_logger);
            _webDriver.LogScreenshot(_logger, "Password reset page loaded");
            _webDriver.CheckAccessibility(_logger);

            _passwordResetPage.EnterEmail(_logger, _email);
            _passwordResetPage.SubmitEmail(_logger);

            int otp = 0;
            await Core.Xunit.RetryHelper.RunWithRetryAsync(async () =>
            {
                otp = await EnrolmentHelper.GetOTPByEmailId(_otpTableService, _email.ToLower());
                Assert.True(otp != 0, "OTP should not be zero");
                Assert.True(otp != previousOtp, $"OTP has not changed yet (still {previousOtp})");
            }, maxRetries: 20, delay: 3000);

            _otpPage.EnterOtp(_logger, otp);
            _otpPage.ClickVerify(_logger);
            _webDriver.LogScreenshot(_logger, "OTP verified - password entry screen reached");
            _webDriver.CheckAccessibility(_logger);
        }

        [When("I enter \"(.+)\" as the new password")]
        public void EnterSpecificNewPassword(string password)
        {
            _passwordResetPage.EnterNewPassword(_logger, password);
            _passwordResetPage.TabOutOfNewPassword(_logger);
        }

        [And("I enter \"(.+)\" as the confirm password")]
        public void EnterSpecificConfirmPassword(string password)
        {
            _passwordResetPage.ConfirmNewPassword(_logger, password);
            _passwordResetPage.TabOutOfConfirmPassword(_logger);
        }

        [Then("the length rule should be met")]
        public void AssertLengthRuleMet()
        {
            Assert.True(_passwordResetPage.IsPasswordRuleMet(_logger, WifiPasswordReset.passwordLengthRule, "Length"), "Length rule should be met");
        }

        [Then("the length rule should not be met")]
        public void AssertLengthRuleNotMet()
        {
            Assert.False(_passwordResetPage.IsPasswordRuleMet(_logger, WifiPasswordReset.passwordLengthRule, "Length"), "Length rule should not be met");
        }

        [And("the number rule should be met")]
        public void AssertNumberRuleMet()
        {
            Assert.True(_passwordResetPage.IsPasswordRuleMet(_logger, WifiPasswordReset.passwordNumberRule, "Number"), "Number rule should be met");
        }

        [Then("the number rule should not be met")]
        public void AssertNumberRuleNotMet()
        {
            Assert.False(_passwordResetPage.IsPasswordRuleMet(_logger, WifiPasswordReset.passwordNumberRule, "Number"), "Number rule should not be met");
        }

        [And("the uppercase rule should be met")]
        public void AssertUppercaseRuleMet()
        {
            Assert.True(_passwordResetPage.IsPasswordRuleMet(_logger, WifiPasswordReset.passwordUppercaseRule, "Uppercase"), "Uppercase rule should be met");
        }

        [Then("the uppercase rule should not be met")]
        public void AssertUppercaseRuleNotMet()
        {
            Assert.False(_passwordResetPage.IsPasswordRuleMet(_logger, WifiPasswordReset.passwordUppercaseRule, "Uppercase"), "Uppercase rule should not be met");
        }

        [And("the lowercase rule should be met")]
        public void AssertLowercaseRuleMet()
        {
            Assert.True(_passwordResetPage.IsPasswordRuleMet(_logger, WifiPasswordReset.passwordLowercaseRule, "Lowercase"), "Lowercase rule should be met");
        }

        [Then("the lowercase rule should not be met")]
        public void AssertLowercaseRuleNotMet()
        {
            Assert.False(_passwordResetPage.IsPasswordRuleMet(_logger, WifiPasswordReset.passwordLowercaseRule, "Lowercase"), "Lowercase rule should not be met");
        }

        [And("the special character rule should be met")]
        public void AssertSpecialCharRuleMet()
        {
            Assert.True(_passwordResetPage.IsPasswordRuleMet(_logger, WifiPasswordReset.passwordSpecialCharsRule, "Special character"), "Special character rule should be met");
        }

        [Then("the special character rule should not be met")]
        public void AssertSpecialCharRuleNotMet()
        {
            Assert.False(_passwordResetPage.IsPasswordRuleMet(_logger, WifiPasswordReset.passwordSpecialCharsRule, "Special character"), "Special character rule should not be met");
        }

        [Then("the passwords match rule should not be met")]
        public void AssertPasswordsMatchRuleNotMet()
        {
            Assert.False(_passwordResetPage.IsPasswordRuleMet(_logger, WifiPasswordReset.passwordsMatchRule, "Passwords match"), "Passwords match rule should not be met");
        }

        [Then("all password rules should be met")]
        public void AssertAllPasswordRulesMet()
        {
            Assert.True(_passwordResetPage.IsPasswordRuleMet(_logger, WifiPasswordReset.passwordLengthRule, "Length"), "Length rule should be met");
            Assert.True(_passwordResetPage.IsPasswordRuleMet(_logger, WifiPasswordReset.passwordNumberRule, "Number"), "Number rule should be met");
            Assert.True(_passwordResetPage.IsPasswordRuleMet(_logger, WifiPasswordReset.passwordUppercaseRule, "Uppercase"), "Uppercase rule should be met");
            Assert.True(_passwordResetPage.IsPasswordRuleMet(_logger, WifiPasswordReset.passwordLowercaseRule, "Lowercase"), "Lowercase rule should be met");
            Assert.True(_passwordResetPage.IsPasswordRuleMet(_logger, WifiPasswordReset.passwordSpecialCharsRule, "Special character"), "Special character rule should be met");
            Assert.True(_passwordResetPage.IsPasswordRuleMet(_logger, WifiPasswordReset.passwordsMatchRule, "Passwords match"), "Passwords match rule should be met");
            _webDriver.LogScreenshotOnPass(_logger, "All password rules met");
        }

        public void OnFailure(Exception ex)
        {
            _webDriver?.LogFailureDiagnostics(_logger, $"Step failed: {ex.Message}");
        }

        public void Dispose()
        {
            _webDriver?.DriverCleanUp();
        }
    }
}
