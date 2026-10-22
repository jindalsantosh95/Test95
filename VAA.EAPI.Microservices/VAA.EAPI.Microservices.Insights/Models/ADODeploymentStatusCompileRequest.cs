namespace VAA.EAPI.Microservices.Insights.Models
{
    public sealed class ADODeploymentStatusCompileRequest
    {
        public int PipelineId { get; set; }
        public string PipelineName { get; set; } = string.Empty;
    }
}
