global using Xunit;
global using Xunit.Abstractions;

[assembly: TestFramework(typeName: "VAA.EAPI.Automation.Core.Xunit.ParallelXunitTestFramework", assemblyName: "VAA.EAPI.Automation.Core")]
[assembly: CollectionBehavior(MaxParallelThreads = 4)]
[assembly: VAA.EAPI.Automation.Core.Xunit.Attributes.JsonEnvironmentSettingsFile()]
