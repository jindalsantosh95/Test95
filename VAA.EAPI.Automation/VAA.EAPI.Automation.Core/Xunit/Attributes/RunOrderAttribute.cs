namespace VAA.EAPI.Automation.Core.Xunit.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class RunOrderAttribute : Attribute
    {
        public int Order { get; }

        public RunOrderAttribute(int order)
        {
            Order = order;
        }
    }
}
