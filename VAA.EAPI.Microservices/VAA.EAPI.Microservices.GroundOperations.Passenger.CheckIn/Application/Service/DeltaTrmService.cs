using System.Text;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Application.Interface;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.DeltaTrm;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Infrastructure.Helper.Interface;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Application.Service
{
    public class DeltaTrmService(IHttpConnector httpConnector, IAir4TokenGenerator air4TokenGenerator) : IDeltaTrmService
    {
        private readonly IHttpConnector _httpConnector = httpConnector;
        private readonly string _serviceEndpoint = ConfigReader.Get(Constants.Environment.DeltaTrmEndpoint);
        private readonly IAir4TokenGenerator _air4TokenGenerator = air4TokenGenerator;

        private readonly static Dictionary<string, string> _headers = new()
        {
            {Constants.Application.DeltaChannelName, ConfigReader.Get(Constants.Application.DeltaChannelValue)},
            {Constants.Application.DeltaApiId, ConfigReader.Get(Constants.Application.DeltaApiIdValue)},
            {Constants.Application.DeltaClientIdName, ConfigReader.Get(Constants.Application.DeltaClientIdValue)},
            {Constants.Application.DeltaTestTrmName, ConfigReader.Get(Constants.Application.DeltaTestTrmValue)},
            {Constants.Application.TrmConfigOptions, ConfigReader.Get(Constants.Application.TrmConfigOptionsValue)}
        };

        private async Task<string> GetToken() => await _air4TokenGenerator.GetToken(ConfigReader.Get(Constants.Application.DeltaClientIdValue), ConfigReader.Get(Constants.Application.DeltaTrmClientSecret));

        public async Task<RequirementsResponse> PostRequirementsAsync(RequirementsRequest request)
        {
            var content = new StringContent(Json.Serialize(request, false), Encoding.UTF8, "application/json");

            var token = await GetToken();

            return await _httpConnector.PostAsync<RequirementsResponse>($"{_serviceEndpoint}/requirements", content, _headers, token);
        }

        public async Task<RequirementsResponse> GetRequirementsAsync(string requirementsResourceId)
        {
            var token = await GetToken();

            return await _httpConnector.GetAsync<RequirementsResponse>($"{_serviceEndpoint}/requirements/{requirementsResourceId}", _headers, token);
        }

        public async Task<RequirementFormResponse> PatchSingleRequirementsAsync(string requirementsResourceId, RequirementFormRequest request)
        {
            // We wrap this in a requests array here, because if we do it using the RequirementsFormRequest c# object instead,
            // then when it is serialised it includes default values for lots of properties that we don't want to use.
            var contentString = "{\"requests\": [" + Json.Serialize(request, false) + "]}";
            var content = new StringContent(contentString, Encoding.UTF8, "application/json");

            var token = await GetToken();

            return await _httpConnector.PatchAsync<RequirementFormResponse>($"{_serviceEndpoint}/requirements/{requirementsResourceId}", content, _headers, token);
        }

        public async Task<RequirementFormResponse> PatchBatchRequirementsAsync(string requirementsResourceId, RequirementFormRequest request)
        {
            var content = new StringContent(Json.Serialize(request, false), Encoding.UTF8, "application/json");

            var token = await GetToken();

            return await _httpConnector.PatchAsync<RequirementFormResponse>($"{_serviceEndpoint}/requirements/{requirementsResourceId}", content, _headers, token);
        }
    }
}