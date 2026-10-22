using Microsoft.Azure.Functions.Worker.Http;
using System.Text;
using System.Web;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Functions;

namespace VAA.EAPI.Infrastructure.Tests
{
    public sealed class ConfigReaderTest
    {
        private static readonly string content = @"{ ""name"": ""test"", ""dimension"": { ""width"": 10, ""length"": 10 }, ""tags"": [ ""test"", ""payload"" ] }";

        [Theory]
        [InlineData("nested/resource.json")]
        [InlineData("nested\\resource.json")]
        public void ReadAllText_LoadsNestedResourceWithEitherSeparator(string relativePath)
        {
            DirectoryInfo temporaryDirectory = Directory.CreateTempSubdirectory("eapi-resource-path-");
            try
            {
                Directory.CreateDirectory(Path.Combine(temporaryDirectory.FullName, "nested"));
                File.WriteAllText(Path.Combine(temporaryDirectory.FullName, "nested", "resource.json"), content);

                Assert.Equal(content, ConfigReader.ReadAllText(temporaryDirectory.FullName, relativePath));
                Assert.Equal(content, ConfigReader.ReadAllText(Path.Combine(temporaryDirectory.FullName, relativePath)));
                Assert.Throws<FileNotFoundException>(() => ConfigReader.ReadAllText(temporaryDirectory.FullName, "nested/missing.json"));
            }
            finally
            {
                temporaryDirectory.Delete(recursive: true);
            }
        }

        public ConfigReaderTest()
        {
            var bindingData = new Dictionary<string, object>
            {
                { "operationName", "testQueryHeader" },
                { "query", "query testQueryHeader(){}" },
                { "variables", @"{ ""selctedTripIndex"": 1 }" },
                { "Headers", Json.Serialize( new Dictionary<string, string>(){{ "x-header", "test" } } ) }
            };

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext(bindingData) };

            var query = bindingData["query"].ToString();
            var variables = bindingData["variables"].ToString();
            var graphQlRequest = $@"{{ query=""{query}"", variables=""{variables}"" }}";
            var stringContent = new StringContent(graphQlRequest, Encoding.UTF8);

            var httpRequestData = new TestHttpRequestData(AsyncContext.Current.Context, new Uri("http://localhost:7044/SubscribeFunc"), stringContent.ReadAsStream());

            httpRequestData.Headers.TryAddWithoutValidation("x-header", "test");

            var httpResponseData = HttpResponseData.CreateResponse(httpRequestData);

            using MemoryStream memoryStream = new();

            memoryStream.Write(Encoding.UTF8.GetBytes(content));

            memoryStream.Position = 0;

            httpResponseData.Body = memoryStream;

            AsyncContext.Current.Context.SetHttpResponseData(httpResponseData);

            AsyncContext.Current.Context.Features.Set<HttpRequestData>(httpRequestData);
        }

        [Fact]
        public void IsCacheEnabledHttpRequestMessageHeaderCheck()
        {
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, "http://headercheck.tempuri.org")
            {
                Content = new StringContent(content, Encoding.UTF8, "applicatgion/json")
            };

            httpRequestMessage.Headers.TryAddWithoutValidation("x-header", "test");

            ConfigReader.Set($"cache:{httpRequestMessage.Method}:{httpRequestMessage.RequestUri!.ToString().Trim('/')}", $"expiry=00:00:00:05&when={HttpUtility.UrlEncode("$[?( @.headers[?( @.key == 'x-header' && @.value[?( @ == 'test' )] )] )]")}");

            bool isCached = ConfigReader.Caching.IsCacheEnabled(httpRequestMessage, out string key);

            Assert.True(isCached, $"Expected caching to be enabled for {httpRequestMessage.Method}:{httpRequestMessage.RequestUri}");
            Assert.False(key.IsNullOrWhiteSpace(), $"Expected caching key to be generated for {httpRequestMessage.Method}:{httpRequestMessage.RequestUri}");
        }

        [Fact]
        public void IsCacheEnabledHttpRequestMessageContentCheck()
        {
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, "http://contentcheck.tempuri.org")
            {
                Content = new StringContent(content, Encoding.UTF8, "applicatgion/json")
            };

            ConfigReader.Set($"cache:{httpRequestMessage.Method}:{httpRequestMessage.RequestUri.ToString().Trim('/')}", $"expiry=00:00:00:05&when={HttpUtility.UrlEncode("$[?( @.content.name == 'test' && @.content.dimension.width == 10 )]")}");

            bool isCached = ConfigReader.Caching.IsCacheEnabled(httpRequestMessage, out string key);

            Assert.True(isCached, $"Expected caching to be enabled for {httpRequestMessage.Method}:{httpRequestMessage.RequestUri}");
            Assert.False(key.IsNullOrWhiteSpace(), $"Expected caching key to be generated for {httpRequestMessage.Method}:{httpRequestMessage.RequestUri}");
        }

        [Fact]
        public void IsCacheEnabledFunctionContextHeaderCheck()
        {
            IHttpRequestDataFeature? httpRequestDataFeature = AsyncContext.Current.Context.Features.Get<IHttpRequestDataFeature>() ?? TestHttpRequestDataFeature.Instance;

            var operationName = AsyncContext.Current.Context.GetOperationName();

            ConfigReader.Set($"cache:{operationName}", $"expiry=00:00:00:05&when={HttpUtility.UrlEncode("$[?( @.headers.x-header[?( @ == 'test' )] )]")}");

            bool isCached = ConfigReader.Caching.IsCacheEnabled(AsyncContext.Current.Context, out string key);

            Assert.True(isCached, $"Expected caching to be enabled for {operationName}");
            Assert.False(key.IsNullOrWhiteSpace(), $"Expected caching key to be generated for {operationName}");
        }

        [Fact]
        public void IsCacheEnabledFunctionContextContentCheck()
        {
            var operationName = AsyncContext.Current.Context.GetOperationName();

            Environment.SetEnvironmentVariable($"cache:{operationName}", $"expiry=00:00:00:05&when={HttpUtility.UrlEncode("$[?( @.variables.selctedTripIndex > 0 )]")}", EnvironmentVariableTarget.Process);

            bool isCached = ConfigReader.Caching.IsCacheEnabled(AsyncContext.Current.Context, out string key);

            Assert.True(isCached, $"Expected caching to be enabled for {operationName}");
            Assert.False(key.IsNullOrWhiteSpace(), $"Expected caching key to be generated for {operationName}");
        }

        [Fact]
        public void ShouldCacheHttpResponse()
        {
            HttpResponseMessage response = new(System.Net.HttpStatusCode.OK)
            {
                RequestMessage = new(HttpMethod.Get, "http://shouldcache.tempuri.org")
                {
                    Content = new StringContent(content, Encoding.UTF8, "applicatgion/json")
                },
                Content = new StringContent(content, Encoding.UTF8, "applicatgion/json")
            };

            ConfigReader.Set($"cache:{response.RequestMessage!.Method}:{response.RequestMessage!.RequestUri!.ToString().Trim('/')}", $"expiry=00:00:00:05&isSliding=true&should={HttpUtility.UrlEncode("$[?( @.response.name == 'test' )]")}");

            bool shouldCache = ConfigReader.Caching.ShouldCache(response, out TimeSpan expiry, out bool isSliding);

            Assert.True(shouldCache, "Expected to cache for response");
            Assert.Equal(TimeSpan.FromSeconds(5), expiry);
            Assert.True(isSliding, "Expected isSliding to be true");
        }

        [Fact(Skip = "Work in progress requiring function context classes")]
        public void ShouldCacheFunctionContext()
        {
            var operationName = AsyncContext.Current.Context.GetOperationName();

            ConfigReader.Set($"cache:{operationName}", $"expiry=00:00:00:05&isSliding=true&should={HttpUtility.UrlEncode("$[?( @.response.name == 'test' )]")}");

            string content = null;

            bool shouldCache = ConfigReader.Caching.ShouldCache(AsyncContext.Current.Context, ref content, out TimeSpan expiry, out bool isSliding);

            Assert.True(shouldCache, "Expected to cache for response");
            Assert.Equal(TimeSpan.FromSeconds(5), expiry);
            Assert.True(isSliding, "Expected isSliding to be true");
        }
    }
}