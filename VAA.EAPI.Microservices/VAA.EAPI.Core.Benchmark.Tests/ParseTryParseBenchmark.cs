using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Core.Benchmark.Tests
{
    public sealed class ParseTryParseBenchmark
    {
        [Fact]
        public void BenchmarkParse()
        {
            Assert.Equal(ParseBenchmark.Inputs[0], ParseBenchmark.Inputs[0].Parse<int>().ToString());
            Assert.Equal(ParseBenchmark.Inputs[1], ParseBenchmark.Inputs[1].Parse<long>().ToString());
            Assert.Equal(ParseBenchmark.Inputs[2], ParseBenchmark.Inputs[2].Parse<float>().ToString());
            Assert.Equal(ParseBenchmark.Inputs[3], ParseBenchmark.Inputs[3].Parse<double>().ToString());
            Assert.Equal(ParseBenchmark.Inputs[4], ParseBenchmark.Inputs[4].Parse<decimal>().ToString());
            Assert.Equal(ParseBenchmark.Inputs[5], ParseBenchmark.Inputs[5].Parse<bool>().ToString());
            Assert.Equal(ParseBenchmark.Inputs[6], ParseBenchmark.Inputs[6].Parse<char>().ToString());
            Assert.Equal(ParseBenchmark.Inputs[7], ParseBenchmark.Inputs[7].Parse<DateTime>().ToString("yyyy-MM-dd"));
            Assert.Equal(ParseBenchmark.Inputs[8], ParseBenchmark.Inputs[8].Parse<DateOnly>().ToString("yyyy-MM-dd"));
            Assert.Equal(ParseBenchmark.Inputs[9], ParseBenchmark.Inputs[9].Parse<TimeOnly>().ToString("HH:mm:ss"));
            Assert.Equal(ParseBenchmark.Inputs[10], ParseBenchmark.Inputs[10].Parse<Guid>().ToString());
            Assert.Equal(ParseBenchmark.Inputs[11], ParseBenchmark.Inputs[11].Parse<TimeSpan>().ToString());
            Assert.Equal(ParseBenchmark.Inputs[12], ParseBenchmark.Inputs[12].Parse<ParseBenchmark.SampleEnum>().ToString());
            Assert.Equal(ParseBenchmark.Inputs[13], ParseBenchmark.Inputs[13].Parse<Uri>().ToString());
            Assert.Equal(ParseBenchmark.Inputs[14].Replace(Environment.NewLine, string.Empty).Replace(" ", string.Empty), ParseBenchmark.Inputs[14].Parse<JObject>().ToString().Replace(Environment.NewLine, string.Empty).Replace(" ", string.Empty));

            var config = new ManualConfig()
               .AddLogger(ConsoleLogger.Default)
               .AddDiagnoser(MemoryDiagnoser.Default)
               .AddColumnProvider(DefaultColumnProviders.Instance)
               .WithOptions(ConfigOptions.DisableOptimizationsValidator)
               .AddValidator(JitOptimizationsValidator.DontFailOnError);

            var summary = BenchmarkRunner.Run<ParseBenchmark>(config);
        }
    }
}