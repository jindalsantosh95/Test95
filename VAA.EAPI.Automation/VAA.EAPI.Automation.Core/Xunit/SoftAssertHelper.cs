using Xunit;
using Xunit.Sdk;

namespace VAA.EAPI.Automation.Core.Xunit
{
    public class SoftAssertHelper
    {
        private readonly List<Exception> exceptions = new List<Exception>();

        public void AreEqual(object expected, object actual, string message = "")
        {
            try
            {
                Assert.Equal(expected, actual);
            }
            catch (Exception ex)
            {
                exceptions.Add(new XunitException(message, ex));
            }
        }

        public void True(bool condition, string message)
        {
            try
            {
                Assert.True(condition);
            }
            catch (Exception ex)
            {
                exceptions.Add(new XunitException(message, ex));
            }
        }

        public void False(bool condition, string message)
        {
            try
            {
                Assert.False(condition);
            }
            catch (Exception ex)
            {
                exceptions.Add(new XunitException(message, ex));
            }
        }

        public void Fail(string message)
        {
            exceptions.Add(new XunitException(message));
        }

        public void AssertAll()
        {
            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
        }
    }
}
