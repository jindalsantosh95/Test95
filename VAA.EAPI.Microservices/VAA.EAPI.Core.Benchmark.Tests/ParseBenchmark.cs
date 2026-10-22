using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Core.Benchmark.Tests
{
    [SimpleJob(RunStrategy.ColdStart, iterationCount: 100)]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn, IterationsColumn]
    public class ParseBenchmark
    {
        public enum SampleEnum { Alpha, Beta, Gamma }

        public static readonly string[] Inputs = [
            "123",
            "1234567890123",
            "123.45",
            "123.456789",
            "123456.789012",
            "True",
            "A",
            "2023-09-01",
            "2023-09-01",
            "12:34:56",
            "550e8400-e29b-41d4-a716-446655440000",
            "01:02:03",
            "Beta",
            "http://www.google.com/",
            @"{ ""PropertyInt"": 1, ""PropertyBoolean"": true, ""PropertyDateTime"": ""20250101T00:00:00"", ""PropertyString"": ""Hello World!!"", ""PropertyArray"": [1, 2, 3, 4, 5]}"
            ];

        public ParseBenchmark()
        {
            ParseInt();
            ParseLong();
            ParseFloat();
            ParseDouble();
            ParseDecimal();
            ParseBool();
            ParseChar();
            ParseDateTime();
            ParseDateOnly();
            ParseTimeOnly();
            ParseGuid();
            ParseTimeSpan();
            ParseSampleEnum();
            ParseUri();
            ParseJObject();
        }

        [Benchmark]
        public void ParseInt() => Inputs[0].Parse<int>();
        [Benchmark]
        public void ParseLong() => Inputs[1].Parse<long>();
        [Benchmark]
        public void ParseFloat() => Inputs[2].Parse<float>();
        [Benchmark]
        public void ParseDouble() => Inputs[3].Parse<double>();
        [Benchmark]
        public void ParseDecimal() => Inputs[4].Parse<decimal>();
        [Benchmark]
        public void ParseBool() => Inputs[5].Parse<bool>();
        [Benchmark]
        public void ParseChar() => Inputs[6].Parse<char>();
        [Benchmark]
        public void ParseDateTime() => Inputs[7].Parse<DateTime>();
        [Benchmark]
        public void ParseDateOnly() => Inputs[8].Parse<DateOnly>();
        [Benchmark]
        public void ParseTimeOnly() => Inputs[9].Parse<TimeOnly>();
        [Benchmark]
        public void ParseGuid() => Inputs[10].Parse<Guid>();
        [Benchmark]
        public void ParseTimeSpan() => Inputs[11].Parse<TimeSpan>();
        [Benchmark]
        public void ParseSampleEnum() => Inputs[12].Parse<SampleEnum>();
        [Benchmark]
        public void ParseUri() => Inputs[13].Parse<Uri>();
        [Benchmark]
        public void ParseJObject() => Inputs[14].Parse<JObject>();
    }
}
