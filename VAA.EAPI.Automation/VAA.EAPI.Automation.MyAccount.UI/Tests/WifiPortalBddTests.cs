using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.Core.UI.Xunit.Features;
using VAA.EAPI.Automation.Core.UI.Xunit.Features.Atttributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MyAccount.Models.Enrolment;
using VAA.EAPI.Automation.MyAccount.UI.StepDefinitions.WifiPortal;
using VAA.EAPI.Infrastructure.AzureStorage;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MyAccount.UI.Tests
{
    public class WifiPortalBddTests : IClassFixture<ReportFixture>, IClassFixture<MemoryStoreFixture>, IClassFixture<TableStorageService<OtpRequestTableEntity>>
    {
        private readonly ReportFixture _reportFixture;
        private readonly MemoryStoreFixture _memoryStore;
        private readonly TableStorageService<OtpRequestTableEntity> _otpTableService;
        private readonly ITestOutputHelper _output;

        public WifiPortalBddTests(ReportFixture reportFixture, MemoryStoreFixture memoryStore, TableStorageService<OtpRequestTableEntity> otpTableService, ITestOutputHelper output)
        {
            _reportFixture = reportFixture;
            _memoryStore = memoryStore;
            _otpTableService = otpTableService;
            _output = output;
        }

        [Theory]
        [Trait("Category", "WifiPortalBDD")]
        [FeatureFileData<WifiPortalSteps>("Features/WifiPortal/SignIn.feature")]
        public async Task SignIn(Scenario scenario)
        {
            _output.WriteLine($"BDD Scenario: {scenario.Name}");
            await scenario.Run(scenario.Feature.Name, scenario.Name, _reportFixture, _memoryStore, _otpTableService);
        }

        [Theory]
        [Trait("Category", "WifiPortalBDD")]
        [FeatureFileData<WifiPortalSteps>("Features/WifiPortal/SignUp.feature")]
        public async Task SignUp(Scenario scenario)
        {
            _output.WriteLine($"BDD Scenario: {scenario.Name}");
            await scenario.Run(scenario.Feature.Name, scenario.Name, _reportFixture, _memoryStore, _otpTableService);
        }

        [Theory]
        [Trait("Category", "WifiPortalBDD")]
        [FeatureFileData<WifiPortalSteps>("Features/WifiPortal/PasswordReset.feature")]
        public async Task PasswordReset(Scenario scenario)
        {
            _output.WriteLine($"BDD Scenario: {scenario.Name}");
            await scenario.Run(scenario.Feature.Name, scenario.Name, _reportFixture, _memoryStore, _otpTableService);
        }

        [Theory]
        [Trait("Category", "WifiPortalBDD")]
        [FeatureFileData<WifiPortalSteps>("Features/WifiPortal/SignInValidation.feature")]
        public async Task SignInValidation(Scenario scenario)
        {
            _output.WriteLine($"BDD Scenario: {scenario.Name}");
            await scenario.Run(scenario.Feature.Name, scenario.Name, _reportFixture, _memoryStore, _otpTableService);
        }

        [Theory]
        [Trait("Category", "WifiPortalBDD")]
        [FeatureFileData<WifiPortalSteps>("Features/WifiPortal/SignUpValidation.feature")]
        public async Task SignUpValidation(Scenario scenario)
        {
            _output.WriteLine($"BDD Scenario: {scenario.Name}");
            await scenario.Run(scenario.Feature.Name, scenario.Name, _reportFixture, _memoryStore, _otpTableService);
        }

        [Theory]
        [Trait("Category", "WifiPortalBDD")]
        [FeatureFileData<WifiPortalSteps>("Features/WifiPortal/PasswordResetValidation.feature")]
        public async Task PasswordResetValidation(Scenario scenario)
        {
            _output.WriteLine($"BDD Scenario: {scenario.Name}");
            await scenario.Run(scenario.Feature.Name, scenario.Name, _reportFixture, _memoryStore, _otpTableService);
        }
    }
}
