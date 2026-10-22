using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace VAA.EAPI.Automation.Core.Xunit.Fixtures
{
    public sealed class CsvFixture<T> : IDisposable where T : class
    {
        internal sealed class TypeStringBuilder
        {
            public static explicit operator TypeStringBuilder((StringBuilder, int, string, string) source) => new()
            {
                StringBuilder = source.Item1,
                Activations = source.Item2,
                FilePath = source.Item3,
                FinalSavePath = source.Item4
            };

            public StringBuilder StringBuilder { get; set; }
            public int Activations { get; set; }
            public bool HeadersWritten { get; set; }
            public string FilePath { get; set; }
            public string FinalSavePath { get; set; }
        }

        private static readonly ConcurrentDictionary<Type, TypeStringBuilder> _cache = [];

        public string FilePath => _context.FilePath;

        private readonly TypeStringBuilder _context;

        public CsvFixture()
        {
            lock (_cache)
            {
                _context = _cache.GetOrAdd(typeof(T), (TypeStringBuilder)(new StringBuilder(), 0, Path.Combine(Path.GetTempPath(), $"testdata_{Guid.NewGuid()}.csv"), string.Empty));

                // track number of times we need this for a test class
                _context.Activations++;
            }
        }

        public void RegisterHeaders(string headers)
        {
            if (_context.HeadersWritten)
                return;

            lock (_cache)
            {
                if (_context.HeadersWritten)
                    return;

                _context.HeadersWritten = true;

                AppendLine(headers);
            }
        }

        public void AppendLine(string text)
        {
            _context.StringBuilder.AppendLine(text);
        }

        public void FileName(string filename)
        {
            _context.FinalSavePath = filename;
        }

        public void Dispose()
        {
            lock (_cache)
            {
                _context.Activations--;

                if (_context.Activations == 0)
                {
                    File.WriteAllText(FilePath, _context.StringBuilder.ToString());

                    File.Copy(FilePath, _context.FinalSavePath, overwrite: true);

                    try
                    {
                        // Delete the temp file
                        if (File.Exists(FilePath))
                        {
                            File.Delete(FilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to delete temp file {FilePath}: {ex.Message}");
                    }
                }
            }
        }
    }
}
