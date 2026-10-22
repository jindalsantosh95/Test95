using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Insights.Application.Providers
{
    public interface IADOPipelineProcessor
    {
        Task HandleAsync(int pipelineId, string pipelineName);

    }
}