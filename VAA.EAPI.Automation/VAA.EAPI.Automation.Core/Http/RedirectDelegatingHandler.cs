using System.Diagnostics;
using System.Net;

namespace VAA.EAPI.Automation.Core.Http
{
    internal sealed class RedirectDelegatingHandler : DelegatingHandler
    {
        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            System.Net.Http.HttpResponseMessage response = base.Send(request, cancellationToken);

            if ((int)response.StatusCode >= 300 && (int)response.StatusCode <= 399)
            {
                Uri location = response.Headers.Location;

                if (location == null)
                    return response;

                System.Net.Http.HttpRequestMessage newRequest = Clone(request, location);

                switch (response.StatusCode)
                {
                    case HttpStatusCode.MovedPermanently:
                    case HttpStatusCode.Redirect:
                    case HttpStatusCode.RedirectMethod:

                        if (request.Method == HttpMethod.Post || request.Method == HttpMethod.Put)
                        {
                            newRequest.Method = HttpMethod.Get;
                            newRequest.Content = null;
                        }

                        response = base.Send(newRequest, cancellationToken);

                        break;
                    case HttpStatusCode.RedirectKeepVerb:
                    case HttpStatusCode.PermanentRedirect:

                        response = base.Send(newRequest, cancellationToken);

                        break;
                }
            }

            string content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            stopwatch.Stop();

            Debug.WriteLine($"{response.RequestMessage.Method}:{response.RequestMessage.RequestUri}, {response.StatusCode}, Elapsed Time: {stopwatch.ElapsedMilliseconds} ms, Content Length: {content.Length} bytes");

            response.Headers.TryAddWithoutValidation("http-execution-time", stopwatch.ElapsedMilliseconds.ToString());

            return response;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            System.Net.Http.HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            if ((int)response.StatusCode >= 300 && (int)response.StatusCode <= 399)
            {
                Uri location = response.Headers.Location;

                if (location == null)
                    return response;

                System.Net.Http.HttpRequestMessage newRequest = Clone(request, location);

                switch (response.StatusCode)
                {
                    case HttpStatusCode.MovedPermanently:
                    case HttpStatusCode.Redirect:
                    case HttpStatusCode.RedirectMethod:

                        if (request.Method == HttpMethod.Post)
                            newRequest.Method = HttpMethod.Get;

                        response = await base.SendAsync(newRequest, cancellationToken);

                        break;
                    case HttpStatusCode.RedirectKeepVerb:
                    case HttpStatusCode.PermanentRedirect:

                        response = await base.SendAsync(newRequest, cancellationToken);

                        break;
                }
            }

            string content = await response.Content.ReadAsStringAsync();

            stopwatch.Stop();

            Debug.WriteLine($"{response.RequestMessage.Method}:{response.RequestMessage.RequestUri}, {response.StatusCode}, {stopwatch.ElapsedMilliseconds}, {content.Length}");

            response.Headers.TryAddWithoutValidation("http-execution-time", stopwatch.ElapsedMilliseconds.ToString());

            return response;
        }

        private static HttpRequestMessage Clone(HttpRequestMessage request, Uri location)
        {
            System.Net.Http.HttpRequestMessage httpRequestMessage = new HttpRequestMessage(request.Method, location);

            if (request.Content != null)
            {
                MemoryStream memoryStream = new MemoryStream();

                request.Content.ReadAsStream().CopyTo(memoryStream);

                memoryStream.Position = 0;

                httpRequestMessage.Content = new StreamContent(memoryStream);

                if (request.Content.Headers != null)
                    foreach (KeyValuePair<string, IEnumerable<string>> header in request.Content.Headers)
                        httpRequestMessage.Content.Headers.Add(header.Key, header.Value);
            }

            httpRequestMessage.Version = request.Version;

            foreach (KeyValuePair<string, object?> option in request.Options)
                httpRequestMessage.Options.Set(new HttpRequestOptionsKey<object>(option.Key), option.Value);

            foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
                httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);

            return httpRequestMessage;
        }
    }
}
