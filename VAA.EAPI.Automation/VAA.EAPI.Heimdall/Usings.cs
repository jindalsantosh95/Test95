global using Xunit;
global using Xunit.Abstractions;
global using ConfigReader = VAA.EAPI.Automation.Core.Helpers.ConfigReader;

[assembly: TestFramework(typeName: "VAA.EAPI.Automation.Core.Xunit.ParallelXunitTestFramework", assemblyName: "VAA.EAPI.Automation.Core")]
[assembly: VAA.EAPI.Automation.Core.Xunit.Attributes.JsonEnvironmentSettingsFile()]