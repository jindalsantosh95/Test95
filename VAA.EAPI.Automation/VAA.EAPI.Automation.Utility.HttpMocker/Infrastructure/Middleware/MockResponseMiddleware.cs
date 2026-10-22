using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Http.Mocking;
using VAA.EAPI.Automation.Utility.HttpMocker.Application;

namespace VAA.EAPI.Automation.Utility.HttpMocker.Infrastructure.Middleware
{
    internal class MockResponseMiddleware : IFunctionsWorkerMiddleware
    {
        private const string HttpRequestContextKey = "HttpRequestContext";
        private const string ResponseDelayHeader = "Response-Delay";

        private readonly IMockStorageService _mockStorageService;

        public MockResponseMiddleware(IMockStorageService mockStorageService)
            => _mockStorageService = mockStorageService;

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var request = (await context.GetHttpRequestDataAsync())!;
            var defaultHttpContext = (DefaultHttpContext)request.FunctionContext.Items.First(x => x.Key.ToString().Is(HttpRequestContextKey)).Value;

            var httpMock = await _mockStorageService.FindMockAsync(context);

            if (httpMock is not null)
            {
                using var requestMessage = httpMock.Request.Clone();
                using var responseMessage = httpMock.CreateResponse(requestMessage);

                if (responseMessage.Headers.TryGetValues(ResponseDelayHeader, out var values) && string.Join(string.Empty, values).TryParse(out TimeSpan value) && value.TotalMilliseconds > 0)
                    await Task.Delay((int)value.TotalMilliseconds);

                defaultHttpContext.Response.Headers.Clear();

                defaultHttpContext.Response.Headers.TryAdd("x-mock-raw-request", Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(httpMock.RawRequest)));

                foreach (var header in responseMessage.Headers.Where(h => !h.Key.Is(ResponseDelayHeader)))
                    defaultHttpContext.Response.Headers.Append(header.Key, string.Join(' ', header.Value));

                defaultHttpContext.Response.StatusCode = (int)responseMessage.StatusCode;
                defaultHttpContext.Response.ContentType = responseMessage.Content?.Headers.ContentType?.ToString();

                if (responseMessage.Content is not null)
                {
                    var content = await responseMessage.Content.ReadAsStringAsync();

                    using var dynamicDataGenerator = (CommandParser)content;

                    content = dynamicDataGenerator.Parse();

                    defaultHttpContext.Response.ContentLength = Math.Max(content.Length, System.Text.Encoding.UTF8.GetByteCount(content));

                    await defaultHttpContext.Response.WriteAsync(content);
                }
            }
            else
                await next(context);
        }
    }
}
