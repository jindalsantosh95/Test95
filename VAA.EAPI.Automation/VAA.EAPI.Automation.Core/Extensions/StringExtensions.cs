using System.Collections.Concurrent;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO.Compression;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Serialisation;
using VAA.EAPI.Automation.Core.Text;

namespace VAA.EAPI.Automation.Core.Extensions
{
    public static class StringExtensions
    {
        public const char ColonChar = ':';
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
            Type targetType = Nullable.GetUnderlyingType(type) ?? type;

            return _cache.GetOrAdd(targetType, t =>
            {
                MethodInfo[] methods = t.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);

                // TryParse(string, out T)
                MethodInfo tryParse = methods.FirstOrDefault(m =>
                {
                    if (!m.Name.Is(TryParseName)) return false;

                    ParameterInfo[] parameters = m.GetParameters();

                    if (parameters.Length != 2) return false;
                    if (parameters[0].ParameterType != typeof(string)) return false;
                    if (!parameters[1].IsOut || !parameters[1].ParameterType.IsByRef) return false;
                    if (parameters[1].ParameterType.GetElementType() != t) return false;

                    return true;
                });

                // TryParse(string, IFormatProvider, out T)
                MethodInfo tryParseWithProvider = methods.FirstOrDefault(m =>
                {
                    if (!m.Name.Is(TryParseName)) return false;

                    ParameterInfo[] parameters = m.GetParameters();

                    if (parameters.Length != 3) return false;
                    if (parameters[0].ParameterType != typeof(string)) return false;
                    if (!typeof(IFormatProvider).IsAssignableFrom(parameters[1].ParameterType)) return false;
                    if (!parameters[2].IsOut || !parameters[2].ParameterType.IsByRef) return false;
                    if (parameters[2].ParameterType.GetElementType() != t) return false;

                    return true;
                });

                // Parse(string)
                MethodInfo parse = methods.FirstOrDefault(m =>
                {
                    if (!m.Name.Is(ParseName)) return false;

                    ParameterInfo[] parameters = m.GetParameters();

                    if (parameters.Length != 1) return false;
                    if (parameters[0].ParameterType != typeof(string)) return false;

                    return true;
                });

                // Parse(string, IFormatProvider)
                MethodInfo parseWithProvider = methods.FirstOrDefault(m =>
                {
                    if (!m.Name.Is(ParseName))
                        return false;

                    ParameterInfo[] parameters = m.GetParameters();

                    if (parameters.Length != 2) return false;
                    if (parameters[0].ParameterType != typeof(string)) return false;
                    if (!typeof(IFormatProvider).IsAssignableFrom(parameters[1].ParameterType)) return false;

                    return true;
                });

                Func<string, (bool, object?)> tryParseDelegate = CreateCompiledDelegate(tryParse, targetType);
                Func<string, System.IFormatProvider?, (bool, object?)> tryParseWithProviderDelegate = CreateCompiledDelegateWithProvider(tryParseWithProvider, targetType);
                Func<string, (bool, object?)> parseDelegate = CreateParseDelegate(parse);
                Func<string, System.IFormatProvider?, (bool, object?)> parseWithProviderDelegate = CreateParseDelegateWithProvider(parseWithProvider);

                return (tryParseDelegate, tryParseWithProviderDelegate, parseDelegate, parseWithProviderDelegate);
            });
        }

        private static Func<string, (bool, object?)> CreateCompiledDelegate(MethodInfo method, Type targetType)
        {
            if (method is null) return default;

            ParameterExpression inputParam = Expression.Parameter(typeof(string), "input");
            ParameterExpression resultVar = Expression.Variable(targetType, "result");

            Expression expression = Expression.Call(method, inputParam, resultVar);

            NewExpression successResult = Expression.New(typeof(ValueTuple<bool, object?>).GetConstructor([typeof(bool), typeof(object)])!, Expression.Constant(true), Expression.Convert(resultVar, typeof(object)));
            NewExpression failureResult = Expression.New(typeof(ValueTuple<bool, object?>).GetConstructor([typeof(bool), typeof(object)])!, Expression.Constant(false), Expression.Constant(null, typeof(object)));
            BlockExpression block = Expression.Block([resultVar], Expression.Condition(expression, successResult, failureResult));

            return Expression.Lambda<Func<string, (bool, object?)>>(block, inputParam).Compile();
        }

        private static Func<string, IFormatProvider?, (bool, object?)> CreateCompiledDelegateWithProvider(MethodInfo method, Type targetType)
        {
            if (method is null) return default;

            ParameterExpression inputParam = Expression.Parameter(typeof(string), "input");
            ParameterExpression providerParam = Expression.Parameter(typeof(IFormatProvider), "provider");
            ParameterExpression resultVar = Expression.Variable(targetType, "result");

            Expression expression = Expression.Call(method, inputParam, providerParam, resultVar);

            NewExpression successResult = Expression.New(typeof(ValueTuple<bool, object?>).GetConstructor([typeof(bool), typeof(object)])!, Expression.Constant(true), Expression.Convert(resultVar, typeof(object)));
            NewExpression failureResult = Expression.New(typeof(ValueTuple<bool, object?>).GetConstructor([typeof(bool), typeof(object)])!, Expression.Constant(false), Expression.Constant(null, typeof(object)));
            BlockExpression block = Expression.Block([resultVar], Expression.Condition(expression, successResult, failureResult));

            return Expression.Lambda<Func<string, IFormatProvider?, (bool, object?)>>(block, inputParam, providerParam).Compile();
        }

        // Parse(string) / Parse(string, IFormatProvider) return the parsed value directly
        // and throw on failure - unlike TryParse, they have no 'out' parameter. The caller
        // wraps the invocation in try/catch, so a throw is treated as "no match".
        // Returns null when 'method' is null (the type exposes no such Parse method).
        private static Func<string, (bool, object?)>? CreateParseDelegate(MethodInfo? method)
        {
            if (method is null) return default;

            ParameterExpression inputParam = Expression.Parameter(typeof(string), "input");

            Expression call = Expression.Call(method, inputParam);

            NewExpression result = Expression.New(typeof(ValueTuple<bool, object?>).GetConstructor([typeof(bool), typeof(object)])!, Expression.Constant(true), Expression.Convert(call, typeof(object)));

            return Expression.Lambda<Func<string, (bool, object?)>>(result, inputParam).Compile();
        }

        private static Func<string, IFormatProvider?, (bool, object?)>? CreateParseDelegateWithProvider(MethodInfo? method)
        {
            if (method is null) return default;

            ParameterExpression inputParam = Expression.Parameter(typeof(string), "input");
            ParameterExpression providerParam = Expression.Parameter(typeof(IFormatProvider), "provider");

            Expression call = Expression.Call(method, inputParam, providerParam);

            NewExpression result = Expression.New(typeof(ValueTuple<bool, object?>).GetConstructor([typeof(bool), typeof(object)])!, Expression.Constant(true), Expression.Convert(call, typeof(object)));

            return Expression.Lambda<Func<string, IFormatProvider?, (bool, object?)>>(result, inputParam, providerParam).Compile();
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

            if (source is null)
                return false;

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
                if (Uri.TryCreate(source, UriKind.RelativeOrAbsolute, out System.Uri? uriValue))
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
                    MethodInfo tryMethod = typeof(StringExtensions).GetMethod(nameof(TryIParsable), BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(targetType);

                    object[] args = { source, provider, null! };

                    if ((bool)tryMethod.Invoke(null, args)!)
                    {
                        result = (T)args[2]!;

                        return true;
                    }
                }

            (Func<string, (bool, object?)> tryParse, Func<string, IFormatProvider?, (bool, object?)> tryParseWithProvider, Func<string, (bool, object?)> parse, Func<string, IFormatProvider?, (bool, object?)> parseWithProvider) = CacheDelegates(targetType);

            if (tryParse is not null)
            {
                (bool success, object? parsed) = tryParse(source);

                if (success && parsed is T typedResult)
                {
                    result = typedResult;

                    return true;
                }
            }

            if (tryParseWithProvider is not null)
            {
                (bool success, object? parsed) = tryParseWithProvider(source, provider);

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
                    (bool success, object? parsed) = parse(source);

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
                    (bool success, object? parsed) = parseWithProvider(source, provider);

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

        public static bool Has(this string source, string value)
        {
            if (source.Is(value))
                return true;

            if (source is null && value is not null) return false;
            if (source is not null && value is null) return false;

            if (source.Contains(value)) return true;

            return source.Contains(value, StringComparison.OrdinalIgnoreCase);
        }

        public static bool HasAny(this string source, params string[] values)
        {
            values ??= [];

            return values.Any(x => source.Has(x));
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
                JwtSecurityToken jwtSecurityToken = jwtSecurityTokenHandler.ReadJwtToken(source);

                Claim createdAt = jwtSecurityToken.Claims.FirstOrDefault(x => x.Type.Is("created_at"));
                Claim expiresIn = jwtSecurityToken.Claims.FirstOrDefault(x => x.Type.Is("expires_in"));

                if (createdAt is not null && expiresIn is not null)
                {
                    DateTimeOffset expiresAt = DateTimeOffset.FromUnixTimeMilliseconds(createdAt.Value.Parse<long>() + expiresIn.Value.Parse<long>());

                    return expiresAt.UtcDateTime > DateTime.UtcNow;
                }

                return jwtSecurityTokenHandler.ValidateToken(source, tokenValidationParameters, out Microsoft.IdentityModel.Tokens.SecurityToken validatedToken) != null && validatedToken != null;
            }
            catch
            {
                return false;
            }
        }

        public static string[] ExtractJwtClaims(this string source, params string[] claims)
        {
            if (source.IsNullOrWhiteSpace()) return [];

            try
            {
                JwtSecurityToken jwtSecurityToken = jwtSecurityTokenHandler.ReadJwtToken(source);

                return jwtSecurityToken.Claims.Where(x => claims.Any(c => c.Is(x.Type))).Select(x => x.Value).ToArray();
            }
            catch
            {
                return [];
            }
        }

        public static byte[] Compress(this string source)
        {
            if (source.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(source));

            using MemoryStream memoryStream = new MemoryStream();

            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(source);

            // Dispose the GZipStream before reading the buffer so the gzip footer
            // (final block + CRC/length trailer) is written; leaveOpen keeps the
            // MemoryStream open for ToArray().
            using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true))
            {
                gZipStream.Write(buffer, 0, buffer.Length);
            }

            return memoryStream.ToArray();
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

        public static string ReplaceIf(this string source, bool condition, string oldValue, string newValue) => condition ? source.Replace(oldValue, newValue) : source;

        public static string IfIsNullOrWhiteSpace(this string source, string value) => source.IsNullOrWhiteSpace() ? value : source;

        public static MultipartFormDataContent ToMultipartFormDataContent(this string source)
        {
            MultipartFormDataContent multipartFormDataContent = new();

            foreach (string line in source.Trim().Split(System.Environment.NewLine))
            {
                string name = line.Substring(0, line.IndexOf(ColonChar));
                string value = line.Substring(line.IndexOf(ColonChar) + 1);

                multipartFormDataContent.Add(new StringContent(value), name);
            }

            return multipartFormDataContent;
        }

        public static FormUrlEncodedContent ToFormUrlEncodedContent(this string source)
        {
            IEnumerable<KeyValuePair<string, string>> nameValueCollection = source.Trim()
                                                                                  .Split(Environment.NewLine)
                                                                                  .Select(line =>
                                                                                  {
                                                                                      string name = line.Substring(0, line.IndexOf(ColonChar));
                                                                                      string value = line.Substring(line.IndexOf(ColonChar) + 1);

                                                                                      return new KeyValuePair<string, string>(name, value);
                                                                                  });

            return new(nameValueCollection);
        }

        public static string GetAny(this string[] source)
        {
            Random random = new Random();

            return source[random.Next(source.Length)];
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

        public static bool ContainsCsvValue(this string source, string value)
        {
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(value)) return false;

            string trimmedValue = value.Trim();

            return source.Split(',', StringSplitOptions.RemoveEmptyEntries)
                         .Any(x => x.Trim().Equals(trimmedValue, StringComparison.OrdinalIgnoreCase));
        }
    }
}
