using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using VAA.EAPI.Core.Xunit.Attributes;
using Xunit.Sdk;
using Xunit.v3;

namespace VAA.EAPI.Core.Xunit
{
    /// <summary>
    /// Apply the following attribute to the target test class where you require your tests to be ordered
    /// 
    // [TestCaseOrderer(typeof(VAA.EAPI.Core.Xunit.TestCaseRunOrder))]
    /// </summary>
    public sealed class TestCaseRunOrder : ITestCaseOrderer
    {
        private const string OrderPropertyName = "Value";

        public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases) where TTestCase : notnull, ITestCase
        {
            var sortedMethods = new SortedDictionary<int, List<TTestCase>>();

            foreach (TTestCase testCase in testCases)
            {
                if (testCase is not IXunitTestCase xunitTestCase)
                {
                    throw new InvalidOperationException("Test ordering requires an xUnit test case with method metadata.");
                }

                var testMethod = xunitTestCase.TestMethod.Method;
                int order = testMethod.GetCustomAttributes<OrderAttribute>(false).FirstOrDefault()?.Value ?? 0;

                GetOrCreate(sortedMethods, order).Add(testCase);
            }

            var orderedTestCases = sortedMethods.Keys.OrderBy(x => x)
                                                     .Select(order =>
                                                     {
                                                         sortedMethods[order].Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.TestMethod!.MethodName, y.TestMethod!.MethodName));
                                                     
                                                         return sortedMethods[order];
                                                     })
                                                     .SelectMany(x => x)
                                                     .ToList();

            return new ReadOnlyCollection<TTestCase>(orderedTestCases);
        }

        private static TValue GetOrCreate<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
        {
            if (dictionary.TryGetValue(key, out var result)) return result;

            dictionary[key] = new TValue();

            return dictionary[key];
        }
    }
}
