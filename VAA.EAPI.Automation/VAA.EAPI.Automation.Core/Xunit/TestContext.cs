using System.Text;
using VAA.EAPI.Automation.Core.Text;
using VAA.EAPI.Automation.Core.Xunit.Contexts;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.Core.Xunit
{
    public sealed class TestContext : ITestContext
    {
        private const string ParameterRegexPattern = @"\(.*\)";

        public static explicit operator TestContext((ITestAssembly testAssembly, ITestCase testCase, ITestContext testContext) source) => new(source.testAssembly, source.testCase, source.testContext);

        private static readonly AsyncLocal<TestContext> asyncLocal = new();

        public static TestContext Current
        {
            get => asyncLocal.Value;
            internal set => asyncLocal.Value = new(value);
        }

        private readonly ITestContext testContext;
        private readonly ITestAssembly testAssembly;
        private readonly ITestCase testCase;

        internal TestContext(TestContext testContext) : this(testContext.testAssembly, testContext.testCase, testContext.testContext) { }
        internal TestContext(ITestAssembly testAssembly, ITestCase testCase, ITestContext testContext)
        {
            this.testAssembly = testAssembly ?? throw new ArgumentNullException(nameof(testAssembly));
            this.testCase = testCase ?? throw new ArgumentNullException(nameof(testCase));
            this.testContext = testContext;
        }

        public bool CanAccess => testContext.CanAccess;
        public string DisplayName => RegexCompiled.Replace(testCase.DisplayName.Split('.', StringSplitOptions.RemoveEmptyEntries).Last(), ParameterRegexPattern, string.Empty);

        public bool Attach(string filePath) => testContext.Attach(filePath);
        public bool Attach(string filePath, out Exception exception) => testContext.Attach(filePath, out exception);

        public bool Attach(StringBuilder stringBuilder, string fileName) => testContext.Attach(stringBuilder, fileName);
        public bool Attach(StringBuilder stringBuilder, string fileName, out Exception exception) => testContext.Attach(stringBuilder, fileName, out exception);

        public bool Attach(ReadOnlySpan<byte> bytes, string fileName) => testContext.Attach(bytes, fileName);
        public bool Attach(ReadOnlySpan<byte> bytes, string fileName, out Exception exception) => testContext.Attach(bytes, fileName, out exception);

        public bool Attach(Stream stream, string fileName) => testContext.Attach(stream, fileName);
        public bool Attach(Stream stream, string fileName, out Exception exception) => testContext.Attach(stream, fileName, out exception);

        public void Complete() => testContext.Complete();
        public void Complete(out Exception exception) => testContext.Complete(out exception);
    }
}
