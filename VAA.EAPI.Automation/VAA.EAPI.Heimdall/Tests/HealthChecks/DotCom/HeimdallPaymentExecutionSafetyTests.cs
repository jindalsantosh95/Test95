namespace VAA.EAPI.Automation.Heimdall.Tests.HealthChecks.DotCom;

public sealed class HeimdallPaymentExecutionSafetyTests
{
    [Fact]
    public void ProductionRequiresValidationOnly()
    {
        Assert.True(HeimdallPaymentExecutionSafety.RequiresHppValidationOnly("prod"));
    }

    [Theory]
    [InlineData("dev")]
    [InlineData("sit")]
    [InlineData("si")]
    [InlineData("uat")]
    [InlineData("stg")]
    [InlineData("stage")]
    public void RecognisedNonProductionEnvironmentRetainsPaymentExecution(string environment)
    {
        Assert.False(HeimdallPaymentExecutionSafety.RequiresHppValidationOnly(environment));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("production")]
    [InlineData("prdo")]
    public void MissingOrUnsupportedEnvironmentFailsClosed(string? environment)
    {
        Assert.Throws<InvalidOperationException>(
            () => HeimdallPaymentExecutionSafety.RequiresHppValidationOnly(environment));
    }
}
