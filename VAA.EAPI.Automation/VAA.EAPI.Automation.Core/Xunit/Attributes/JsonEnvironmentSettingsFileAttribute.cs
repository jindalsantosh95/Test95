using VAA.EAPI.Automation.Core.Helpers;

namespace VAA.EAPI.Automation.Core.Xunit.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class JsonEnvironmentSettingsFileAttribute : Attribute
    {
        public JsonEnvironmentSettingsFileAttribute(bool optional = true, bool forceOverride = true, string root = ".") => ConfigReader.LoadEnvironmentSettingsFile(optional, forceOverride, root);
    }
}
