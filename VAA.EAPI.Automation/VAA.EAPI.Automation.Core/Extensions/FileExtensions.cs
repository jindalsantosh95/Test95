namespace VAA.EAPI.Automation.Core.Extensions
{
    public static class FileExtensions
    {
        public static string ReadFile(string filename, string fileType)
        {
            return File.ReadAllText($"Requests\\Body\\{filename}.{fileType}");
        }
    }
}