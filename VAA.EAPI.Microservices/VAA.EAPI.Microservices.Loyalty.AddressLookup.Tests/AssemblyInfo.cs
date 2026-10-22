using Xunit;

// ConfigReader.Set also changes process-wide environment variables, including keys shared by v2 and v3 tests.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
