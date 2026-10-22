using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;

namespace VAA.EAPI.GraphQL.Common
{
    public sealed class AsyncContext
    {
        public const string APIDiagnosticsHeader = "api-diagnostics";
        public const string APIChannelHeader = "api-channel";
        public const string DefaultChannel = "GRAPHQL";
        public const bool DefaultDiagnosticsEnabled = false;
        public const string APISessionIdHeader = "api-session-id";
        public const string APIOperationIdHeader = "api-operation-id";
        public const string APICorrelationIdHeader = "api-correlation-id";
        public const string OcpApimTraceHeader = "ocp-apim-trace";
        public const string W3CBaggageHeader = "baggage";
        public const string W3cTraceContextHeader = "traceparent";
        public const string W3cTraceStateHeader = "tracestate";
        public const string W3cTraceContextVersion = "00";
        public const string W3cTraceContextTraceFlags = "00";

        private const int MaximumBaggageItems = 64;
        private const int MaximumBaggageLength = 8192;
        private static readonly AsyncLocal<AsyncContext> asyncLocal = new();
        private static readonly Lazy<ISet<string>> baggagePropagationAllowList = new(CreateBaggagePropagationAllowList);

        public static explicit operator AsyncContext(FunctionContext source) => CreateAsync(source).AsTask().GetAwaiter().GetResult();

        public static AsyncContext Current
        {
            get => asyncLocal.Value;
            set => asyncLocal.Value = value;
        }

        public static async ValueTask<AsyncContext> CreateAsync(FunctionContext source)
        {
            var result = new AsyncContext();

            await result.InitializeAsync(source, createActivity: true).ConfigureAwait(false);

            return result;
        }

        private FunctionContext context;

        public bool DiagnosticsEnabled { get; private set; }
        public Guid SessionId { get; private set; }
        public Guid OperationId { get; private set; }
        public Guid CorrelationId { get; private set; }
        public string[] Channel { get; private set; }
        public string W3CBaggage { get; private set; }
        public string W3CTraceParent { get; private set; }
        public string W3CTraceState { get; private set; }
        public string W3CTraceId { get; private set; }
        public string W3CSpanId { get; private set; }
        public ActivityTraceFlags W3CTraceFlags { get; private set; }

        internal Activity ExecutionActivity { get; private set; }

        public FunctionContext Context
        {
            get => context;
            set => InitializeAsync(value, createActivity: false).AsTask().GetAwaiter().GetResult();
        }

        private async ValueTask InitializeAsync(FunctionContext value, bool createActivity)
        {
            context = value;
            DiagnosticsEnabled = ConfigReader.Try(APIDiagnosticsHeader, DefaultDiagnosticsEnabled);

            HttpRequestData request = value is null ? null : await value.GetHttpRequestDataAsync().ConfigureAwait(false);

            DiagnosticsEnabled = ReadBooleanHeader(request, APIDiagnosticsHeader, DiagnosticsEnabled);
            Channel = ReadChannels(request);
            SessionId = ReadGuidHeader(request, APISessionIdHeader);
            OperationId = ReadGuidHeader(request, APIOperationIdHeader);
            CorrelationId = ReadGuidHeader(request, APICorrelationIdHeader);

            var incomingBaggage = ReadHeader(request, W3CBaggageHeader);
            var incomingTraceParent = ReadHeader(request, W3cTraceContextHeader);
            var incomingTraceState = ReadHeader(request, W3cTraceStateHeader);
            var normalizedTraceState = NormalizeTraceState(incomingTraceState);

            Activity activity = Activity.Current;

            if (activity is null && createActivity)
            {
                activity = new Activity(value?.FunctionDefinition?.Name ?? nameof(AsyncContext));
                activity.SetIdFormat(ActivityIdFormat.W3C);

                if (ActivityContext.TryParse(incomingTraceParent, normalizedTraceState, true, out _))
                    activity.SetParentId(incomingTraceParent);

                activity.TraceStateString = normalizedTraceState;

                ExecutionActivity = activity.Start();
            }

            W3CBaggage = MergeBaggage(incomingBaggage, this.BuildW3cBaggage());

            AddBaggageToActivity(activity, W3CBaggage);

            if (activity?.IdFormat == ActivityIdFormat.W3C)
            {
                W3CTraceParent = activity.Id;
                W3CTraceState = NormalizeTraceState(activity.TraceStateString);
                W3CTraceId = activity.TraceId.ToHexString();
                W3CSpanId = activity.SpanId.ToHexString();
                W3CTraceFlags = activity.ActivityTraceFlags;
            }
            else if (ActivityContext.TryParse(incomingTraceParent, normalizedTraceState, true, out ActivityContext parsedContext))
            {
                W3CTraceParent = incomingTraceParent;
                W3CTraceState = normalizedTraceState;
                W3CTraceId = parsedContext.TraceId.ToHexString();
                W3CSpanId = parsedContext.SpanId.ToHexString();
                W3CTraceFlags = parsedContext.TraceFlags;
            }
            else
            {
                W3CTraceParent = this.BuildW3cTraceContext();
                ActivityContext.TryParse(W3CTraceParent, null, false, out parsedContext);
                W3CTraceId = parsedContext.TraceId.ToHexString();
                W3CSpanId = parsedContext.SpanId.ToHexString();
                W3CTraceFlags = parsedContext.TraceFlags;
            }
        }

        public void SetDiagnosticsEnabled(bool value) => DiagnosticsEnabled = value;
        public void SetSessionId(Guid value) => SessionId = value;
        public void SetOperationId(Guid value) => OperationId = value;
        public void SetCorrelationId(Guid value) => CorrelationId = value;
        public void SetChannel(string[] value) => Channel = value;
        public void SetTraceHeaders()
        {
            SetTraceHeaders(preserveRemoteTraceContext: false);
        }

        internal bool HasRemoteTraceContext { get; private set; }

        public void RestoreTraceContext(string traceParent, string traceState, string baggage)
        {
            var propagatedBaggage = FilterBaggage(baggage, GetBaggagePropagationAllowList());
            RestoreEapiContextFromBaggage(propagatedBaggage);
            W3CBaggage = MergeBaggage(propagatedBaggage, this.BuildW3cBaggage());

            var normalizedTraceState = NormalizeTraceState(traceState);
            if (traceParent.IsNullOrWhiteSpace() || !ActivityContext.TryParse(traceParent, normalizedTraceState, true, out var parsedContext))
            {
                AddBaggageToActivity(Activity.Current ?? ExecutionActivity, W3CBaggage);
                return;
            }

            var activity = Activity.Current ?? ExecutionActivity;
            if (ExecutionActivity is not null && ReferenceEquals(activity, ExecutionActivity))
            {
                ExecutionActivity.Stop();

                var executionActivity = new Activity(context?.FunctionDefinition?.Name ?? nameof(AsyncContext));
                executionActivity.SetIdFormat(ActivityIdFormat.W3C);
                executionActivity.SetParentId(traceParent);
                executionActivity.TraceStateString = normalizedTraceState;
                ExecutionActivity = executionActivity.Start();
                activity = ExecutionActivity;
            }

            AddBaggageToActivity(activity, W3CBaggage);

            if (activity?.IdFormat == ActivityIdFormat.W3C && activity.TraceId == parsedContext.TraceId)
            {
                W3CTraceParent = activity.Id;
                W3CTraceState = NormalizeTraceState(activity.TraceStateString);
                W3CTraceId = activity.TraceId.ToHexString();
                W3CSpanId = activity.SpanId.ToHexString();
                W3CTraceFlags = activity.ActivityTraceFlags;
            }
            else
            {
                W3CTraceParent = traceParent;
                W3CTraceState = normalizedTraceState;
                W3CTraceId = parsedContext.TraceId.ToHexString();
                W3CSpanId = parsedContext.SpanId.ToHexString();
                W3CTraceFlags = parsedContext.TraceFlags;
            }

            HasRemoteTraceContext = true;
        }

        public string GetBaggageForPropagation() => FilterBaggage(W3CBaggage, GetBaggagePropagationAllowList());

        internal void SetTraceHeaders(bool preserveRemoteTraceContext)
        {
            W3CBaggage = MergeBaggage(W3CBaggage, this.BuildW3cBaggage());

            if (preserveRemoteTraceContext && HasRemoteTraceContext)
                return;

            var activity = Activity.Current;

            if (activity?.IdFormat == ActivityIdFormat.W3C)
            {
                W3CTraceParent = activity.Id;
                W3CTraceState = NormalizeTraceState(activity.TraceStateString);
                W3CTraceId = activity.TraceId.ToHexString();
                W3CSpanId = activity.SpanId.ToHexString();
                W3CTraceFlags = activity.ActivityTraceFlags;
            }
            else
            {
                W3CTraceParent = this.BuildW3cTraceContext();
                ActivityContext.TryParse(W3CTraceParent, null, false, out var parsedContext);
                W3CTraceState = null;
                W3CTraceId = parsedContext.TraceId.ToHexString();
                W3CSpanId = parsedContext.SpanId.ToHexString();
                W3CTraceFlags = parsedContext.TraceFlags;
            }
        }

        public void Complete() => ExecutionActivity?.Stop();

        public IReadOnlyDictionary<string, string> GetBaggage(IEnumerable<string> allowedKeys = null)
        {
            var allowed = allowedKeys is null ? null : new HashSet<string>(allowedKeys.Where(key => !key.IsNullOrWhiteSpace()), StringComparer.OrdinalIgnoreCase);
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in ParseBaggage(W3CBaggage))
                if ((allowed is null || allowed.Contains(item.Key)) && TryDecodeBaggageValue(item.Value, out var decodedValue))
                    result[item.Key] = decodedValue;

            return result;
        }


        private static bool ReadBooleanHeader(HttpRequestData request, string name, bool defaultValue)
        {
            var value = ReadHeader(request, name);

            return bool.TryParse(value, out var result) ? result : defaultValue;
        }

        private static Guid ReadGuidHeader(HttpRequestData request, string name) => Guid.TryParse(ReadHeader(request, name), out var value) && value != Guid.Empty ? value : Guid.NewGuid();

        private static string[] ReadChannels(HttpRequestData request) => ParseChannels(ReadHeader(request, APIChannelHeader));

        private static string[] ParseChannels(string value) =>
            [.. (value ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                       .Concat([DefaultChannel])
                                       .Distinct(StringComparer.OrdinalIgnoreCase)];

        private static string ReadHeader(HttpRequestData request, string name)
        {
            if (request?.Headers?.TryGetValues(name, out IEnumerable<string> values) != true)
                return null;

            return string.Join(',', values);
        }

        private static string NormalizeTraceState(string value)
        {
            if (value.IsNullOrWhiteSpace() || value.Length > 512)
                return null;

            var members = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var keys = new HashSet<string>(StringComparer.Ordinal);

            if (members.Length is 0 or > 32)
                return null;

            foreach (var member in members)
            {
                var separatorIndex = member.IndexOf('=');
                if (separatorIndex <= 0 || separatorIndex == member.Length - 1)
                    return null;

                var key = member[..separatorIndex];
                var memberValue = member[(separatorIndex + 1)..];
                if (!IsValidTraceStateKey(key) || !IsValidTraceStateValue(memberValue) || !keys.Add(key))
                    return null;
            }

            return string.Join(',', members);
        }

        private static string MergeBaggage(params string[] values)
        {
            var result = new Dictionary<string, BaggageItem>(StringComparer.Ordinal);

            foreach (var item in values.SelectMany(ParseBaggage))
            {
                if (result.Count >= MaximumBaggageItems && !result.ContainsKey(item.Key))
                    break;

                result[item.Key] = item;
            }

            var normalized = string.Join(',', result.Values.Select(item => item.Member));

            return normalized.Length <= MaximumBaggageLength ? normalized : string.Empty;
        }

        private void RestoreEapiContextFromBaggage(string baggage)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in ParseBaggage(baggage))
                if (TryDecodeBaggageValue(item.Value, out var decodedValue))
                    values[item.Key] = decodedValue;

            if (values.TryGetValue(APIDiagnosticsHeader, out var diagnosticsEnabled) && bool.TryParse(diagnosticsEnabled, out var parsedDiagnosticsEnabled))
                DiagnosticsEnabled = parsedDiagnosticsEnabled;

            if (values.TryGetValue(APIChannelHeader, out var channel))
                Channel = ParseChannels(channel);

            if (values.TryGetValue(APISessionIdHeader, out var sessionId) && Guid.TryParse(sessionId, out var parsedSessionId) && parsedSessionId != Guid.Empty)
                SessionId = parsedSessionId;

            if (values.TryGetValue(APIOperationIdHeader, out var operationId) && Guid.TryParse(operationId, out var parsedOperationId) && parsedOperationId != Guid.Empty)
                OperationId = parsedOperationId;

            if (values.TryGetValue(APICorrelationIdHeader, out var correlationId) && Guid.TryParse(correlationId, out var parsedCorrelationId) && parsedCorrelationId != Guid.Empty)
                CorrelationId = parsedCorrelationId;
        }

        private static string FilterBaggage(string baggage, ISet<string> allowedKeys) =>
            string.Join(',', ParseBaggage(baggage)
                .Where(item => allowedKeys.Contains(item.Key) && TryDecodeBaggageValue(item.Value, out _))
                .Select(item => item.Member));

        private static ISet<string> GetBaggagePropagationAllowList() => baggagePropagationAllowList.Value;

        private static ISet<string> CreateBaggagePropagationAllowList()
        {
            var allowedKeys = new HashSet<string>(
            [
                APIDiagnosticsHeader,
                APIChannelHeader,
                APISessionIdHeader,
                APIOperationIdHeader,
                APICorrelationIdHeader,
                OcpApimTraceHeader
            ], StringComparer.OrdinalIgnoreCase);

            allowedKeys.UnionWith(ConfigReader.Try(Constants.Environment.DatadogBaggageAllowList, string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

            return allowedKeys;
        }

        private static IEnumerable<BaggageItem> ParseBaggage(string value)
        {
            if (value.IsNullOrWhiteSpace() || value.Length > MaximumBaggageLength)
                yield break;

            foreach (var member in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var segments = member.Split(';', StringSplitOptions.TrimEntries);
                var pair = segments[0];
                var separatorIndex = pair.IndexOf('=');

                if (separatorIndex <= 0)
                    continue;

                var key = pair[..separatorIndex].Trim();
                var itemValue = pair[(separatorIndex + 1)..].Trim();

                if (IsValidBaggageKey(key) && IsValidBaggageValue(itemValue) && segments.Skip(1).All(IsValidBaggageProperty))
                    yield return new BaggageItem(key, itemValue, string.Join(';', segments));
            }
        }

        private static bool IsValidBaggageKey(string value) =>
            !value.IsNullOrWhiteSpace() && value.All(character => IsAsciiLetterOrDigit(character) || "!#$%&'*+-.^_`|~".Contains(character));

        private static bool IsValidBaggageValue(string value)
        {
            if (value is null)
                return false;

            return value.All(character => character is >= '!' and <= '~' && character is not '"' and not ',' and not ';' and not '\\');
        }

        private static bool IsValidBaggageProperty(string value)
        {
            if (value.IsNullOrWhiteSpace() || value.Contains('\r') || value.Contains('\n'))
                return false;

            var separatorIndex = value.IndexOf('=');

            return separatorIndex < 0 ? IsValidBaggageKey(value) : separatorIndex > 0 && IsValidBaggageKey(value[..separatorIndex]) && IsValidBaggageValue(value[(separatorIndex + 1)..]);
        }

        private static bool IsValidTraceStateKey(string value)
        {
            if (value.IsNullOrWhiteSpace() || value.Length > 256)
                return false;

            var tenantSeparator = value.IndexOf('@');
            if (tenantSeparator < 0)
                return IsAsciiLower(value[0]) && value.Skip(1).All(IsValidTraceStateKeyCharacter);

            if (tenantSeparator is 0 or > 241 || value.LastIndexOf('@') != tenantSeparator)
                return false;

            var tenant = value[..tenantSeparator];
            var system = value[(tenantSeparator + 1)..];
            return system.Length is > 0 and <= 14 &&
                   (IsAsciiLower(tenant[0]) || IsAsciiDigit(tenant[0])) &&
                   tenant.Skip(1).All(IsValidTraceStateKeyCharacter) &&
                   IsAsciiLower(system[0]) &&
                   system.Skip(1).All(IsValidTraceStateKeyCharacter);
        }

        private static bool IsValidTraceStateValue(string value) =>
            !value.IsNullOrWhiteSpace() &&
            value.Length <= 256 &&
            value[^1] is >= '!' and <= '~' and not ',' and not '=' &&
            value.All(character => character is >= ' ' and <= '~' and not ',' and not '=');

        private static bool IsValidTraceStateKeyCharacter(char value) =>
            IsAsciiLower(value) || IsAsciiDigit(value) || "_-*/".Contains(value);

        private static bool IsAsciiLetterOrDigit(char value) =>
            IsAsciiLower(value) || value is >= 'A' and <= 'Z' || IsAsciiDigit(value);

        private static bool IsAsciiLower(char value) => value is >= 'a' and <= 'z';
        private static bool IsAsciiDigit(char value) => value is >= '0' and <= '9';

        private static void AddBaggageToActivity(Activity activity, string baggage)
        {
            if (activity is null)
                return;

            var existingKeys = new HashSet<string>(activity.Baggage.Select(item => item.Key), StringComparer.OrdinalIgnoreCase);

            foreach (var item in ParseBaggage(baggage))
                if (existingKeys.Add(item.Key) && TryDecodeBaggageValue(item.Value, out var decodedValue))
                    activity.AddBaggage(item.Key, decodedValue);
        }

        private static bool TryDecodeBaggageValue(string value, out string decodedValue)
        {
            try
            {
                decodedValue = Uri.UnescapeDataString(value);
                return !decodedValue.Contains('\r') && !decodedValue.Contains('\n');
            }
            catch (ArgumentException)
            {
                decodedValue = null;
                return false;
            }
        }

        private sealed record BaggageItem(string Key, string Value, string Member);
    }
}
