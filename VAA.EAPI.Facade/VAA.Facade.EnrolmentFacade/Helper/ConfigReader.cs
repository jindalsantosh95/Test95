using System;
using System.IO;

namespace VAA.Facade.EnrolmentFacade.Helper { 
    public static class ConfigReader
    {
        public static string ReadJsonFileContent(string rootPath, string filepath)
        {
            string filePath = Path.GetFullPath(Path.Combine(rootPath, filepath));

            using (StreamReader stream = new StreamReader(filePath))
            {
                return stream.ReadToEnd();
            }
        }

        public static string GetConfigurationSettings(string key)
        {
            return Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process);
        }

    }
}
