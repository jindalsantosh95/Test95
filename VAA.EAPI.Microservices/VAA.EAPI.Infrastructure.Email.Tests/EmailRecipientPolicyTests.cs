using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Email;
using VAA.EAPI.Infrastructure.Email.Entities;
using VAA.EAPI.Infrastructure.Email.Interface;
using VAA.EAPI.Infrastructure.Email.Policies;
using VAA.EAPI.Infrastructure.Email.Repository;
using Xunit;

namespace VAA.EAPI.Infrastructure.Email.Tests
{
    public class EmailRecipientPolicyTests
    {
        [Theory]
        [InlineData("1234567890abcdef1234567890abcdef@U18.virgin.com")]
        [InlineData("1234567890abcdef1234567890abcdef@u18.VIRGIN.com")]
        [InlineData(" 1234567890abcdef1234567890abcdef@U18.virgin.com ")]
        public void ShouldSuppress_ReturnsTrue_ForYoungFlyerDummyDomain(string emailAddress)
        {
            Assert.True(EmailRecipientPolicy.ShouldSuppress(emailAddress));
        }

        [Theory]
        [InlineData("parent@example.com")]
        [InlineData("u18.virgin.com@example.com")]
        [InlineData("child@notu18.virgin.com")]
        [InlineData("")]
        [InlineData(null)]
        public void ShouldSuppress_ReturnsFalse_ForOtherAddresses(string? emailAddress)
        {
            Assert.False(EmailRecipientPolicy.ShouldSuppress(emailAddress));
        }

        [Fact]
        public void ShouldSuppressAcmRequest_ReturnsTrue_WhenRtEventTargetsDummyDomain()
        {
            const string request = """
                <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/">
                  <soapenv:Body>
                    <PushEvent>
                      <rtEvent email="1234567890abcdef1234567890abcdef@U18.virgin.com" />
                    </PushEvent>
                  </soapenv:Body>
                </soapenv:Envelope>
                """;

            Assert.True(EmailRecipientPolicy.ShouldSuppressAcmRequest(request));
        }

        [Fact]
        public void ShouldSuppressAcmRequest_ReturnsTrue_WhenRtEventIsDocumentRoot()
        {
            const string request = "<rtEvent email=\"1234567890abcdef1234567890abcdef@U18.virgin.com\" />";

            Assert.True(EmailRecipientPolicy.ShouldSuppressAcmRequest(request));
        }

        [Theory]
        [InlineData("<rtEvent email=\"parent@example.com\" />")]
        [InlineData("not xml")]
        [InlineData("")]
        public void ShouldSuppressAcmRequest_ReturnsFalse_WhenRequestDoesNotTargetDummyDomain(string request)
        {
            Assert.False(EmailRecipientPolicy.ShouldSuppressAcmRequest(request));
        }

        [Fact]
        public async Task CcpRepository_DoesNotCallCcp_WhenRecipientUsesDummyDomain()
        {
            var client = new RecordingCcpClient();
            var repository = new CcpRepository(NullLogger<CcpRepository>.Instance, client);

            var result = await repository.SendEmail(new SendEmailRequest
            {
                To = "1234567890abcdef1234567890abcdef@U18.virgin.com"
            });

            Assert.True(result);
            Assert.Equal(0, client.CallCount);
        }

        [Fact]
        public async Task CcpRepository_ThrowsArgumentNullException_WhenRequestIsNull()
        {
            var repository = new CcpRepository(NullLogger<CcpRepository>.Instance, new RecordingCcpClient());

            var exception = await Assert.ThrowsAsync<System.ArgumentNullException>(() => repository.SendEmail(null));

            Assert.Equal("request", exception.ParamName);
        }

        private sealed class RecordingCcpClient : ICcpClient
        {
            public int CallCount { get; private set; }

            public Task<KeyValuePair<bool, string>> SendEmail(CcpRequest customerCommRequest)
            {
                CallCount++;
                return Task.FromResult(new KeyValuePair<bool, string>(true, "{\"Success\":true}"));
            }
        }
    }
}
