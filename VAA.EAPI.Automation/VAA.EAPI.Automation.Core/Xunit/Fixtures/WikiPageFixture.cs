using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;

namespace VAA.EAPI.Automation.Core.Xunit.Fixtures
{
    public sealed class WikiPageFixture<T> : IDisposable where T : class
    {
        internal sealed class TypeStringBuilder
        {
            public static explicit operator TypeStringBuilder(StringBuilder source) => new()
            {
                StringBuilder = source,
                Activations = 0,
            };

            private readonly ConcurrentDictionary<string, StringBuilder> _tables = [];
            private readonly ConcurrentDictionary<string, string> _tableTags = [];


            public StringBuilder StringBuilder { get; set; }
            public int Activations { get; set; }
            public string PagePath { get; set; }


            public string CreateTable(params string[] headers)
            {
                string header = $"| {string.Join(" | ", headers)} |\n|{string.Join('|', headers.Select(x => string.Empty.PadLeft(x.Length + 2, '-')))}|";
                string key = _tableTags.Any(x => x.Value == header) ? _tableTags.First(x => x.Value == header).Key : default;

                if (!key.IsNullOrWhiteSpace())
                    return key;

                lock (_tables)
                {
                    key = Guid.NewGuid().ToString("N");

                    if (_tableTags.TryAdd(key, header) && _tables.TryAdd(header, new StringBuilder()))
                    {
                        _tables[header].AppendLine(header);

                        StringBuilder.AppendLine(key);
                    }
                }

                return key;
            }

            public void WriteTableRow(string tableKey, params string[] values)
            {
                if (_tableTags.TryGetValue(tableKey, out string headerKey))
                    if (_tables.TryGetValue(headerKey, out System.Text.StringBuilder table))
                        table.AppendLine($"|{string.Join('|', values)}|");
            }

            public void WriteTableRow(string tableKey, string value)
            {
                if (_tableTags.TryGetValue(tableKey, out string headerKey))
                    if (_tables.TryGetValue(headerKey, out System.Text.StringBuilder table))
                        table.AppendLine(value);
            }

            public string BuildPage()
            {
                if (_tables.IsEmpty)
                    return StringBuilder.ToString();

                foreach (string tableTag in _tableTags.Keys)
                    if (_tableTags.TryGetValue(tableTag, out string tableKey))
                        if (_tables.TryGetValue(tableKey, out System.Text.StringBuilder table))
                            StringBuilder.Replace(tableTag, table.ToString());

                return StringBuilder.ToString();
            }
        }

        private static readonly ConcurrentDictionary<Type, TypeStringBuilder> _cache = [];

        public string PagePath => _context.PagePath;

        private readonly TypeStringBuilder _context;

        public WikiPageFixture()
        {
            lock (_cache)
            {
                _context = _cache.GetOrAdd(typeof(T), (TypeStringBuilder)new StringBuilder());

                // track number of times we need this for a test class
                _context.Activations++;
            }
        }

        public void SetWikiPath(string path)
        {
            path = path?.Trim() ?? string.Empty;

            if (path.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(path));

            _context.PagePath = path;
        }

        public string CreateTable(params string[] headers) => _context.CreateTable(headers);

        public void WriteTableRow(string tableKey, params string[] values) => _context.WriteTableRow(tableKey, values);

        public void WriteTableRow(string tableKey, string value) => _context.WriteTableRow(tableKey, value);

        public void Write(string value) => _context.StringBuilder.Append(value);
        public void WriteLine(string value) => _context.StringBuilder.AppendLine(value);


        public void Dispose()
        {
            lock (_cache)
            {
                _context.Activations--;

                if (_context.Activations == 0)
                {
                    try
                    {
                        AzureDevOpsWikiUploader.UploadMarkdownAsync(_context.BuildPage(), _context.PagePath)
                                               .GetAwaiter()
                                               .GetResult();

                    }

                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to create/update wiki page {PagePath}: {ex.Message}");
                    }
                }
            }
        }
    }
}
