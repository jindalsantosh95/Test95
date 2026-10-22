using System.Net;
using System.Net.Http.Headers;
using System.Text;
using VAA.EAPI.Automation.Core.Extensions;
using Xunit;

namespace VAA.EAPI.Automation.Core.Tests
{
    public sealed class HttpResponseMessageExtensionsTests
    {
        [Fact]
        public void EnsureIsSuccessStatusCode_RedactsSensitiveRequestHeaders()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://example.test/payment")
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "authorization-secret");
            request.Headers.TryAddWithoutValidation("Ocp-Apim-Subscription-Key", "subscription-secret");
            request.Headers.TryAddWithoutValidation("api-channel", "Automation");

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                RequestMessage = request,
                Content = new StringContent("downstream failure")
            };

            Exception exception = Assert.ThrowsAny<Exception>(() => response.EnsureIsSuccessStatusCode());

            Assert.DoesNotContain("authorization-secret", exception.Message);
            Assert.DoesNotContain("subscription-secret", exception.Message);
            Assert.Contains("Authorization:[REDACTED]", exception.Message);
            Assert.Contains("Ocp-Apim-Subscription-Key:[REDACTED]", exception.Message);
            Assert.Contains("api-channel:Automation", exception.Message);
            Assert.Contains("downstream failure", exception.Message);
        }
    }
}
