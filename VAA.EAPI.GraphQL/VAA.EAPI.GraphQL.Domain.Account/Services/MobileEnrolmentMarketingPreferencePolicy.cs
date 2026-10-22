using System.Globalization;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Services
{
    public static class MobileEnrolmentMarketingPreferencePolicy
    {
        private const string MobileChannel = "mobile";
        private const string PlatformKey = "platform";
        private const string BuildIdKey = "build-id";
        private const string IosPlatform = "ios";
        private const string AndroidPlatform = "android";
        private const string DisabledPreferenceValue = "N";

        private static readonly AccountPreference.AccountPreferenceTypes[] ProtectedPreferenceTypes =
        [
            AccountPreference.AccountPreferenceTypes.VaaMarketingPermissions,
            AccountPreference.AccountPreferenceTypes.VaPartnersMarketingPermissions,
            AccountPreference.AccountPreferenceTypes.VaGroupMarketingPermissions
        ];

        public static void Apply(CreateCustomerAccountRequest request)
        {
            if (request?.CustomerDetails is null) return;

            var context = AsyncContext.Current;
            if (context is null || HasMalformedChannelValues(context))
            {
                DisableProtectedPreferences(request);
                return;
            }

            if (!context.HasChannel(MobileChannel)) return;

            var platform = context.GetChannelKey(MobileChannel, PlatformKey);
            var buildId = context.GetChannelKey(MobileChannel, BuildIdKey);

            var minimumVersionKey = GetMinimumVersionKey(platform);
            if (minimumVersionKey is null || !TryParseVersion(buildId, out var buildVersion) ||
                !TryParseVersion(ConfigReader.Try(minimumVersionKey), out var minimumVersion) ||
                CompareVersions(buildVersion, minimumVersion) < 0)
            {
                DisableProtectedPreferences(request);
            }
        }

        private static string? GetMinimumVersionKey(string? platform)
        {
            if (platform.Is(IosPlatform)) return Environment.MobileIosMinimumEnrolmentVersion;
            if (platform.Is(AndroidPlatform)) return Environment.MobileAndroidMinimumEnrolmentVersion;

            return null;
        }

        private static void DisableProtectedPreferences(CreateCustomerAccountRequest request)
        {
            var preferences = request.CustomerDetails!.Preferences?.ToList() ?? [];

            foreach (var preferenceType in ProtectedPreferenceTypes)
            {
                var matchingPreferences = preferences.Where(preference => preference?.PreferenceType == preferenceType).ToList();
                if (matchingPreferences.Count == 0)
                {
                    preferences.Add(new AccountPreference { PreferenceType = preferenceType, PreferenceValue = DisabledPreferenceValue });
                    continue;
                }

                foreach (var preference in matchingPreferences)
                    preference.PreferenceValue = DisabledPreferenceValue;
            }

            request.CustomerDetails.Preferences = preferences;
        }

        private static bool TryParseVersion(string? value, out int[] components)
        {
            components = [];
            if (value.IsNullOrWhiteSpace() || char.IsWhiteSpace(value[0]) || char.IsWhiteSpace(value[^1])) return false;

            var version = value;
            var buildStart = version.IndexOf('(');
            int? buildComponent = null;
            if (buildStart >= 0)
            {
                if (!version.EndsWith(')') || version.IndexOf('(', buildStart + 1) >= 0 ||
                    !TryParseVersionComponent(version[(buildStart + 1)..^1], out var parsedBuildComponent))
                    return false;

                buildComponent = parsedBuildComponent;
                version = version[..buildStart];
            }

            var parts = version.Split('.', StringSplitOptions.None);
            if (parts.Length is < 2 or > 4) return false;

            components = new int[parts.Length + (buildComponent.HasValue ? 1 : 0)];
            for (var index = 0; index < parts.Length; index++)
            {
                if (!TryParseVersionComponent(parts[index], out components[index]))
                    return false;
            }

            if (buildComponent.HasValue)
                components[^1] = buildComponent.Value;

            return true;
        }

        private static bool TryParseVersionComponent(string value, out int component)
        {
            component = 0;

            return value.Length > 0 &&
                   value.All(char.IsAsciiDigit) &&
                   int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out component);
        }

        private static bool HasMalformedChannelValues(AsyncContext context) =>
            context.Channel is null || context.Channel.Length == 0 || context.Channel.Any(channel =>
                channel.IsNullOrWhiteSpace() ||
                channel.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length == 0);

        private static int CompareVersions(IReadOnlyList<int> left, IReadOnlyList<int> right)
        {
            var componentCount = Math.Max(left.Count, right.Count);
            for (var index = 0; index < componentCount; index++)
            {
                var comparison = (index < left.Count ? left[index] : 0).CompareTo(index < right.Count ? right[index] : 0);
                if (comparison != 0) return comparison;
            }

            return 0;
        }
    }
}
