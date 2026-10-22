global using Xunit;
global using ConfigReader = VAA.EAPI.Automation.Core.Helpers.ConfigReader;
global using VAA.EAPI.Automation.Core.Helpers.StorageAccount;

[assembly: TestFramework(typeName: "VAA.EAPI.Automation.Core.Xunit.ParallelXunitTestFramework", assemblyName: "VAA.EAPI.Automation.Core")]
[assembly: VAA.EAPI.Automation.Core.Xunit.Attributes.JsonEnvironmentSettingsFile()]
