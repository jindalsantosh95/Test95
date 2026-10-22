using System;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.NDC.Order.Tests.Infrastructure.Services.InRule
{
    public sealed class InRuleConfigurationScope : IDisposable
    {
        private readonly string _inRuleEnabled;
        private readonly string _inRuleUrl;
        private readonly string _inRuleSubscriptionKey;
        private readonly string _inRuleScope;

        private InRuleConfigurationScope()
        {
            _inRuleEnabled = ConfigReader.Try(Constants.Environment.InRuleEnabled);
            _inRuleUrl = ConfigReader.Try(Constants.Environment.InRuleUrl);
            _inRuleSubscriptionKey = ConfigReader.Try(Constants.Environment.InRuleSubscriptionKey);
            _inRuleScope = ConfigReader.Try(Constants.Environment.InRuleScope);
        }

        public static InRuleConfigurationScope Create(string inRuleUrl, string inRuleScope, string inRuleSubscriptionKey, bool inRuleEnabled = true)
        {
            var scope = new InRuleConfigurationScope();

            ConfigReader.Set(Constants.Environment.InRuleEnabled, inRuleEnabled.ToString());
            ConfigReader.Set(Constants.Environment.InRuleUrl, inRuleUrl);
            ConfigReader.Set(Constants.Environment.InRuleSubscriptionKey, inRuleSubscriptionKey);
            ConfigReader.Set(Constants.Environment.InRuleScope, inRuleScope);

            return scope;
        }

        public void Dispose()
        {
            ConfigReader.Set(Constants.Environment.InRuleEnabled, _inRuleEnabled ?? string.Empty);
            ConfigReader.Set(Constants.Environment.InRuleUrl, _inRuleUrl ?? string.Empty);
            ConfigReader.Set(Constants.Environment.InRuleSubscriptionKey, _inRuleSubscriptionKey ?? string.Empty);
            ConfigReader.Set(Constants.Environment.InRuleScope, _inRuleScope ?? string.Empty);
        }
    }
}
