using Xunit.Abstractions;

namespace VAA.EAPI.Automation.Utility.PNRCreation
{
    public class TestOutput : ITestOutputHelper
    {
        TextWriter output;

        public TestOutput(TextWriter output)
        {
            this.output = output;
        }

        public void WriteLine(string message) => this.output?.WriteLine(message);

        public void WriteLine(string format, params object[] args) => this.output?.WriteLine(format, args);
    }
}
