using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands.WingsMemberAssessment;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.CustomerTransactionMs;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.RuleEngineMs;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Triggers.Timer;
using Xunit;
using CoreXunitHttpClientFactory = VAA.EAPI.Core.Xunit.Http.HttpClientFactory;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class WingsMemberAssessmentTimerTest
    {
        private readonly IServiceProvider _serviceProvider;

        public WingsMemberAssessmentTimerTest()
        {
            ConfigReader.Set(
                VAA.EAPI.Microservices.Loyalty.Member.Management.Constants
                    .Environment.WingsAssessmentBatchSize,
                "100");

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpClientFactory, CoreXunitHttpClientFactory>()
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IOAuthTokenProvider, VAA.EAPI.Core.Xunit.Azure.OAuthTokenProvider>()
                .AddSingleton<ICustomerTransactionTokenProvider, CustomerTransactionTokenProvider>()
                .AddSingleton<MockReportingDbServices>()
                .AddSingleton<IReportingDBServices>(serviceProvider =>
                    serviceProvider.GetRequiredService<MockReportingDbServices>())
                .AddSingleton<AwardDocumentServiceQueryHandler>()
                .AddSingleton<FlyingClubMemberDocumentServiceQueryHandler>()
                .AddSingleton<CustomerTransactionDocumentsServiceQueryHandler>()
                .AddSingleton<IRulesEngineService, RulesEngineService>()
                .AddSingleton<IWingsMemberAssessmentCommand, WingsMemberAssessmentCommandHandler>()
                .AddTransient<WingsMemberAssessmentTimer>()
                .BuildServiceProvider();
        }

        private IWingsMemberAssessmentCommand CommandHandler =>
            _serviceProvider.GetRequiredService<IWingsMemberAssessmentCommand>();

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new WingsMemberAssessmentTimer(CommandHandler, null));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenCommandHandlerIsNull()
        {
            var logger = _serviceProvider.GetRequiredService<ILogger<WingsMemberAssessmentTimer>>();

            Assert.Throws<ArgumentNullException>(() => new WingsMemberAssessmentTimer(null, logger));
        }

        [Fact]
        public void Run_ShouldBeScheduledForTheFirstOfJanuaryMayAndSeptember()
        {
            var runMethod = typeof(WingsMemberAssessmentTimer).GetMethod(nameof(WingsMemberAssessmentTimer.Run));
            var timerParameter = runMethod!.GetParameters()
                .Single(parameter => parameter.GetCustomAttributesData()
                    .Any(attribute => attribute.AttributeType.Name == "TimerTriggerAttribute"));
            var timerTrigger = timerParameter.GetCustomAttributesData()
                .Single(attribute => attribute.AttributeType.Name == "TimerTriggerAttribute");

            Assert.Equal("%WingsMemberAssessmentTimerSchedule%", timerTrigger.ConstructorArguments.Single().Value);
        }

        [Fact]
        public async Task Run_ShouldCompleteSuccessfully()
        {
            var timer = _serviceProvider.GetRequiredService<WingsMemberAssessmentTimer>();
            using var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            await timer.Run(new TimerInfo(), cancellationToken);

            var reportingDbServices =
                _serviceProvider.GetRequiredService<MockReportingDbServices>();

            Assert.Equal(1, reportingDbServices.StageGoldMembersCallCount);
            Assert.Equal(1, reportingDbServices.GetBatchCallCount);
        }

    }
}
