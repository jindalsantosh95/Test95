using System.ComponentModel;

namespace VAA.EAPI.Automation.Core.UI.Xunit.Features.Atttributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class GivenAttribute(string description) : DescriptionAttribute(description)
    {
    }
}
