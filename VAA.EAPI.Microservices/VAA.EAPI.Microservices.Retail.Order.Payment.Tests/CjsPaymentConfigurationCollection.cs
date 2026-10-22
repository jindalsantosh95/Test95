using Xunit;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Tests
{
    // ConfigReader.Set mutates process environment variables and the cache seeded by TestEnvironment.
    // These tests must not overwrite another test's downstream URL while its request is in flight.
    [CollectionDefinition("CJS payment configuration", DisableParallelization = true)]
    public sealed class CjsPaymentConfigurationCollection
    {
    }
}
