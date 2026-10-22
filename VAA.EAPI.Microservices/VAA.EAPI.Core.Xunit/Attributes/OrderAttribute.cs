namespace VAA.EAPI.Core.Xunit.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class OrderAttribute(int value) : Attribute
    {
        public int Value { get; } = value;
    }
}
