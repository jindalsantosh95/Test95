using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Insights.Domain.ADO;
using VAA.EAPI.Microservices.Insights.Models;

namespace VAA.EAPI.Microservices.Insights.Application.Providers
{
    public class ADOPipelineProcessor : IADOPipelineProcessor
    {
        private readonly IHttpConnector _httpConnector;
        private readonly IDistributedCacheService _distributedCacheService;
        public ADOPipelineProcessor(IHttpConnector httpConnector, IDistributedCacheService distributedCacheService, ILogger<ADOPipelineProcessor> logger)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _distributedCacheService = distributedCacheService ?? throw new ArgumentNullException(nameof(distributedCacheService));
            _ = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleAsync(int pipelineId, string pipelineName)
        {
            var pipelineDetails = await GetPipelineDetails(pipelineId);
            var buildStagesStatus = await BuildStatusAsync(pipelineDetails);

            await ADODeploymentStatusCache.UpdateAsync(
                _distributedCacheService,
                deploymentCollection =>
                {
                    // Need the collection to be mutable to update the status for the pipeline
                    var statusCollection = deploymentCollection.DeploymentStatuses.ToList();
                    statusCollection.RemoveAll(s => s.PipelineId == pipelineId);
                    statusCollection.Add(buildStagesStatus);

                    deploymentCollection.DeploymentStatuses = statusCollection;

                    return true;
                });
        }

        private async Task<ADODeploymentStatus> BuildStatusAsync(ADOPipeline pipelineDetails)
        {
            //Models.ADODeploymentStatus deploymentStatusesForPipeline = new Models.ADODeploymentStatus();

            // Get Pipeline Runs for Pipeline
            var pipelineRuns = await GetPipelineRuns(pipelineDetails.id);

            // Get Build Status for Each Pipeline Run - Need to do sequentially to stop at the last time MAIN was deployed to PROD
            var incompleteDeploys = 0;
            var deployedToProdDateTime = DateTime.MinValue;
            var deployedToProdBuildDateTime = DateTime.MinValue;
            var deployedToUatDateTime = DateTime.MinValue;
            var deployedToUatBuildDateTime = DateTime.MinValue;
            var deployedToSitDateTime = DateTime.MinValue;
            var deployedToSitBuildDateTime = DateTime.MinValue;
            var deployedToDevDateTime = DateTime.MinValue;
            var deployedToDevBuildDateTime = DateTime.MinValue;

            var latestPendingDeployDateTime = DateTime.MinValue;
            var oldestPendingDeployDateTime = DateTime.MinValue;
            var mostRecentIncompleteBuildDateTime = DateTime.MinValue;

            foreach (var run in pipelineRuns)
            {
                // Get Build Details
                var buildDetails = await GetBuildDetails(run.id);
                if (buildDetails.sourceBranch == "refs/heads/main")
                {
                    var buildTimeLine = (await GetBuildTimeLine(buildDetails.id)).Where(t => string.IsNullOrEmpty(t.parentId)).OrderBy(t => t.order);

                    // Has this build deployed to PROD?
                    var prodRecord = buildTimeLine.FirstOrDefault(t => t.name.Is("AFN - PROD") || (t.name.Is("Deployment - PROD")));
                    if (prodRecord != null && prodRecord.state.Is("completed") && prodRecord.result.Is("succeeded"))
                    {
                        deployedToProdBuildDateTime = run.createdDate;
                        deployedToProdDateTime = prodRecord.finishTime.HasValue ? prodRecord.finishTime.Value : DateTime.MinValue;
                        break;
                    }
                    latestPendingDeployDateTime = latestPendingDeployDateTime == DateTime.MinValue ? run.createdDate : latestPendingDeployDateTime;
                    oldestPendingDeployDateTime = run.createdDate;

                    // Has this build deployed to UAT? - UAT is below PROD because if there are no other UATs since last PROD, we dont want to know
                    if (deployedToUatBuildDateTime == DateTime.MinValue)
                    {
                        var uatRecord = buildTimeLine.FirstOrDefault(t => t.name.Is("AFN - UAT") || (t.name.Is("Deployment - UAT")));
                        if (uatRecord != null && uatRecord.state.Is("completed") && uatRecord.result.Is("succeeded"))
                        {
                            deployedToUatBuildDateTime = run.createdDate;
                            deployedToUatDateTime = uatRecord.finishTime.HasValue ? uatRecord.finishTime.Value : DateTime.MinValue;
                        }
                    }

                    // Has this build deployed to SIT? - SIT is below UAT because if there are no other SITs since last UAT, we dont want to know
                    if (deployedToSitBuildDateTime == DateTime.MinValue)
                    {
                        var sitRecord = buildTimeLine.FirstOrDefault(t => t.name.Is("AFN - SIT") || (t.name.Is("Deployment - SIT")));
                        if (sitRecord != null && sitRecord.state.Is("completed") && sitRecord.result.Is("succeeded"))
                        {
                            deployedToSitBuildDateTime = run.createdDate;
                            deployedToSitDateTime = sitRecord.finishTime.HasValue ? sitRecord.finishTime.Value : DateTime.MinValue;
                        }
                    }

                    // Has this build deployed to DEV? - DEV is below SIT because if there are no other DEVs since last SIT, we dont want to know
                    if (deployedToDevBuildDateTime == DateTime.MinValue)
                    {
                        var devRecord = buildTimeLine.FirstOrDefault(t => t.name.Is("AFN - DEV") || (t.name.Is("Deployment - DEV")));
                        if (devRecord != null && devRecord.state.Is("completed") && devRecord.result.Is("succeeded"))
                        {
                            deployedToDevBuildDateTime = run.createdDate;
                            deployedToDevDateTime = devRecord.finishTime.HasValue ? devRecord.finishTime.Value : DateTime.MinValue;
                        }
                    }

                    incompleteDeploys++;
                    if (run.createdDate > mostRecentIncompleteBuildDateTime)
                    {
                        mostRecentIncompleteBuildDateTime = run.createdDate;
                    }
                }
            }

            return new()
            {
                PipelineId = pipelineDetails.id,
                PipelineName = pipelineDetails.name,
                Folder = pipelineDetails.folder,
                LastUpdated = DateTime.UtcNow,
                LastProdBuildDateTime = deployedToProdBuildDateTime,
                LastProdDeployedDateTime = deployedToProdDateTime,
                LastUatBuildDateTime = deployedToUatBuildDateTime,
                LastUatDeployedDateTime = deployedToUatDateTime,
                BuildToProdLatencyDays = (deployedToProdDateTime - deployedToProdBuildDateTime).TotalDays,
                IncompleteDeploys = incompleteDeploys,
                MostRecentIncompleteBuildDateTime = mostRecentIncompleteBuildDateTime,
                OldestPendingDeployDateTime = oldestPendingDeployDateTime,
                DaysSinceOldestPendingDeploy = (DateTime.Now - oldestPendingDeployDateTime).TotalDays,
                LatestPendingDeployDateTime = latestPendingDeployDateTime,
                DaysSinceLatestPendingDeploy = (DateTime.Now - latestPendingDeployDateTime).TotalDays,
                LastSitBuildDateTime = deployedToSitBuildDateTime,
                LastSitDeployedDateTime = deployedToSitDateTime,
                LastDevBuildDateTime = deployedToDevBuildDateTime,
                LastDevDeployedDateTime = deployedToDevDateTime
            };
        }

        private async Task<ADOPipeline> GetPipelineDetails(int id)
        {
            var azureDevOpsOrgUrl = ConfigReader.Get(Constants.Environment.DevOpsBaseUrl);
            var projectName = ConfigReader.Get(Constants.Environment.DevOpsProject);
            var personalAccessToken = ConfigReader.Get(Constants.Environment.DevOpsPAT);

            var pipelineResponse = await _httpConnector.SendAsync<ADOPipeline>(new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{azureDevOpsOrgUrl}/{projectName}/_apis/pipelines/{id}?api-version=7.1"),
                Headers =
                {
                    { "Accept", "application/json" },
                    { "Authorization", $"Basic {Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{personalAccessToken}"))}" }
                }
            }, System.Threading.CancellationToken.None);

            return pipelineResponse!;
        }

        private async Task<ADOBuildDetailsResponse> GetBuildDetails(int id)
        {
            var azureDevOpsOrgUrl = ConfigReader.Get(Constants.Environment.DevOpsBaseUrl);
            var projectName = ConfigReader.Get(Constants.Environment.DevOpsProject);
            var personalAccessToken = ConfigReader.Get(Constants.Environment.DevOpsPAT);

            var buildDetailResponse = await _httpConnector.SendAsync<ADOBuildDetailsResponse>(new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{azureDevOpsOrgUrl}/{projectName}/_apis/build/builds/{id}?api-version=7.1"),
                Headers =
                {
                    { "Accept", "application/json" },
                    { "Authorization", $"Basic {Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{personalAccessToken}"))}" }
                }
            }, System.Threading.CancellationToken.None);

            return buildDetailResponse!;
        }

        private async Task<IEnumerable<ADOBuildTimeline>> GetBuildTimeLine(int id)
        {
            var azureDevOpsOrgUrl = ConfigReader.Get(Constants.Environment.DevOpsBaseUrl);
            var projectName = ConfigReader.Get(Constants.Environment.DevOpsProject);
            var personalAccessToken = ConfigReader.Get(Constants.Environment.DevOpsPAT);

            var buildStatusResponse = await _httpConnector.SendAsync<ADOBuildTimelineResponse>(new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{azureDevOpsOrgUrl}/{projectName}/_apis/build/builds/{id}/timeline?api-version=7.1"),
                Headers =
                {
                    { "Accept", "application/json" },
                    { "Authorization", $"Basic {Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{personalAccessToken}"))}" }
                }
            }, System.Threading.CancellationToken.None);

            return buildStatusResponse!.BuildTimeLines.Where(b => string.IsNullOrWhiteSpace(b.parentId));
        }

        private async Task<IEnumerable<ADOPipelineRun>> GetPipelineRuns(int id)
        {
            var azureDevOpsOrgUrl = ConfigReader.Get(Constants.Environment.DevOpsBaseUrl);
            var projectName = ConfigReader.Get(Constants.Environment.DevOpsProject);
            var personalAccessToken = ConfigReader.Get(Constants.Environment.DevOpsPAT);

            var pipelineRunsResponse = await _httpConnector.SendAsync<ADOPipelineRunsResponse>(new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{azureDevOpsOrgUrl}/{projectName}/_apis/pipelines/{id}/runs?api-version=7.1"),
                Headers =
                {
                    { "Accept", "application/json" },
                    { "Authorization", $"Basic {Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{personalAccessToken}"))}" }
                }
            }, System.Threading.CancellationToken.None);

            return pipelineRunsResponse!.PipelineRuns;
        }
    }
}
