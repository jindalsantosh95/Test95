using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Book.Constants;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MyAccount.Helpers;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.GraphQL.EndtoEnd
{
    public class E2EGraphQL : IClassFixture<ReportFixture>, IClassFixture<TokenFixture>
    {
        private readonly TokenFixture _tokenFixture;
        private readonly TestConfig _testConfig;
        private readonly ReportFixture _reportFixture;
        private readonly ITestOutputHelper _output;
        private readonly LoyaltyServiceHelper loyaltyServiceHelper;
        private readonly E2ETestHelper _helper;

        public E2EGraphQL(ReportFixture reportFixture, TokenFixture tokenFixture, ITestOutputHelper output)
        {
            _reportFixture = reportFixture;
            _tokenFixture = tokenFixture;
            _output = output;
            BootstrapBookLocalSettings();
            _testConfig = new TestConfig();
            loyaltyServiceHelper = new LoyaltyServiceHelper(_tokenFixture);
            _helper = new E2ETestHelper(_testConfig, _tokenFixture, loyaltyServiceHelper, output);
        }

        private static void BootstrapBookLocalSettings()
        {
            ConfigReader.LoadEnvironmentSettingsFile(optional: true, forceOverride: true, root: AppContext.BaseDirectory);
        }

        private static void EnsureOrderDocumentKeyVaultAccessForE2E()
        {
            const string orderDocumentSecretName = "TESTING-CmdbOrderDocument-Automation-Key";
            string secretValue = Vault.Default.GetSecretValue(orderDocumentSecretName);

            if (!secretValue.IsNullOrWhiteSpace())
            {
                return;
            }

            string keyVaultUrl = ConfigReader.Get(VAA.EAPI.Automation.Core.Constants.Environment.KeyVaultUrl);
            throw new InvalidOperationException(
                $"Key Vault preflight failed for Book E2E. Missing/unreadable secret(s): {orderDocumentSecretName}. " +
                $"Environment='{Config.Environment}', KeyVaultUrl='{keyVaultUrl ?? "<null>"}'.");
        }

        [Theory]
        [MemberData(nameof(E2ETestData.AllScenarios), MemberType = typeof(E2ETestData))]
        public Task EndToEndBooking(E2EScenario scenario) => RunScenario(scenario);

        [Theory]
        [Trait("Category", Traits.BookGraphQLPSSITSanity)]
        [MemberData(nameof(E2ETestData.SanityScenarios), MemberType = typeof(E2ETestData))]
        public Task SanityEndToEndBooking(E2EScenario scenario) => RunScenario(scenario);

        [Theory]
        [Trait("Category", Traits.BookGraphQLRegression)]
        [MemberData(nameof(E2ETestData.RegressionScenarios), MemberType = typeof(E2ETestData))]
        public Task RegressionEndToEndBooking(E2EScenario scenario) => RunScenario(scenario);

        [Theory]
        [Trait("Category", Traits.BookE2ERegression)]
        [MemberData(nameof(E2ETestData.E2ERegressionScenarios), MemberType = typeof(E2ETestData))]
        public Task E2ERegressionEndToEndBooking(E2EScenario scenario) => RunScenario(scenario);

        [Theory]
        [Trait("Category", Traits.BookCoreRegression)]
        [MemberData(nameof(E2ETestData.CoreRegressionScenarios), MemberType = typeof(E2ETestData))]
        public Task CoreRegressionEndToEndBooking(E2EScenario scenario) => RunScenario(scenario);

        [Theory]
        [Trait("Category", Traits.BookFunctional)]
        [MemberData(nameof(E2ETestData.FunctionalScenarios), MemberType = typeof(E2ETestData))]
        public Task FunctionalEndToEndBooking(E2EScenario scenario) => RunScenario(scenario);

        [Theory]
        [Trait("Category", Traits.SharedServicesSignOff)]
        [Trait("SharedService", "OfferMS")]
        [Trait("SharedService", "ReferenceMS")]
        [Trait("SharedService", "Retail.PaymentMS")]
        [Trait("SharedService", "Retail.OrderMgmtMS")]
        [Trait("SharedServiceConsumer", "Book")]
        [MemberData(nameof(E2ETestData.SharedServicesSignOffScenarios), MemberType = typeof(E2ETestData))]
        public Task SharedServicesSignOffBooking(E2EScenario scenario) => RunScenario(scenario);

        [Fact]
        [Trait("Category", Traits.BookAdHoc)]
        public async Task AdHocEndToEndBooking()
        {
            if (System.Environment.GetEnvironmentVariable(E2ETestData.AdHocScenarioFileEnvironmentVariable)
                .IsNullOrWhiteSpace())
            {
                _output.WriteLine(
                    $"Book ad-hoc run skipped: {E2ETestData.AdHocScenarioFileEnvironmentVariable} is not set.");
                return;
            }

            foreach (object[] data in E2ETestData.AllScenarios())
            {
                await RunScenario((E2EScenario)data[0]);
            }
        }

        private async Task RunScenario(E2EScenario scenario)
        {
            if (!IsCoreRegressionScenario(scenario))
            {
                EnsureOrderDocumentKeyVaultAccessForE2E();
            }

            // 1. Configure TestConfig from scenario
            _helper.ConfigureTestConfig(scenario);

            // 2. Loyalty/B2C setup
            string b2cToken = null;
            string fcMembershipNumber = null;
            Dictionary<string, string> loyaltyDetails = null;
            int fqTPointsBeforeBooking = 0;
            dynamic offerPriceResult = null;

            bool needsUpfrontLoyalty = scenario.BookingType == BookingType.Redemption
                || scenario.IsCoBrand
                || scenario.FlyingClubPassenger
                || scenario.IsLoggedIn
                || scenario.ValidateVoucherConfirmation;
            if (needsUpfrontLoyalty)
                (b2cToken, fcMembershipNumber, loyaltyDetails, fqTPointsBeforeBooking) = await _helper.SetupLoyaltyAndLogin(scenario);

            // 3. Search
            string basketId = null;
            (basketId, offerPriceResult) = await _helper.PerformSearch(scenario, b2cToken);
            if (!scenario.ValidateVoucherConfirmation)
                _helper.ValidateExpectedLegOperatingAirline(scenario, offerPriceResult, "offer price");

            // 4. Retrieve Basket
            (HttpResponseMessage retrieveBasketResponse, dynamic retrieveBasketResult) =
                await RunAdHocTransientReadWithRetry(
                    () => _helper.RetrieveBasket(basketId, b2cToken),
                    "retrieve basket");
            _helper.ValidateExpectedLegOperatingAirline(scenario, retrieveBasketResult, "retrieve basket");

            if (scenario.ValidateVoucherConfirmation)
            {
                await _helper.ApplyVoucherSelection(scenario, basketId, b2cToken, (JToken)offerPriceResult);
                (retrieveBasketResponse, retrieveBasketResult) = await RunAdHocTransientReadWithRetry(
                    () => _helper.RetrieveBasket(basketId, b2cToken),
                    "retrieve basket after voucher selection");
            }

            // 5. Validate basket for redemption
            if (scenario.ValidateRedemptionBasket && scenario.BookingType == BookingType.Redemption)
                _helper.ValidateRedemptionBasket(offerPriceResult, retrieveBasketResult, loyaltyDetails, basketId);

            // 6. Insurance
            JToken insuranceQuoteResult = null;
            JToken insuranceDecisionResult = null;
            if (scenario.Insurance != InsuranceAction.None)
                (insuranceQuoteResult, insuranceDecisionResult) = await _helper.HandleInsurance(scenario, basketId, b2cToken);

            // 7. Passenger/Contact/T&C
            (string sectorId, HttpResponseMessage updatedBasketResponse) = await _helper.UpdateBasketDetails(scenario, (object)retrieveBasketResult, basketId, b2cToken, (object)offerPriceResult);
            retrieveBasketResponse = updatedBasketResponse;

            // 8. Insurance price validation
            JToken afterInsuranceBasketResponse = null;
            if (scenario.Insurance != InsuranceAction.None)
                afterInsuranceBasketResponse = await _helper.ValidateInsurancePrices(scenario, basketId, b2cToken, insuranceQuoteResult, insuranceDecisionResult);

            // 9. Seats
            double totalSeatPrice = 0;
            if (scenario.SeatMode != SeatMode.None)
                (totalSeatPrice, _) = await _helper.HandleSeatSelection(scenario, basketId, b2cToken, sectorId, retrieveBasketResponse);
            else if (scenario.HasPaidSeat || scenario.HasFreeSeat)
                await _helper.HandlePaidSeatForInsuranceTests(scenario, basketId, b2cToken, retrieveBasketResponse);

            // 10. Points converter (PWP)
            string selectedPoints = null;
            if (scenario.BookingType == BookingType.PayWithPoints)
                (selectedPoints, b2cToken, fcMembershipNumber) = await _helper.HandlePointsConverter(basketId, b2cToken, fqTPointsBeforeBooking, scenario, fcMembershipNumber, retrieveBasketResponse);

            // 11. CoBrand/Synchrony
            string synchronyToken = null;
            if (scenario.IsCoBrand)
                synchronyToken = await _helper.HandleCoBrand(scenario, basketId, b2cToken);

            // 12. Get final basket state for price validation (must be AFTER seats so totals include seat charges)
            if (scenario.ValidateInsurancePriceWithPaymentMS || scenario.ExpectFraudCheck || scenario.BookingType == BookingType.PayWithPoints || scenario.HasPaidSeat || scenario.ValidateVoucherConfirmation || scenario.ValidateRedemptionBasket)
                afterInsuranceBasketResponse = await _helper.GetFinalBasketState(basketId, b2cToken);

            // 13. Payment
            bool stopAfterCardDetails = await _helper.HandlePayment(scenario, basketId, b2cToken, selectedPoints, synchronyToken, afterInsuranceBasketResponse);
            if (stopAfterCardDetails)
                return;

            // 14. Validate payment MS prices if needed
            if (scenario.ValidateInsurancePriceWithPaymentMS)
                await _helper.ValidatePaymentMSPrices(scenario, afterInsuranceBasketResponse);

            // 15. Order Create
            await _helper.HandleOrderCreate(scenario, basketId, b2cToken, fcMembershipNumber, afterInsuranceBasketResponse, selectedPoints);

            // 16. Post-order
            await _helper.PostOrderValidations(scenario, basketId, b2cToken, fcMembershipNumber, afterInsuranceBasketResponse);
        }

        private static bool IsCoreRegressionScenario(E2EScenario scenario)
        {
            if (scenario?.Categories == null)
            {
                return false;
            }

            foreach (string category in scenario.Categories)
            {
                if (category.Is(Traits.BookCoreRegression))
                {
                    return true;
                }
            }

            return false;
        }

        private async Task<T> RunAdHocTransientReadWithRetry<T>(Func<Task<T>> operation, string operationName)
        {
            bool isAdHocRun = !System.Environment.GetEnvironmentVariable(
                E2ETestData.AdHocScenarioFileEnvironmentVariable).IsNullOrWhiteSpace();
            int maxAttempts = isAdHocRun ? 3 : 1;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    return await operation();
                }
                catch (Exception exception) when (attempt < maxAttempts && IsTransientReadFailure(exception))
                {
                    int delaySeconds = (int)Math.Pow(2, attempt - 1);
                    _output.WriteLine(
                        $"Book ad-hoc transient retry: Operation={operationName} | Attempt={attempt}/{maxAttempts} | DelaySeconds={delaySeconds} | Error={exception.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                }
            }

            throw new InvalidOperationException($"Unexpected retry state for Book operation '{operationName}'.");
        }

        private static bool IsTransientReadFailure(Exception exception)
        {
            if (exception is HttpRequestException || exception is TaskCanceledException || exception is TimeoutException)
            {
                return true;
            }

            string message = exception.ToString();
            return message.Contains("429", StringComparison.OrdinalIgnoreCase)
                || message.Contains("502", StringComparison.OrdinalIgnoreCase)
                || message.Contains("503", StringComparison.OrdinalIgnoreCase)
                || message.Contains("504", StringComparison.OrdinalIgnoreCase)
                || message.Contains("TooManyRequests", StringComparison.OrdinalIgnoreCase)
                || message.Contains("BadGateway", StringComparison.OrdinalIgnoreCase)
                || message.Contains("ServiceUnavailable", StringComparison.OrdinalIgnoreCase)
                || message.Contains("GatewayTimeout", StringComparison.OrdinalIgnoreCase)
                || message.Contains("timed out", StringComparison.OrdinalIgnoreCase)
                || message.Contains("connection reset", StringComparison.OrdinalIgnoreCase);
        }
    }
}
