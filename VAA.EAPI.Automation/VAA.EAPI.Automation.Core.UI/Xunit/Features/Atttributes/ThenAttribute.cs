using System.ComponentModel;

namespace VAA.EAPI.Automation.Core.UI.Xunit.Features.Atttributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ThenAttribute(string description) : DescriptionAttribute(description)
    {
    }
}
