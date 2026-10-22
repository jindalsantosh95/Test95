using System.Reflection;
using Xunit;

namespace VAA.EAPI.Automation.Core.Xunit.Attributes
{
    public sealed class IterationAttribute : ClassDataAttribute
    {
        public int Count { get; }

        public IterationAttribute(int count) : base(typeof(object))
        {
            Count = count;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod) => Enumerable.Range(1, Count).Select(x => new object[] { x });
    }
}
