using Xunit;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.Core.Xunit
{
    internal class ExecutionMessageSink : LongLivedMarshalByRefObject, IMessageSink
    {
        private IMessageSink messageSink;

        public ExecutionMessageSink(IMessageSink messageSink) => this.messageSink = messageSink;

        public bool OnMessage(IMessageSinkMessage message)
        {
            switch (message.GetType().FullName)
            {
                case "Xunit.Sdk.TestAssemblyStarting":

                    break;
                case "Xunit.Sdk.TestCollectionStarting":

                    break;
                case "Xunit.Sdk.TestClassStarting":

                    break;
                case "Xunit.Sdk.TestMethodStarting":

                    break;
                case "Xunit.Sdk.TestCaseStarting":

                    break;
                case "Xunit.Sdk.TestStarting":

                    //var type = message.GetType();
                    //var xunitTest = (XunitTest)type.GetProperty("Test").GetValue(message);
                    //var testAssembly = (TestAssembly)type.GetProperty("TestAssembly").GetValue(message);

                    //if (LocalContext.Current.CanAccess)
                    //    TestContext.Current = (TestContext)(testAssembly, xunitTest, LocalContext.Current);

                    break;
                case "Xunit.Sdk.TestFailed":

                    break;
                case "Xunit.Sdk.TestFinished":

                    break;
                case "Xunit.Sdk.TestCaseFinished":

                    break;
                case "Xunit.Sdk.TestMethodFinished":

                    break;
                case "Xunit.Sdk.TestClassFinished":

                    break;
                case "Xunit.Sdk.TestCollectionFinished":

                    break;
                case "Xunit.Sdk.TestAssemblyFinished":

                    break;
            }

            return this.messageSink.OnMessage(message);
        }
    }
}
