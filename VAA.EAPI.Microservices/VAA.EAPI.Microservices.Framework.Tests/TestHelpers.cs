using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text;
using VAA.EAPI.Common.Serialisation;

namespace VAA.EAPI.Microservices.Framework.Tests
{
    [ExcludeFromCodeCoverage]
    public static class TestHelpers
    {
        public static ClaimsPrincipal TestUser() => TestDataProvider.TestUser();

        public static class TestDataProvider
        {
            public static ClaimsPrincipal TestUser()
            {
                var claims = new List<Claim>()
                {
                    new(ClaimTypes.Name, "john.doe@somewhere.com"),
                    new(ClaimTypes.NameIdentifier, "JDOE0001"),
                    new("name", "John Doe"),
                };

                return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));
            }

            public static string GetTestData(string dataKey) => File.ReadAllText($"TestDataSamples/{dataKey.Replace('\\', '/')}.json");

            public static T GetTestData<T>(string dataKey)
            {
                // Load the test data file 
                string testDataFileContent = GetTestData(dataKey);

                // Convert the test data into the required object
                T response;

                if (typeof(T) == typeof(string))
                    response = (T)Convert.ChangeType(testDataFileContent, typeof(T));
                else
                    response = VAA.EAPI.Common.Serialisation.Json.Deserialize<T>(testDataFileContent);

                return response;
            }

            public static string GetXmlTestData(string dataKey) => File.ReadAllText($"TestDataSamples/{dataKey.Replace('\\', '/')}.xml");

            public static T GetXmlTestData<T>(string dataKey)
            {
                // Load the test data file 
                string testXmlInputData = File.ReadAllText($"TestDataSamples/{dataKey.Replace('\\', '/')}.xml");
                // Convert the test data into the required object
                T response;
                if (typeof(T) == typeof(string))
                {
                    response = (T)Convert.ChangeType(testXmlInputData, typeof(T));
                }
                else
                {
                    response = Xml.Deserialize<T>(testXmlInputData);
                }

                return response;
            }

            public static Mock<HttpRequestData> GetMockHttpRequest(string dataKey, Mock<FunctionContext> functionContext, bool isXml = false)
            {
                var body = isXml ? GetXmlTestData<string>(dataKey) : GetTestData(dataKey);
                var byteArray = Encoding.ASCII.GetBytes(body);
                var bodyStream = new MemoryStream(byteArray);
                var request = new Mock<HttpRequestData>(functionContext.Object);

                request.Setup(r => r.Body).Returns(bodyStream);
                request.Setup(r => r.CreateResponse()).Returns(() =>
                {
                    var response = new Mock<HttpResponseData>(functionContext.Object);

                    response.SetupProperty(r => r.Headers, new HttpHeadersCollection());
                    response.SetupProperty(r => r.StatusCode);
                    response.SetupProperty(r => r.Body, new MemoryStream());

                    return response.Object;
                });

                return request;
            }

            public static Mock<HttpRequestData> GetMockHttpRequestWithMockResponse(string dataKey, Mock<FunctionContext> functionContext, string mockResponse, bool isXml = false)
            {
                var body = isXml ? GetXmlTestData<string>(dataKey) : GetTestData(dataKey);
                var byteArray = Encoding.UTF8.GetBytes(body);
                var bodyStream = new MemoryStream(byteArray);
                var request = new Mock<HttpRequestData>(functionContext.Object);

                request.Setup(r => r.Body).Returns(bodyStream);
                request.Setup(r => r.CreateResponse()).Returns(() =>
                {
                    var response = new Mock<HttpResponseData>(functionContext.Object);

                    response.SetupProperty(r => r.Headers, new HttpHeadersCollection());
                    response.SetupProperty(r => r.StatusCode);
                    response.SetupProperty(r => r.Body, new MemoryStream(Encoding.UTF8.GetBytes(mockResponse)));

                    return response.Object;
                });

                return request;
            }
        }
    }
}
