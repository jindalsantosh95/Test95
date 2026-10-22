using System.Reflection;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Contexts;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace VAA.EAPI.Automation.Core.Xunit
{
    internal sealed class ParallelXunitTestFrameworkExecutor : XunitTestFrameworkExecutor
    {
        public ParallelXunitTestFrameworkExecutor(AssemblyName assemblyName, ISourceInformationProvider sourceInformationProvider, IMessageSink diagnosticMessageSink) : base(assemblyName, sourceInformationProvider, diagnosticMessageSink)
        {
            // we do this invoke the assembly attribute
            IEnumerable<IAttributeInfo> jsonEnvironmentSettingsFileAttributes = TestAssembly.Assembly.GetCustomAttributes(typeof(JsonEnvironmentSettingsFileAttribute));

            WebRequest.PreLoadHttpClients();
        }

        public override async void RunTests(IEnumerable<ITestCase> testCases, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions)
        {
            ITestCase testCase = testCases.First();
            ITestAssembly testAssembly = testCase.TestMethod.TestClass.TestCollection.TestAssembly;

            // initialise test context
            //if (AzureDevOpsContext.Current.CanAccess)
            //    TestContext.Current = (TestContext)(testAssembly, testCase, AzureDevOpsContext.Current);
            //else if (LocalContext.Current.CanAccess)
            //    TestContext.Current = (TestContext)(testAssembly, testCase, LocalContext.Current);

            if (LocalContext.Current.CanAccess)
                TestContext.Current = (TestContext)(testAssembly, testCase, LocalContext.Current);

            base.RunTests(testCases, executionMessageSink, executionOptions);
        }

        protected override void RunTestCases(IEnumerable<IXunitTestCase> testCases, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions)
            => base.RunTestCases(SetUpTestCaseParallelization(testCases), executionMessageSink, executionOptions);

        /// <summary>
        /// By default, all test cases in a test class share the same collection instance which ensures they run synchronously.
        /// By providing a unique test collection instance to every test case in a test class you can make them all run in parallel.
        /// </summary>
        private IXunitTestCase[] SetUpTestCaseParallelization(IEnumerable<IXunitTestCase> testCases)
        {
            List<IXunitTestCase> result = new();

            if (testCases.Any())
            {
                ITestClass testClass = testCases.First().TestMethod.TestClass;

                if (testClass.Class.GetCustomAttributes(typeof(global::Xunit.TestCaseOrdererAttribute)).Any())
                {
                    dynamic testCaseOrdererAttribute = testClass.Class.GetCustomAttributes(typeof(global::Xunit.TestCaseOrdererAttribute)).First();

                    dynamic constructorArguments = testCaseOrdererAttribute.AttributeData.ConstructorArguments;

                    string assemblyQualifiedAttributeTypeName = $"{constructorArguments[0].Value},{constructorArguments[1].Value}";

                    ITestCaseOrderer orderer = Activator.CreateInstance(Type.GetType(assemblyQualifiedAttributeTypeName)) as ITestCaseOrderer;

                    testCases = orderer.OrderTestCases(testCases);
                }
            }

            return testCases.Select(testCase =>
                             {
                                 ITestMethod oldTestMethod = testCase.TestMethod;
                                 ITestClass oldTestClass = oldTestMethod.TestClass;
                                 ITestCollection oldTestCollection = oldTestMethod.TestClass.TestCollection;

                                 // If the collection is explicitly set, don't try to parallelize test execution
                                 if (oldTestCollection.CollectionDefinition != null || oldTestClass.Class.GetCustomAttributes(typeof(global::Xunit.CollectionAttribute)).Any())
                                     return testCase;

                                 // Preserve skippable-fact test case types so dynamic skip behavior is not lost.
                                 if (testCase.GetType().FullName?.Contains("Skippable", StringComparison.OrdinalIgnoreCase) == true)
                                     return testCase;

                                 Guid uniqueId = Guid.NewGuid();

                                 // Create a new collection with a unique id for the test case.
                                 TestCollection newTestCollection = new(oldTestCollection.TestAssembly, oldTestCollection.CollectionDefinition, displayName: $"{oldTestCollection.DisplayName} {uniqueId}")
                                 {
                                     UniqueID = uniqueId
                                 };

                                 // Duplicate the test and assign it to the new collection
                                 TestClass newTestClass = new(newTestCollection, oldTestClass.Class);
                                 TestMethod newTestMethod = new(newTestClass, oldTestMethod.Method);

                                 return testCase switch
                                 {
                                     // Used by Theory having DisableDiscoveryEnumeration or non-serializable data
                                     XunitTheoryTestCase xunitTheoryTestCase => new XunitTheoryTestCase(DiagnosticMessageSink, GetTestMethodDisplay(xunitTheoryTestCase), GetTestMethodDisplayOptions(xunitTheoryTestCase), newTestMethod),
                                     // Used by all other tests
                                     XunitTestCase xunitTestCase => new XunitTestCase(DiagnosticMessageSink, GetTestMethodDisplay(xunitTestCase), GetTestMethodDisplayOptions(xunitTestCase), newTestMethod, xunitTestCase.TestMethodArguments),
                                     // TODO If you use custom attribute, you may need to add cases here
                                     _ => throw new ArgumentOutOfRangeException("Test case " + testCase.GetType() + " not supported"),
                                 };
                             })
                            .OrderBy(x => x.DisplayName)
                            .ToArray();
        }

        private static TestMethodDisplay GetTestMethodDisplay(TestMethodTestCase testMethodTestCase) => (TestMethodDisplay)typeof(TestMethodTestCase).GetProperty("DefaultMethodDisplay", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(testMethodTestCase)!;

        private static TestMethodDisplayOptions GetTestMethodDisplayOptions(TestMethodTestCase testMethodTestCase) => (TestMethodDisplayOptions)typeof(TestMethodTestCase).GetProperty("DefaultMethodDisplayOptions", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(testMethodTestCase)!;
    }
}
