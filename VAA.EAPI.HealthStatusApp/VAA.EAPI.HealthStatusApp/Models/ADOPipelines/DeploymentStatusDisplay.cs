namespace VAA.EAPI.HealthStatusApp.Models.ADOPipelines
{
    public static class DeploymentStatusDisplay
    {
        private const string PipelineBaseUrl = "https://dev.azure.com/virginatlantic/Enterprise%20API%20Management%20Team/_build";
        private const int RepositoryFilterId = 762;
        private const int BranchFilterId = 46661;

        public static string GetPipelineLink(int pipelineId)
        {
            return $"{PipelineBaseUrl}?definitionId={pipelineId}&repositoryFilter={RepositoryFilterId}&branchFilter={BranchFilterId}";
        }

        public static bool IsDigitalPipeline(ADODeploymentStatus status)
        {
            ArgumentNullException.ThrowIfNull(status);

            return status.Folder?.Contains("digital", StringComparison.OrdinalIgnoreCase) == true;
        }
    }
}
