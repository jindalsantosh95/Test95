using System;

namespace VAA.Facade.AzureB2C.Shared
{
    public static class ReadConfig
    {
        public static string GetConfigurationSettings(string key)
        {
            return Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process);
        }

        public static string GetFunctionAppDirRootPath()
        {
            var localRoot = Environment.GetEnvironmentVariable("AzureWebJobsScriptRoot");
            var azureRoot = $"{Environment.GetEnvironmentVariable("HOME")}/site/wwwroot";

            return localRoot ?? azureRoot;
        }
    }
}
