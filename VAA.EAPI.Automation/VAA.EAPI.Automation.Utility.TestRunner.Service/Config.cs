using VAA.EAPI.Automation.Core.Helpers;

namespace VAA.EAPI.Automation.Utility.TestRunner.Service
{
    public static class Config
    {
        public static class TestRun
        {
            public static string Assemblies => ConfigReader.Get("Configuration:TestRun:Assemblies", string.Empty)!;
            public static bool InParallel => ConfigReader.Get("Configuration:TestRun:InParallel", false)!;
            public static string Filter => ConfigReader.Get("Configuration:TestRun:Filter", string.Empty)!;
            public static string TimerInterval => ConfigReader.Get("Configuration:TestRun:TimerInterval", string.Empty)!;
        }
    }
}
