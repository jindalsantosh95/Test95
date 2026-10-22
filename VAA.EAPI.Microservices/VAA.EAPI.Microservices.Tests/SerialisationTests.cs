using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;
using System.Diagnostics;
using VAA.EAPI.Common.Diagnostics;
using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Microservices.Tests
{
    public sealed class SerialisationTests
    {
        private enum EnumType
        {
            No = 0,
            Yes = 1
        };

        private sealed class TestClass
        {
            public byte Byte { get; set; }
            public byte[] Bytes { get; set; }
            public sbyte SByte { get; set; }
            public short Short { get; set; }
            public ushort UShort { get; set; }
            public int Int { get; set; }
            public uint UInt { get; set; }
            public long Long { get; set; }
            public ulong ULong { get; set; }
            public decimal Decimal { get; set; }
            public float Float { get; set; }
            public double Double { get; set; }
            public bool Boolean { get; set; }
            public char Char { get; set; }
            public string String { get; set; }
            public DateTime DateTime { get; set; }
            public TimeSpan TimeSpan { get; set; }
            public Guid Guid { get; set; }
            public EnumType Enum { get; set; }

            public TestClass[] Array { get; set; }
            public IEnumerable<TestClass> Enumerable { get; set; }

            public TestClass Custom { get; set; }
        }

        private sealed class TestClassDifferentOrder
        {
            public bool Boolean { get; set; }
            public char Char { get; set; }
            public string String { get; set; }

            public DateTime DateTime { get; set; }
            public TimeSpan TimeSpan { get; set; }
            public Guid Guid { get; set; }
            public EnumType Enum { get; set; }

            public TestClass[] Array { get; set; }
            public IEnumerable<TestClass> Enumerable { get; set; }

            public TestClass Custom { get; set; }

            public byte Byte { get; set; }
            public sbyte SByte { get; set; }
            public short Short { get; set; }
            public ushort UShort { get; set; }
            public byte[] Bytes { get; set; }
            public int Int { get; set; }
            public uint UInt { get; set; }
            public long Long { get; set; }
            public ulong ULong { get; set; }
            public decimal Decimal { get; set; }
            public float Float { get; set; }
            public double Double { get; set; }
        }

        //[SimpleJob(RunStrategy.ColdStart, iterationCount: 5)]
        //[MinColumn, MaxColumn, MeanColumn, MedianColumn]
        //[MemoryDiagnoser]
        //[ThreadingDiagnoser]
        [SimpleJob(RunStrategy.ColdStart, iterationCount: 1000)]
        [MinColumn, MaxColumn, MeanColumn, MedianColumn, IterationsColumn]
        public class Benchmark
        {
            private readonly TestClass instance;
            private readonly byte[] seriliasedBytes;
            private readonly string serialisedJson;

            public Benchmark()
            {
                int depth = 1;

                instance = Generate(ref depth);

                seriliasedBytes = Common.Serialisation.Binary.Serialize(instance);
                serialisedJson = Common.Serialisation.Json.Serialize(instance);
            }

            [Benchmark]
            public byte[] BinarySerialize() => Common.Serialisation.Binary.Serialize(instance);

            [Benchmark]
            public string JsonSerialize() => Common.Serialisation.Json.Serialize(instance);

            [Benchmark]
            public object BinaryDeserialize() => Common.Serialisation.Binary.Deserialize<TestClass>(seriliasedBytes);

            [Benchmark]
            public object JsonDeserialize() => Common.Serialisation.Json.Deserialize<TestClass>(serialisedJson);
        }

        [Fact]
        private void BenchmarkDotNetSerialisation()
        {
            var config = new ManualConfig()
               .AddLogger(ConsoleLogger.Default)
               .AddDiagnoser(MemoryDiagnoser.Default)
               .AddColumnProvider(DefaultColumnProviders.Instance)
               .WithOptions(ConfigOptions.DisableOptimizationsValidator)
               .AddValidator(JitOptimizationsValidator.DontFailOnError);

            var summary = BenchmarkRunner.Run<Benchmark>(config);
        }

        [Fact]
        private void BinaryJsonSerialisation()
        {
            //var summary = BenchmarkRunner.Run<Benchmark>();

            int depth = 10;

            var instance = Generate(ref depth);

            var enumeration = new TestClass[] { instance };

            // we do this to make the test fair by execcuting a prefetch
            _ = Common.Serialisation.Binary.Serialize(instance);
            _ = Common.Serialisation.Json.Serialize(instance);

            Trace<SerialisationTests> trace = new();

            List<TimeSpan> binarySerialize = [];
            List<TimeSpan> binaryDeserialize = [];
            List<TimeSpan> jsonSerialize = [];
            List<TimeSpan> jsonDeserialize = [];

            for (int i = 0; i < 10; i++)
            {
                var serilizedToBytes = Common.Serialisation.Binary.Serialize(instance);

                binarySerialize.Add(trace.Stamp());

                var deserializedBytes = Common.Serialisation.Binary.Deserialize<TestClass>(serilizedToBytes);

                binaryDeserialize.Add(trace.Stamp());

                var serializedToJson = Common.Serialisation.Json.Serialize(instance);

                jsonSerialize.Add(trace.Stamp());

                var deserializedJson = Common.Serialisation.Json.Deserialize<TestClass>(serializedToJson);

                jsonDeserialize.Add(trace.Stamp());
            }

            var binary = Common.Serialisation.Binary.Serialize(instance);
            var json = Common.Serialisation.Json.Serialize(instance);
            var avgBinarySerialize = binarySerialize.Average(x => x.TotalMilliseconds);
            var avgBinaryDeserialize = binaryDeserialize.Average(x => x.TotalMilliseconds);
            var avgJsonSerialize = jsonSerialize.Average(x => x.TotalMilliseconds);
            var avgJsonDeserialize = jsonDeserialize.Average(x => x.TotalMilliseconds);

            Debug.WriteLine(string.Empty);
            Debug.WriteLine("Result");
            Debug.WriteLine($"Format \tLength \tSerialize \tDeserialize");
            Debug.WriteLine($"binary \t{binary.Length} \t{Math.Round(avgBinarySerialize, 6)} \t{Math.Round(avgBinaryDeserialize, 6)}");
            Debug.WriteLine($"json   \t{json.Length} \t{Math.Round(avgJsonSerialize, 6)} \t{Math.Round(avgJsonDeserialize, 6)}");

            Debug.WriteLine(string.Empty);
            Debug.WriteLine($"{Math.Round(100 - (binary.Length / (double)json.Length * 100), 3)}% difference in size.");
            Debug.WriteLine($"{Math.Round(100 - (avgBinarySerialize / avgJsonSerialize * 100), 3)}% difference in serialisation speed.");
            Debug.WriteLine($"{Math.Round(100 - (avgBinaryDeserialize / avgJsonDeserialize * 100), 3)}% difference in deserialisation speed.");

            var buffer = Common.Serialisation.Binary.Serialize(instance);
            var differntClassType = Common.Serialisation.Binary.Deserialize<TestClassDifferentOrder>(buffer);

            //check that binary is faster than json
            Assert.True(avgBinarySerialize < avgJsonSerialize);
            Assert.True(avgBinaryDeserialize < avgJsonDeserialize);

            //sample to check that the differnt order of properties
            //didn't affect the de serialisation process
            Assert.Equal(instance.String, differntClassType.String);
            Assert.Equal(instance.Guid, differntClassType.Guid);
            Assert.Equal(instance.Int, differntClassType.Int);
        }

        [Fact]
        private void BinaryJsonSerialisationCompression()
        {
            int depth = 10;

            var instance = Generate(ref depth);

            var enumeration = new TestClass[] { instance };

            // we do this to make the test fair by execcuting a prefetch
            _ = Common.Serialisation.Binary.Serialize(instance);
            _ = Common.Serialisation.Json.Serialize(instance);

            var serilizedToBytes = Common.Serialisation.Binary.Serialize(instance);
            var compressedBytes = serilizedToBytes.Compress();
            var decompressedBytes = compressedBytes.Decompress();

            var serializedToJson = Common.Serialisation.Json.Serialize(instance);
            var compressedJson = serializedToJson.Compress();
            var decompressedJson = compressedJson.Decompress();

            Debug.WriteLine(string.Empty);
            Debug.WriteLine($"serilizedToBytes: {serilizedToBytes.Length}");
            Debug.WriteLine($"compressedBytes: {compressedBytes.Length}, {Math.Round(100 - ((double)compressedBytes.Length / serilizedToBytes.Length * 100), 3)}%");
            Debug.WriteLine($"decompressedBytes: {decompressedBytes.Length}");

            Debug.WriteLine(string.Empty);
            Debug.WriteLine($"serializedToJson: {serializedToJson.Length}");
            Debug.WriteLine($"compressedJson: {compressedJson.Length}, {Math.Round(100 - ((double)compressedJson.Length / serializedToJson.Length * 100), 3)}%");
            Debug.WriteLine($"decompressedJson: {decompressedJson.Length}");

            //check that binary is faster than json
            Assert.True(compressedBytes.Length < compressedJson.Length);
        }

        private static TestClass Generate(ref int depth)
        {
            var instance = new TestClass()
            {
                Byte = 1,
                Bytes = [1, 2, 3, 4, 5],
                SByte = 2,
                Short = 3,
                UShort = 4,
                Int = 5,
                UInt = 6,
                Long = 9,
                ULong = 10,
                Decimal = 11,
                Float = 12,
                Double = 13,
                Boolean = true,
                Char = char.MinValue,
                String = "String",
                DateTime = DateTime.Now,
                TimeSpan = TimeSpan.Zero,
                Guid = Guid.NewGuid(),
                Enum = EnumType.Yes,
                Array =
                [
                    new()
                    {
                        Byte = 1,
                        Bytes = [6, 7, 8],
                        SByte = 2,
                        Short = 3,
                        UShort = 4,
                        Int = 5,
                        UInt = 6,
                        Long = 9,
                        ULong = 10,
                        Decimal = 11,
                        Float = 12,
                        Double = 13,
                        Boolean = true,
                        Char = char.MinValue,
                        String = "String",
                        DateTime = DateTime.Now,
                        TimeSpan = TimeSpan.Zero,
                        Guid = Guid.NewGuid(),
                        Enum = EnumType.Yes
                    }
                ],
                Enumerable =
                [
                    new()
                    {
                        Byte = 1,
                        Bytes = [9, 10, 11],
                        SByte = 2,
                        Short = 3,
                        UShort = 4,
                        Int = 5,
                        UInt = 6,
                        Long = 9,
                        ULong = 10,
                        Decimal = 11,
                        Float = 12,
                        Double = 13,
                        Boolean = true,
                        Char = char.MinValue,
                        String = "String",
                        DateTime = DateTime.Now,
                        TimeSpan = TimeSpan.Zero,
                        Guid = Guid.NewGuid(),
                        Enum = EnumType.Yes
                    }
                ],
                Custom = new()
                {
                    Bytes = [12, 13, 14],
                    Byte = 1,
                    SByte = 2,
                    Short = 3,
                    UShort = 4,
                    Int = 5,
                    UInt = 6,
                    Long = 9,
                    ULong = 10,
                    Decimal = 11,
                    Float = 12,
                    Double = 13,
                    Boolean = true,
                    Char = char.MinValue,
                    String = "String",
                    DateTime = DateTime.Now,
                    TimeSpan = TimeSpan.Zero,
                    Guid = Guid.NewGuid(),
                    Enum = EnumType.Yes
                }
            };

            if (depth > 0)
            {
                depth--;

                instance.Custom = Generate(ref depth);
            }

            return instance;
        }
    }
}
