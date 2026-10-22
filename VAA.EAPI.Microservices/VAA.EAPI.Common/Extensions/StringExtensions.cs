using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;


namespace VAA.EAPI.Common.Extensions
{
    public static class StringExtensions
    {
        private const string TryParseName = "TryParse";
        private const string ParseName = "Parse";
        private const string IParsableName = "IParsable";

        private static readonly JwtSecurityTokenHandler jwtSecurityTokenHandler = new();
        private static readonly TokenValidationParameters tokenValidationParameters = new()
        {
            ValidateAudience = false,
            RequireAudience = false,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = false,
            RequireSignedTokens = false,
            SignatureValidator = (string token, TokenValidationParameters parameters) => new JwtSecurityToken(token),
            ClockSkew = TimeSpan.Zero
        };
        private static readonly ConcurrentDictionary<Type, (Func<string, (bool, object?)>? TryParse, Func<string, IFormatProvider?, (bool, object?)>? TryParseWithProvider, Func<string, (bool, object?)>? Parse, Func<string, IFormatProvider?, (bool, object?)>? ParseWithProvider)> _cache = [];

        private static (Func<string, (bool, object?)>? TryParse, Func<string, IFormatProvider?, (bool, object?)>? TryParseWithProvider, Func<string, (bool, object?)>? Parse, Func<string, IFormatProvider?, (bool, object?)>? ParseWithProvider) CacheDelegates(Type type)
        {
            var targetType = Nullable.GetUnderlyingType(type) ?? type;

            return _cache.GetOrAdd(targetType, t =>
            {
                var methods = t.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);

                // TryParse(string, out T)
                var tryParse = methods.FirstOrDefault(m =>
                                       {
                                           if (!m.Name.Is(TryParseName)) return false;

                                           var parameters = m.GetParameters();

                                           if (parameters.Length != 2) return false;
                                           if (parameters[0].ParameterType != typeof(string)) return false;
                                           if (!parameters[1].IsOut || !parameters[1].ParameterType.IsByRef) return false;
                                           if (parameters[1].ParameterType.GetElementType() != t) return false;

                                           return true;
                                       });

                // TryParse(string, IFormatProvider, out T)
                var tryParseWithProvider = methods.FirstOrDefault(m =>
                                                   {
                                                       if (!m.Name.Is(TryParseName)) return false;

                                                       var parameters = m.GetParameters();

                                                       if (parameters.Length != 3) return false;
                                                       if (parameters[0].ParameterType != typeof(string)) return false;
                                                       if (!typeof(IFormatProvider).IsAssignableFrom(parameters[1].ParameterType)) return false;
                                                       if (!parameters[2].IsOut || !parameters[2].ParameterType.IsByRef) return false;
                                                       if (parameters[2].ParameterType.GetElementType() != t) return false;

                                                       return true;
                                                   });

                // Parse(string)
                var parse = methods.FirstOrDefault(m =>
                                    {
                                        if (!m.Name.Is(ParseName)) return false;

                                        var parameters = m.GetParameters();

                                        if (parameters.Length != 1) return false;
                                        if (parameters[0].ParameterType != typeof(string)) return false;

                                        return true;
                                    });

                // Parse(string, IFormatProvider)
                var parseWithProvider = methods.FirstOrDefault(m =>
                                                {
                                                    if (!m.Name.Is(ParseName)) return false;

                                                    var parameters = m.GetParameters();

                                                    if (parameters.Length != 2) return false;
                                                    if (parameters[0].ParameterType != typeof(string)) return false;
                                                    if (!typeof(IFormatProvider).IsAssignableFrom(parameters[1].ParameterType)) return false;

                                                    return true;
                                                });

                var tryParseDelegate = CreateCompiledDelegate(tryParse, targetType);
                var tryParseWithProviderDelegate = CreateCompiledDelegateWithProvider(tryParseWithProvider, targetType);
                var parseDelegate = CreateCompiledDelegate(parse, targetType);
                var parseWithProviderDelegate = CreateCompiledDelegateWithProvider(parseWithProvider, targetType);

                return (tryParseDelegate, tryParseWithProviderDelegate, parseDelegate, parseWithProviderDelegate);
            });
        }
        private static Func<string, (bool, object?)> CreateCompiledDelegate(MethodInfo method, Type targetType)
        {
            if (method is null) return default;

            var inputParam = Expression.Parameter(typeof(string), "input");
            var resultVar = Expression.Variable(targetType, "result");

            Expression expression = Expression.Call(method, inputParam, resultVar);

            var successResult = Expression.New(typeof(ValueTuple<bool, object?>).GetConstructor([typeof(bool), typeof(object)])!, Expression.Constant(true), Expression.Convert(resultVar, typeof(object)));
            var failureResult = Expression.New(typeof(ValueTuple<bool, object?>).GetConstructor([typeof(bool), typeof(object)])!, Expression.Constant(false), Expression.Constant(null, typeof(object)));
            var block = Expression.Block([resultVar], Expression.Condition(expression, successResult, failureResult));

            return Expression.Lambda<Func<string, (bool, object?)>>(block, inputParam).Compile();
        }
        private static Func<string, IFormatProvider?, (bool, object?)> CreateCompiledDelegateWithProvider(MethodInfo method, Type targetType)
        {
            if (method is null) return default;

            var inputParam = Expression.Parameter(typeof(string), "input");
            var providerParam = Expression.Parameter(typeof(IFormatProvider), "provider");
            var resultVar = Expression.Variable(targetType, "result");

            Expression expression = Expression.Call(method, inputParam, providerParam, resultVar);

            var successResult = Expression.New(typeof(ValueTuple<bool, object?>).GetConstructor([typeof(bool), typeof(object)])!, Expression.Constant(true), Expression.Convert(resultVar, typeof(object)));
            var failureResult = Expression.New(typeof(ValueTuple<bool, object?>).GetConstructor([typeof(bool), typeof(object)])!, Expression.Constant(false), Expression.Constant(null, typeof(object)));
            var block = Expression.Block([resultVar], Expression.Condition(expression, successResult, failureResult));

            return Expression.Lambda<Func<string, IFormatProvider?, (bool, object?)>>(block, inputParam, providerParam).Compile();
        }
        private static bool TryIParsable<T>(string s, IFormatProvider? provider, out T result) where T : IParsable<T> => T.TryParse(s, provider, out result);

        public static T Parse<T>(this string source, IFormatProvider? provider = default)
        {
            if (!source.TryParse(out T result, default, provider)) throw new FormatException($"Unable to parse '{source}' to type {typeof(T).FullName}.");

            return result;
        }
        public static T Parse<T>(this string source, T @default, IFormatProvider? provider = default)
        {
            _ = source.TryParse(out T result, @default, provider);

            return result;
        }

        public static bool TryParse<T>(this string source, out T result, T @default = default, IFormatProvider? provider = default)
        {
            result = @default;
            provider ??= CultureInfo.CurrentCulture;

            if (source is null) return false;

            source = source.Trim();

            Type targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            if (targetType == typeof(string))
            {
                result = (T)(object)source;

                return true;
            }

            if (targetType.IsEnum)
            {
                if (Enum.TryParse(targetType, source, true, out object enumValue))
                {
                    result = (T)enumValue;

                    return true;
                }

                return false;
            }

            if (targetType == typeof(bool))
            {
                result = (T)(object)(bool.TryParse(source, out bool boolValue) ? boolValue : source.IsAny("y", "yes", "true", "1", "on"));

                return true;
            }

            if (targetType == typeof(Uri))
            {
                if (Uri.TryCreate(source, UriKind.RelativeOrAbsolute, out var uriValue))
                {
                    result = (T)(object)uriValue;

                    return true;
                }

                return false;
            }

            if (targetType == typeof(JObject))
            {
                result = (T)(object)JObject.Parse(source);

                return true;
            }

            if (((TypeInfo)targetType).ImplementedInterfaces.Any(x => x.Name.StartsWith(IParsableName)))
                if (typeof(IParsable<>).MakeGenericType(targetType).IsAssignableFrom(targetType))
                {
                    var tryMethod = typeof(StringExtensions).GetMethod(nameof(TryIParsable), BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(targetType);

                    object[] args = { source, provider, null! };

                    if ((bool)tryMethod.Invoke(null, args)!)
                    {
                        result = (T)args[2]!;

                        return true;
                    }
                }

            var (tryParse, tryParseWithProvider, parse, parseWithProvider) = CacheDelegates(targetType);

            if (tryParse is not null)
            {
                var (success, parsed) = tryParse(source);

                if (success && parsed is T typedResult)
                {
                    result = typedResult;

                    return true;
                }
            }

            if (tryParseWithProvider is not null)
            {
                var (success, parsed) = tryParseWithProvider(source, provider);

                if (success && parsed is T typedResult)
                {
                    result = typedResult;

                    return true;
                }
            }

            if (parse is not null)
            {
                try
                {
                    var (success, parsed) = parse(source);

                    if (success && parsed is T typedResult)
                    {
                        result = typedResult;

                        return true;
                    }
                }
                catch
                {
                    // suppress because we are trying to parse
                }
            }

            if (parseWithProvider is not null)
            {
                try
                {
                    var (success, parsed) = parseWithProvider(source, provider);

                    if (success && parsed is T typedResult)
                    {
                        result = typedResult;

                        return true;
                    }
                }
                catch
                {
                    // suppress because we are trying to parse
                }
            }

            return false;
        }

        public static bool Is(this char source, char value)
        {
            if (source == value) return true;

            if (source is char.MinValue && value is not char.MinValue) return false;
            if (source is not char.MinValue && value is char.MinValue) return false;

            unsafe
            {
                var first = new string(&source);
                var second = new string(&value);

                return first.Is(second);
            }
        }
        public static bool Is(this string source, string value)
        {
            if (source == value) return true;

            if (source is null && value is not null) return false;
            if (source is not null && value is null) return false;

            return source.Equals(value, StringComparison.OrdinalIgnoreCase);
        }
        public static bool IsAny(this string source, params string[] values)
        {
            if (source is null && values is null) return true;
            if (source is not null && (values is null || !values.Any())) return false;

            return values.Any(x => x.Is(source));
        }
        public static bool IsNullOrEmpty(this string source) => string.IsNullOrEmpty(source);
        public static bool IsNullOrWhiteSpace(this string source) => string.IsNullOrWhiteSpace(source);
        public static bool IsNullOrWhiteSpaceOr(this string source, string value) => source.IsNullOrWhiteSpace() || source.Is(value);

        public static string IfIsNullOrWhiteSpace(this string source, string @default) => string.IsNullOrWhiteSpace(source) ? @default : source;

        public static bool ContainsAny(this string source, params string[] values)
        {
            if (source is null && values is null) return true;
            if (source is not null && (values is null || !values.Any())) return false;

            return values.Any(x => source.Contains(x));
        }

        public static bool IsJwtTokenValid(this string source)
        {
            if (source.IsNullOrWhiteSpace()) return false;

            try
            {
                var jwtSecurityToken = jwtSecurityTokenHandler.ReadJwtToken(source);

                var createdAt = jwtSecurityToken.Claims.FirstOrDefault(x => x.Type.Is("created_at"));
                var expiresIn = jwtSecurityToken.Claims.FirstOrDefault(x => x.Type.Is("expires_in"));

                if (createdAt is not null && expiresIn is not null)
                {
                    var expiresAt = DateTimeOffset.FromUnixTimeMilliseconds(createdAt.Value.Parse<long>() + expiresIn.Value.Parse<long>());

                    return expiresAt.UtcDateTime > DateTime.UtcNow;
                }

                return jwtSecurityTokenHandler.ValidateToken(source, tokenValidationParameters, out var validatedToken) != null && validatedToken != null;
            }
            catch
            {
                return false;
            }
        }

        public static string ExtractJwtClaim(this string? source, string claimType) => jwtSecurityTokenHandler.ReadJwtToken(source).Claims.FirstOrDefault(c => c.Type.Is(claimType))?.Value;

        public static byte[] Compress(this string source)
        {
            if (source.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(source));

            using var memoryStream = new MemoryStream();
            using var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress);

            var buffer = System.Text.Encoding.UTF8.GetBytes(source);

            gZipStream.Write(buffer, 0, buffer.Length);

            gZipStream.Flush();

            return memoryStream.ToArray();
        }
        /// <summary>
        /// Decompresses a based 64 compressed string 
        /// </summary>
        /// <param name="source">Base 64 compressed string</param>
        /// <returns>Decompressed raw string</returns>
        /// <exception cref="ArgumentNullException">source</exception>
        public static string DecompressToString(this string source)
        {
            if (source.IsNullOrEmpty()) throw new ArgumentNullException(nameof(source));

            return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(source).Decompress());
        }
        /// <summary>
        /// De-serializes a based 64 compressed string to the target generic type
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="source">Base 64 compressed string</param>
        /// <returns>Decompressed raw string</returns>
        /// <exception cref="ArgumentNullException">source</exception>
        public static T DecompressTo<T>(this string source)
        {
            if (source.IsNullOrEmpty()) throw new ArgumentNullException(nameof(source));

            return Json.Deserialize<T>(source.DecompressToString());
        }

        public static string SeparateUpperCamelCase(this string source) => RegexCompiled.Replace(source, "(?!^)([A-Z])([a-z0-9]+)", " $1$2", RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant);

        public static string MakeInterpolationSafe(this string source) => source?.Replace("{", "{{").Replace("}", "}}");

        public static string ToTitleCase(this string source) => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(source.ToLower());

        public static int GetStableHashCode(this string source)
        {
            unchecked
            {
                int hash1 = 5381;
                int hash2 = hash1;

                for (int i = 0; i < source.Length && source[i] != '\0'; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ source[i];

                    if (i == source.Length - 1 || source[i + 1] == '\0')
                        break;

                    hash2 = ((hash2 << 5) + hash2) ^ source[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }

        public static string ExtractQRData(this string source)
        {
            if (source.IsNullOrWhiteSpace()) return null;
            if (!System.Buffers.Text.Base64.IsValid(source)) return null;

            var bytes = Convert.FromBase64String(source);

            try
            {
                using var bitmap = (Bitmap)Bitmap.FromStream(new MemoryStream(bytes) { Position = 0 });

                return new ZXing.Windows.Compatibility.BarcodeReader().Decode(bitmap)?.Text;
            }
            catch
            {
                return null;
            }
        }
    }
}