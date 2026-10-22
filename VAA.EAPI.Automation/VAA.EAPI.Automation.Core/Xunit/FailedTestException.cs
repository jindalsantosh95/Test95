using System.Text;

namespace VAA.EAPI.Automation.Core.Xunit
{
    public sealed class FailedTestException : Exception
    {
        public static explicit operator FailedTestException((string test, string message, string stackTrace, string type, decimal executionTime, string output) source) => new(source.test, source.message, source.stackTrace, source.type, source.executionTime, source.output);

        private readonly string stackTrace;

        public FailedTestException(string test, string message, string stackTrace, string type, decimal executionTime, string output) : base(Newtonsoft.Json.JsonConvert.SerializeObject(new
        {
            Test = test ?? throw new ArgumentNullException(nameof(test)),
            Message = message ?? $"An error has occured in test '{test}'",
            StackTrace = stackTrace ?? string.Empty,
            Type = type ?? "Unknown exception type",
            ExecutionTime = executionTime,
            Output = output ?? string.Empty,
        }))
        {
            Test = test ?? throw new ArgumentNullException(nameof(test));
            this.stackTrace = stackTrace ?? string.Empty;
            Type = type ?? "Unknown exception type";
            ExecutionTime = executionTime;
            Output = output ?? string.Empty;
        }

        public override string StackTrace
        {
            get => stackTrace;
        }

        public string Test { get; private set; }
        public string Type { get; private set; }
        public decimal ExecutionTime { get; private set; }
        public string Output { get; private set; }

        public override string ToString() => Message;
    }
}
