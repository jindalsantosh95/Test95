using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace VAA.EAPI.Automation.Core.Xunit
{
    public sealed class ParallelXunitTestFramework : XunitTestFramework
    {
        public ParallelXunitTestFramework(IMessageSink diagnosticMessageSink) : base(diagnosticMessageSink) { }

        protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName) => new ParallelXunitTestFrameworkExecutor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);
    }
}
