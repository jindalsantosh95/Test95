using System.Text;

namespace VAA.EAPI.Automation.Core.Xunit
{
    public sealed class TestException : Exception
    {
        public static explicit operator TestException((string messageType, string message, string stackTrace, string type) source) => new(source.messageType, source.message, source.stackTrace, source.type);

        private readonly string stackTrace;

        public TestException(string messageType, string message, string stackTrace, string type) : base(Newtonsoft.Json.JsonConvert.SerializeObject(new
        {
            Test = messageType ?? throw new ArgumentNullException(nameof(messageType)),
            Message = message ?? $"An error of type {messageType} has occured",
            StackTrace = stackTrace ?? string.Empty,
            Type = type ?? "Unknown exception type",
        }))
        {
            MessageType = messageType ?? throw new ArgumentNullException(nameof(messageType));
            this.stackTrace = stackTrace ?? string.Empty;
            Type = type ?? "Unknown exception type";
        }

        public override string StackTrace
        {
            get => stackTrace;
        }

        public string MessageType { get; private set; }
        public string Type { get; private set; }
        public decimal ExecutionTime { get; private set; }
        public string Output { get; private set; }

        public override string ToString() => Message;
    }
}
