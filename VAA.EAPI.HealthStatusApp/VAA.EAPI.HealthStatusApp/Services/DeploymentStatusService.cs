using VAA.EAPI.Infrastructure.Interfaces;

namespace VAA.EAPI.HealthStatusApp.Services;


public class DeploymentStatusService : IDeploymentStatusService
{
    private readonly IConfiguration _config;
    private readonly IHttpConnector _httpConnector;

    public DeploymentStatusService(IConfiguration configuration, IHttpConnector httpConnector)
    {
        _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
    }

    public async Task<Models.ADOPipelines.ADODeploymentCollection> GetDeploymentStatusesAsync()
    {
        var deploymentStatusApiUrl = _config["HEALTH_CHECK_URL"];
        var subscriptionKey = _config["HEALTH_CHECK_SUBSCRIPTION_KEY"];

        var response = await _httpConnector.SendAsync<Models.ADOPipelines.ADODeploymentCollection>(new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"{deploymentStatusApiUrl}/deploymentstatus"),
            Headers =
                {
                    { "Accept", "application/json" },
                    { "Ocp-Apim-Subscription-Key", $"{subscriptionKey}" }
                }
        }, CancellationToken.None);

        response.DeploymentStatuses = response.DeploymentStatuses.OrderBy(d => d.PipelineName).ToList();

        return response;
    }
}
