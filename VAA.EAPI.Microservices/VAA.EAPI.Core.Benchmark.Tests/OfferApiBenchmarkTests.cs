using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;

namespace VAA.EAPI.Core.Benchmark.Tests
{
    public class OfferApiBenchmarkTests
    {
        [Fact]
        public void OfferApiBenchmarkTest()
        {
            var config = new ManualConfig()
               .AddLogger(ConsoleLogger.Default)
               .AddDiagnoser(MemoryDiagnoser.Default)
               .AddColumnProvider(DefaultColumnProviders.Instance)
               .WithOptions(ConfigOptions.DisableOptimizationsValidator)
               .AddValidator(JitOptimizationsValidator.DontFailOnError);

            var summary = BenchmarkRunner.Run<OfferApiBenchmark>(config);
        }
    }
}
