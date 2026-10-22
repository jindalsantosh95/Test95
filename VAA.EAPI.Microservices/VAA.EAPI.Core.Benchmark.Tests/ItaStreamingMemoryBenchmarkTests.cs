using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;

namespace VAA.EAPI.Core.Benchmark.Tests;

public class ItaStreamingMemoryBenchmarkTests
{
    [Fact]
    public void ItaStreamingMemoryBenchmarkTest()
    {
        var config = ManualConfig.Create(DefaultConfig.Instance)
            .AddDiagnoser(MemoryDiagnoser.Default)
            .WithOptions(ConfigOptions.DisableOptimizationsValidator)
            .WithBuildTimeout(TimeSpan.FromMinutes(10))
            .WithArtifactsPath(Path.Combine(AppContext.BaseDirectory, "BenchmarkDotNet.Artifacts"))
            .AddValidator(JitOptimizationsValidator.DontFailOnError);

        _ = BenchmarkRunner.Run<ItaStreamingMemoryBenchmark>(config);
    }
}
