using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;

namespace VAA.EAPI.Core.Benchmark.Tests
{
    public class HttpMockCreateResponseMessageBenchmarkTests
    {
        [Fact]
        public void CreateResponseMessageBenchmark()
        {
            var config = ManualConfig.Create(DefaultConfig.Instance)
                .AddDiagnoser(MemoryDiagnoser.Default)
                .WithOptions(ConfigOptions.DisableOptimizationsValidator)
                .WithBuildTimeout(TimeSpan.FromMinutes(10))
                .WithArtifactsPath(Path.Combine(AppContext.BaseDirectory, "BenchmarkDotNet.Artifacts"))
                .AddValidator(JitOptimizationsValidator.DontFailOnError);

            var summary = BenchmarkRunner.Run<HttpMockCreateResponseMessageBenchmark>(config);
        }
    }
}
