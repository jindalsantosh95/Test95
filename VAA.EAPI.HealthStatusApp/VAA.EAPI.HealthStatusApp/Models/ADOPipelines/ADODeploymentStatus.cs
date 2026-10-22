namespace VAA.EAPI.HealthStatusApp.Models.ADOPipelines
{
    public class ADODeploymentStatus
    {
        public int PipelineId { get; set; }
        public string PipelineName { get; set; } = string.Empty;
        public string? Folder { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime LastProdBuildDateTime { get; set; }
        public DateTime LastProdDeployedDateTime { get; set; }
        public double BuildToProdLatencyDays { get; set; }
        public int IncompleteDeploys { get; set; }
        public DateTime MostRecentIncompleteBuildDateTime { get; set; }
        public DateTime OldestPendingDeployDateTime { get; set; }
        public double DaysSinceOldestPendingDeploy { get; set; }
        public DateTime LatestPendingDeployDateTime { get; set; }
        public double DaysSinceLatestPendingDeploy { get; set; }
        public DateTime LastUatBuildDateTime { get; set; }
        public DateTime LastUatDeployedDateTime { get; set; }
        public DateTime LastSitBuildDateTime { get; set; }
        public DateTime LastSitDeployedDateTime { get; set; }
        public DateTime LastDevBuildDateTime { get; set; }
        public DateTime LastDevDeployedDateTime { get; set; }
    }
}
