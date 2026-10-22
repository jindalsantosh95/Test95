using Newtonsoft.Json;

namespace VAA.EAPI.Automation.OpenAI.Helpers
{
    public class RequestHelper
    {
        public static string OpenAIRequest(string prompt = "", string systemPrompt = "")
        {
            // Arrange
            var body = new
            {
                model = "gpt-4",  // Use "gpt-4" for GPT-4 models or gpt-3.5-turbo
                messages = new[]
{
                new { role = "system", content = systemPrompt },
                new { role = "user", content = prompt }
            },
                max_tokens = 600
            };

            return JsonConvert.SerializeObject(body);
        }
    }
}
