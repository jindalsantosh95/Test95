using System.Linq.Expressions;
using VAA.EAPI.Automation.Core.Extensions;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.Core.Xunit.Runners
{
    public sealed class FilterExpression
    {
        public static explicit operator FilterExpression(string source) => new FilterExpression(source);

        private readonly Group group = default;

        private FilterExpression(string filterExpression)
        {
            if (filterExpression.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(filterExpression));

            group = (Group)filterExpression;
        }

        public bool IsValid(ITestCase testCase) => group.IsValid(testCase);
    }
}
