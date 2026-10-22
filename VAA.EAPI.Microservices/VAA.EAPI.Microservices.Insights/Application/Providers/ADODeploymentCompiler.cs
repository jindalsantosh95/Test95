using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Microservices.Insights.Domain.ADO;
using VAA.EAPI.Microservices.Insights.Interfaces;
using VAA.EAPI.Microservices.Insights.Models;

namespace VAA.EAPI.Microservices.Insights.Application.Providers
{


    public class ADODeploymentCompiler : IADODeploymentCompiler
    {
        private readonly IHttpConnector _httpConnector;
        private readonly IDistributedCacheService _distributedCacheService;
        private readonly IMessageBusFactory _messageBusFactory;

        public ADODeploymentCompiler(IDistributedCacheService distributedCacheService, IHttpConnector httpConnector, IMessageBusFactory messageBusFactory)
        {
            _distributedCacheService = distributedCacheService ?? throw new ArgumentNullException(nameof(distributedCacheService));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _messageBusFactory = messageBusFactory ?? throw new ArgumentNullException(nameof(messageBusFactory));
        }

        public async Task CompileDeploymentDataAsync()
        {
            // Get List of Azure Build Pipelines
            var pipelines = (await GetPipelines()).ToList();

            // Get the current compiled data so we can skip recently refreshed pipelines and prune statuses for deleted pipelines.
            var compiledData = await ADODeploymentStatusCache.UpdateAsync(
                _distributedCacheService,
                deploymentCollection => RemoveDeletedPipelineStatuses(deploymentCollection, pipelines));

            var messageBusClient = _messageBusFactory.GetClient(ConfigReader.Get(Constants.Environment.ADOPipelineServiceBus), Constants.Application.ADOPipelineTopic);

            // Loop through Pipelines
            foreach (var pipeline in pipelines)
            {
                // Skip Disabled or DO NOT USE Pipelines
                if (pipeline.name.Contains("DISABLED", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }
                if (pipeline.name.Contains("DO NOT USE", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                // Check to see if its stale, if last update was within 24hours, then we'll skip it for now and let the existing data be used.
                var existingPipeline = compiledData.DeploymentStatuses.FirstOrDefault(p => p.PipelineId == pipeline.id);
                if (existingPipeline != null && existingPipeline.LastUpdated > DateTime.UtcNow.AddHours(-24))
                {
                    continue;
                }


                await SendPipelineCompileMessageAsync(messageBusClient, pipeline.id, pipeline.name);
            }
        }

        private static bool RemoveDeletedPipelineStatuses(ADODeploymentCollection compiledData, IEnumerable<ADOPipeline> pipelines)
        {
            var currentPipelineIds = pipelines.Select(pipeline => pipeline.id).ToHashSet();
            var storedDeploymentStatuses = compiledData.DeploymentStatuses.ToList();
            var currentDeploymentStatuses = storedDeploymentStatuses
                .Where(status => currentPipelineIds.Contains(status.PipelineId))
                .ToList();

            if (currentDeploymentStatuses.Count == storedDeploymentStatuses.Count)
            {
                return false;
            }

            compiledData.DeploymentStatuses = currentDeploymentStatuses;

            return true;
        }

        private static async Task SendPipelineCompileMessageAsync(IMessageBus messageBusClient, int pipelineId, string pipelineName)
        {
            JObject itemsData = new();
            itemsData.Add("pipelineId", pipelineId);
            itemsData.Add("pipelineName", pipelineName);

            var cloudEvent = new CloudEvent()
            {
                Id = Guid.NewGuid().ToString(),
                Subject = Constants.Application.ADOPipelineTopic,
                Source = Constants.Application.ADOPipelineSource,
                Time = DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss tt"),
                Data = Json.Serialize(itemsData),
                Type = Constants.Application.ADOPipelineTopic
            };

            AsyncContext.Current.MapToCloudEvent(cloudEvent);

            await messageBusClient.PublishMessageAsync(cloudEvent);
        }

        private async Task<IEnumerable<ADOPipeline>> GetPipelines()
        {
            var azureDevOpsOrgUrl = ConfigReader.Get(Constants.Environment.DevOpsBaseUrl);
            var projectName = ConfigReader.Get(Constants.Environment.DevOpsProject);
            var personalAccessToken = ConfigReader.Get(Constants.Environment.DevOpsPAT);


            var pipelineResponse = await _httpConnector.SendAsync<ADOPipelineResponse>(new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{azureDevOpsOrgUrl}/{projectName}/_apis/pipelines?api-version=7.1"),
                Headers =
                {
                    { "Accept", "application/json" },
                    { "Authorization", $"Basic {Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{personalAccessToken}"))}" }
                }
            }, System.Threading.CancellationToken.None);

            string[] folderList = [
                "Enterprise-Microservices-AFN-CICD-Pipelines (YAML)",
                    "Enterprise-Microservices-AFN-CICD-Pipelines (YAML) (SlotDeployment)",
                    "Digital Services",
                    "Digital Services - Slot",
                    "GitHub-Pipelines"
            ];


            return pipelineResponse!.Pipelines.Where(p => folderList.Any(folder => p.folder.Contains(folder, StringComparison.InvariantCultureIgnoreCase)));
        }

        public async Task<ADODeploymentCollection> GetCompiledData()
        {
            return await ADODeploymentStatusCache.GetAsync(_distributedCacheService);
        }

        public async Task Reset()
        {
            await ADODeploymentStatusCache.ResetAsync(_distributedCacheService);

        }
    }
}
