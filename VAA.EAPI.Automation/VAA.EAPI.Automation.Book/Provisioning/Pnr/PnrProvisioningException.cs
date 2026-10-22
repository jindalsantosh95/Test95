namespace VAA.EAPI.Automation.Book.Provisioning.Pnr
{
    public sealed class PnrProvisioningException : Exception
    {
        public PnrStage FailedStage { get; }

        public PnrScenario Scenario { get; }

        public CreatedPnr? CreatedPnr { get; }

        public PnrProvisioningException(
            PnrStage failedStage,
            PnrScenario scenario,
            Exception inner,
            CreatedPnr? createdPnr = null)
            : base($"PNR provisioning failed at stage {failedStage} for scenario '{scenario.Name}'", inner)
        {
            FailedStage = failedStage;
            Scenario = scenario;
            CreatedPnr = createdPnr;
        }
    }
}
