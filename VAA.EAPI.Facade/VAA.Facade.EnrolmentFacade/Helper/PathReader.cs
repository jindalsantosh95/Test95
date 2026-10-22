using System;

namespace VAA.Facade.EnrolmentFacade.Helper
{
    public class PathReader
    {
        public static string SetFunctionAppDirRootPath()
        {
            var localRoot = Environment.GetEnvironmentVariable("AzureWebJobsScriptRoot");
            var azureRoot = $"{Environment.GetEnvironmentVariable("HOME")}/site/wwwroot";
            Constants.Constants.FunctionAppDirPath = localRoot ?? azureRoot;
            return Constants.Constants.FunctionAppDirPath;
        }
    }
}
