using Microsoft.Azure.Functions.Worker.Http;
using VAA.EAPI.Common.Serialisation;

namespace VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Tests
{
    public static class EligibilityTestHelpers
    {
        public static async Task<T> ReadResponseData<T>(HttpResponseData responseData)
        {
            responseData.Body.Seek(0, SeekOrigin.Begin);

            var reader = new StreamReader(responseData.Body);
            var responseBody = await reader.ReadToEndAsync();

            return Json.Deserialize<T>(responseBody);
        }
    }
}
