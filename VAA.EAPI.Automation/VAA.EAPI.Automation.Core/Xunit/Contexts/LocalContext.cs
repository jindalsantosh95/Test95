using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Text;

namespace VAA.EAPI.Automation.Core.Xunit.Contexts
{
    internal sealed class LocalContext : ITestContext
    {
        private readonly static string InvalidFileNameCharsRegexPattern = $"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()))}]";
        public static ITestContext Current { get; private set; } = new LocalContext();

        public bool CanAccess { get; }

        private readonly string attachmentPath;

        private LocalContext()
        {
            attachmentPath = Path.Combine(ConfigReader.Get("DTA.LocalTestDropPath", new FileInfo(Environment.ProcessPath!).Directory!.FullName), "attachments");

            try
            {
                Directory.CreateDirectory(attachmentPath);
            }
            catch { }

            CanAccess = Directory.Exists(attachmentPath);
        }

        public bool Attach(string filePath) => Attach(filePath, out _);
        public bool Attach(string filePath, out Exception exception)
        {
            exception = null;

            if (!CanAccess)
                return false;

            string fileName = Path.GetFileName(filePath);

            using Stream stream = File.OpenRead(filePath);

            return Attach(stream, fileName, out exception);
        }

        public bool Attach(StringBuilder stringBuilder, string fileName) => Attach(stringBuilder, fileName, out _);
        public bool Attach(StringBuilder stringBuilder, string fileName, out Exception exception)
        {
            exception = null;

            if (!CanAccess)
                return false;

            using MemoryStream memoryStream = new();
            using StreamWriter streamWriter = new(memoryStream);

            streamWriter.Write(stringBuilder.ToString());

            streamWriter.Flush();

            memoryStream.Position = 0;

            return Attach(memoryStream, fileName, out exception);
        }

        public bool Attach(ReadOnlySpan<byte> bytes, string fileName) => Attach(bytes, fileName, out _);
        public bool Attach(ReadOnlySpan<byte> bytes, string fileName, out Exception exception)
        {
            exception = null;

            if (!CanAccess)
                return false;

            using MemoryStream memoryStream = new();

            memoryStream.Write(bytes);

            memoryStream.Position = 0;

            return Attach(memoryStream, fileName, out exception);
        }

        public bool Attach(Stream stream, string fileName) => Attach(stream, fileName, out _);
        public bool Attach(Stream stream, string fileName, out Exception exception)
        {
            exception = null;

            if (!CanAccess)
                return false;

            fileName = RegexCompiled.Replace(fileName, InvalidFileNameCharsRegexPattern, string.Empty);

            string path = Path.Combine(attachmentPath, fileName);
            bool hasErrored = false;

            try
            {
                using FileStream fileStream = new(path, FileMode.Create, FileAccess.Write);

                stream.CopyTo(fileStream);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred whilst attempting to attach file '{path}'{Environment.NewLine}{ex.Message}");

                exception = ex;
                hasErrored = true;
            }
            finally
            {
                Debug.WriteLine($"Attempted to attach file '{path}'");
            }

            return !hasErrored && File.Exists(attachmentPath);
        }

        public void Complete() => Complete(out _);
        public void Complete(out Exception exception)
        {
            exception = default;
        }
    }
}
