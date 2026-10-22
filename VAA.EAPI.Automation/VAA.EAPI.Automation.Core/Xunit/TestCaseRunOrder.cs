using VAA.EAPI.Automation.Core.Xunit.Attributes;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace VAA.EAPI.Automation.Core.Xunit
{
    public sealed class TestCaseRunOrder : ITestCaseOrderer
    {
        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase
        {
            SortedDictionary<int, List<TTestCase>> sortedMethods = new SortedDictionary<int, List<TTestCase>>();

            foreach (TTestCase testCase in testCases)
            {
                int order = testCase.TestMethod.Method.GetCustomAttributes((typeof(RunOrderAttribute).AssemblyQualifiedName)).FirstOrDefault()?.GetNamedArgument<int>("Order") ?? 0;

                GetOrCreate(sortedMethods, order).Add(testCase);
            }

            foreach (List<TTestCase> list in sortedMethods.Keys.OrderBy(x => x).Select(order => sortedMethods[order]))
            {
                list.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.TestMethod.Method.Name, y.TestMethod.Method.Name));

                foreach (TTestCase testCase in list)
                    yield return testCase;
            }
        }

        private static TValue GetOrCreate<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
        {
            TValue result;

            if (dictionary.TryGetValue(key, out result))
                return result;

            dictionary[key] = new TValue();

            return dictionary[key];
        }
    }
}
