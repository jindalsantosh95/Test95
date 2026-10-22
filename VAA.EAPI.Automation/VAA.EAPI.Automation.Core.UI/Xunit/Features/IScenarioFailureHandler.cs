namespace VAA.EAPI.Automation.Core.UI.Xunit.Features
{
    public interface IScenarioFailureHandler
    {
        void OnFailure(Exception ex);
    }
}
