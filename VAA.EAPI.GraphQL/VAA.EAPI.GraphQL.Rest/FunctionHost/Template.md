# Agent guidelines for endpoint consumption

## Scope
These instructions apply to the specified endpoint and method.

## HTTP rest operation
- Endpoint: `{endpoint}`
- Method: `{method}`

Access the endpoint at `{endpoint}` with the following HTTP method: `{method}`.
To generate this document call the endpoint with the OPTIONS method.

### Payload structure for POST if applicable
`{request}`

### Payload Schema for POST if applicable
`{requestSchema}`

### Response Schema
`{responseSchema}`

## Examples
A number of examples are provided below to illustrate how to call the endpoint using different tools and programming languages.

Curl command:
```bash
curl -X {method} {endpoint} -H "Content-Type: application/json" -d '{request}'
```

Python code using requests library:
```python
import requests
import json

url = "{endpoint}"
payload = """{request}"""
headers = { "Content-Type": "application/json" }

response = requests.request("{method}", url, json=json.loads(payload), headers=headers)

print(response.text)
```

C# code using HttpClient: 
```csharp
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace EndpointConsumer
{
    class Program
    {
        static async Task Main()
        {
            var url = "{endpoint}";
            var payload = """{request}""";
            
            using var client = new HttpClient();

            var request = new HttpRequestMessage
            {
                Method = new HttpMethod("{method}"),
                RequestUri = new Uri(url),
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            Console.WriteLine(responseString);
        }
    }
}
```