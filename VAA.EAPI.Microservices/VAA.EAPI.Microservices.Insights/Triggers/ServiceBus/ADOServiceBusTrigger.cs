using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Insights.Application.Providers;

namespace VAA.EAPI.Microservices.Insights.Triggers.ServiceBus
{
    public class ADOServiceBusTrigger
    {
        private static readonly string[] folderList = [
                    "Enterprise-Microservices-AFN-CICD-Pipelines (YAML)",
                    "Enterprise-Microservices-AFN-CICD-Pipelines (YAML) (SlotDeployment)",
                    "Digital Services",
                    "Digital Services - Slot",
                    "GitHub-Pipelines"];

        private readonly IADOPipelineProcessor _adoPipelineProcessor;
        public ADOServiceBusTrigger(IADOPipelineProcessor adoPipelineProcessor)
        {
            _adoPipelineProcessor = adoPipelineProcessor ?? throw new System.ArgumentNullException(nameof(adoPipelineProcessor));
        }

        [OpenApiOperation(operationId: "ADOServiceBusTrigger", tags: new[] { "ServiceBus" }, Visibility = OpenApiVisibilityType.Important, Summary = "Processes Service Bus messages for ADO pipelines", Description = "Triggered by Service Bus messages for ADO pipeline events.")]
        [Function("ADOServiceBusTrigger")]
        public async Task Run([ServiceBusTrigger(Constants.Application.ADOPipelineTopic, Constants.Application.ADOPipelineSubscription, Connection = Constants.Environment.ADOPipelineServiceBus)] string myQueueItem)
        {
            // If we are debugging, we don't want to process the message
#if DEBUG
            return;
#endif 

            int pipelineId = 0;
            string pipelineName = string.Empty;
            bool isMainBranch = false;

            try
            {
                var eventPayload = JObject.Parse(myQueueItem);
                var cloudEvent = Newtonsoft.Json.JsonConvert.DeserializeObject<CloudEvent>(myQueueItem);

                // Did the message come from ADO or from internal reprocessing?
                if (cloudEvent?.Source?.Is(Constants.Application.ADOPipelineSource) == true)
                {
                    VAA.EAPI.Common.AsyncContext.Current?.MapFromCloudEvent(cloudEvent);
                    var eventData = JObject.Parse(cloudEvent.Data.ToString());
                    if (int.TryParse(eventData["pipelineId"]?.ToString(), out pipelineId))
                    {
                        pipelineName = eventData["pipelineName"]?.ToString();
                        isMainBranch = true;
                    }
                }
                else
                {
                    if (int.TryParse(eventPayload["resource"]?["pipeline"]?["id"]?.ToString(), out pipelineId))
                    {
                        pipelineName = eventPayload["resource"]?["pipeline"]?["name"]?.ToString();
                        var refName = eventPayload["resource"]?["run"]?["resources"]?["repositories"]?["self"]?["refName"]?.ToString();
                        var pipelineFolder = eventPayload["resource"]?["pipeline"]?["folder"]?.ToString() ?? string.Empty;

                        isMainBranch = refName != null && refName.Is("refs/heads/main") && folderList.Any(fli => pipelineFolder.Contains(fli, System.StringComparison.OrdinalIgnoreCase));
                    }
                }

                if ((pipelineId > 0) && (!pipelineName.IsNullOrWhiteSpace()) && (isMainBranch))
                {
                    // We are not awaiting this call because we want to process messages in parallel and not have them wait on each other. The processing will be idempotent so it doesn't matter if they run at the same time.
                    _adoPipelineProcessor.HandleAsync(pipelineId, pipelineName);
                }
            }
            catch
            {
                // If the message can't be parsed, log and skip processing
            }
        }
    }
}
