using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Email.Client;
using VAA.EAPI.Infrastructure.Email.Interface;
using Xunit;

namespace VAA.EAPI.Infrastructure.Email.Tests
{
    public class WorkflowClientTests
    {
        private readonly Mock<System.Net.Http.IHttpClientFactory> _mockIHttpClientFactory;
        private readonly Mock<ILogger<WorkflowClient>> _logger;
        private readonly IWorkflowClient _iWorkflowClient;

        public WorkflowClientTests()
        {
            _logger = new Mock<ILogger<WorkflowClient>>();
            _mockIHttpClientFactory = new Mock<System.Net.Http.IHttpClientFactory>();

            ConfigReader.Set("AzureADEndpoint", "https://login.microsoftonline.com/6bdfd5a8-531f-4f14-9af1-dc7b444c3ad8/");
            ConfigReader.Set("WorkFlowClientId", "22121-ssaa-2124-hjj");
            ConfigReader.Set("WorkFlowClientSecret", "fddfdfe-323222323");
            ConfigReader.Set("WorkFlowResource", "3232-KKJJ-32323N-3232d");
            ConfigReader.Set("WorkFlowAPIUri", "https://Testresources.virginatlantic.com/uat/emailcomms/manual/paths/invoke");

        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenHttpClientFactoryIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new WorkflowClient(null, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new WorkflowClient(_mockIHttpClientFactory.Object, null));
        }
    }
}
