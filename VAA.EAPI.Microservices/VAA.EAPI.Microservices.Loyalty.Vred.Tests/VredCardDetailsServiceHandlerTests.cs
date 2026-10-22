using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Framework.Tests;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredCardDetails;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Tests
{
    public class VredCardDetailsServiceHandlerTests
    {
        private readonly Mock<IHttpConnector> _mockApiConnector;

        private Func<HttpRequestMessage, HttpResponseMessage> tokenResponse;
        private Func<HttpRequestMessage, HttpResponseMessage> detailsResponse;
        private readonly VredCardDetailsServiceHandler _handler;

        public VredCardDetailsServiceHandlerTests()
        {
            // Load "mock" settings 
            VRedCommon.CreateTestEnvironmentSettings();
            _mockApiConnector = new Mock<IHttpConnector>();
            _handler = new VredCardDetailsServiceHandler(_mockApiConnector.Object);
        }

        private const string Token = "{" +
                                        "\"access_token\": \"eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6ImJ0NEpTVWs3TE9mRXN4VXBNaE91diJ9.eyJodHRwczovL2FwaS5zdGFnaW5nLmxveWFsdHkudmlyZ2luLmNvbS9jbGFpbXMiOiJwYXJ0bmVyLXJlYWQscGFydG5lci1lYXJuLHBhcnRuZXItc3BlbmQscGFydG5lci1oaWdoLXRydXN0LGV4ZWN1dGVfYXBpOnBhcnRuZXIiLCJodHRwczovL2FwaS5zdGFnaW5nLmxveWFsdHkudmlyZ2luLmNvbS9jbGFpbXMvZGF0YSI6IntcImNvZGVcIjpcIlZBQVwiLFwicmVzb2x2ZVVzZXJJZFwiOltcIlZBQVwiLFwiVlJFRFwiXX0iLCJpc3MiOiJodHRwczovL2xvZ2luLnN0YWdpbmcubG95YWx0eS52aXJnaW4uY29tLyIsInN1YiI6Ikd6RUVYT2RoNXc1QVpEZWplZXhHekt4WFFZTkU2VVBoQGNsaWVudHMiLCJhdWQiOiJodHRwczovL2FwaS5zdGFnaW5nLmxveWFsdHkudmlyZ2luLmNvbS8iLCJpYXQiOjE3MTY0NzQ3ODksImV4cCI6MTcxNjU2MTE4OSwiZ3R5IjoiY2xpZW50LWNyZWRlbnRpYWxzIiwiYXpwIjoiR3pFRVhPZGg1dzVBWkRlamVleEd6S3hYUVlORTZVUGgifQ.zNPLasqBTvrnIsdDn0_JEK_UJKPlAmGU3XxxZquDNiri3_kneHxS8IwdNf8Yg9wESgGnU7q0NXF4VS0tR_UsVrbavh57cbipJsXXkF-F0NcBiMttpaP6xCCLGfMrP__QWbJ7It_PA0z_dQ6bzYCjL0D_zzP7G8s4sUbTlVJl-_MqA0weEeH0BsRkb7VKBSsj5lnXwjQ13lxmnm8F2hiTPoDCOkNGLuUG4gU9dAV-u2j8mhhixFoc6pjuGWgTOzJTS9Ipz0SXJu3S76qZRgdp-FbjwvBas1kPC5a4vdBK9iPELd__7C2ZK2vfX2DU-FJY_jcyjR8eDCqc5wPJT7mkOw\"," +
                                        "\"expires_in\": 86400," +
                                        "\"token_type\": \"Bearer\"" +
                                    "}";

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new VredCardDetailsServiceHandler(null));
        }

        private HttpResponseMessage HttpRequestSimulator(HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            var requestUri = requestMessage.RequestUri;

            if (requestUri.Equals(ConfigReader.Get(VAA.EAPI.Microservices.Loyalty.Vred.Constants.Environment.VRedAuthUrlSettingName)))
            {
                return tokenResponse(requestMessage);
            }
            else if (requestUri.Equals(ConfigReader.Get(VAA.EAPI.Microservices.Loyalty.Vred.Constants.Environment.UpdateVredurlSettingName)))
            {
                return detailsResponse(requestMessage);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public async Task Handle_MemberNumberSucceeds_ShouldReturnSuccessResponse()
        {
            var response = TestDataProvider.GetTestData<VredCardDetailsResponse>("VRedMS\\VredCardDetailsResponse");
            var request = new VredCardDetailsServiceQuery("9106913081");

            _mockApiConnector.Setup(m => m.GetAsync<string>(It.IsAny<string>(),
                                                            It.IsAny<Dictionary<string, string>>(),
                                                            It.IsAny<string>())).ReturnsAsync(Json.Serialize(response));

            _mockApiConnector.Setup(m => m.PostAsync(It.IsAny<string>(),
                                                     It.IsAny<HttpContent>(),
                                                     It.IsAny<Dictionary<string, string>>(),
                                                     It.IsAny<string>())).ReturnsAsync(new HttpResponseMessage()
                                                     {
                                                         StatusCode = System.Net.HttpStatusCode.OK,
                                                         Content = new StringContent(Token, System.Text.Encoding.UTF8, "application/json")
                                                     });

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.IsType<VredCardDetailsResponse>(result);
            Assert.True(result.CardLinks.Count() >= 0);
            Assert.Equal(result.CardLinks.Count(), response.CardLinks.Count());
        }

        [Fact]
        public async Task HandlerShouldThrowExceptionWhenInputIsNull()
        {
            var request = new VredCardDetailsServiceQuery("test");
            var response = TestDataProvider.GetTestData<VredCardDetailsResponse>("VRedMS\\VredCardDetailsResponse");

            _mockApiConnector.Setup(m => m.GetAsync<string>(It.IsAny<string>(),
                                                            It.IsAny<Dictionary<string, string>>(),
                                                            It.IsAny<string>())).ReturnsAsync(Json.Serialize(response));

            _mockApiConnector.Setup(m => m.PostAsync(It.IsAny<string>(),
                                                     It.IsAny<HttpContent>(),
                                                     It.IsAny<Dictionary<string, string>>(),
                                                     It.IsAny<string>())).ReturnsAsync(new HttpResponseMessage()
                                                     {
                                                         StatusCode = System.Net.HttpStatusCode.OK,
                                                         Content = new StringContent(Token, System.Text.Encoding.UTF8, "application/json")
                                                     });

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.Empty(result.RedUserId);
        }
    }
}
