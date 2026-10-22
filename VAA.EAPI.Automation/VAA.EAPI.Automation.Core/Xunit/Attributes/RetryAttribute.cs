using Xunit;

namespace VAA.EAPI.Automation.Core.Xunit.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RetryAttribute : FactAttribute
    {
        public int MaxRetries { get; }

        public RetryAttribute(int maxRetries = 3)
        {
            MaxRetries = maxRetries;
        }
    }
}
