using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO.Compression;
using System.Reflection;
using System.Text.RegularExpressions;
using VAA.EAPI.Web.Common.Helper;

namespace VAA.EAPI.Web.Common.Extensions
{
    public static class StringExtensions
    {
        private static readonly object reflectionLock = new();
        private static readonly ConcurrentDictionary<string, MethodInfo> methodInfos = new();
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
            SignatureValidator = (token, parameters) => new JwtSecurityToken(token),
            ClockSkew = TimeSpan.Zero
        };

        private static MethodInfo CacheMethodInfo<T>(string name, Func<ParameterInfo[], bool> prameterCheck)
        {
            Type targetType = typeof(T);

            string typeKey = $"{targetType.FullName}+{name}";

            if (!methodInfos.ContainsKey(typeKey))
                lock (reflectionLock)
                    if (!methodInfos.ContainsKey(typeKey))
                    {
                        var methodInfo = targetType.GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && prameterCheck(m.GetParameters()));

                        if (methodInfo != null)
                            while (!methodInfos.ContainsKey(typeKey) && !methodInfos.TryAdd(typeKey, methodInfo))
                                if (!Thread.Yield())
                                    Thread.Sleep(0);
                    }

            return methodInfos.ContainsKey(typeKey) ? methodInfos[typeKey] : null;
        }

        public static T Parse<T>(this string source)
        {
            if (!source.TryParse(out T result)) throw new FormatException($"Unable to parse '{source}' to type {nameof(T)}.");

            return result;
        }
        public static T Parse<T>(this string source, T @default)
        {
            _ = source.TryParse(out T result, @default);

            return result;
        }

        public static bool TryParse<T>(this string source, out T result, T @default = default)
        {
            result = @default;

            if (source is null) return false;

            if (typeof(T) == typeof(string))
            {
                result = (T)(object)source;

                return true;
            }

            if (typeof(T).IsEnum)
            {
                if (Enum.TryParse(typeof(T), source, true, out object enumValue))
                {
                    result = (T)enumValue;

                    return true;
                }

                return false;
            }

            if (typeof(T) == typeof(bool))
            {
                result = (T)(object)(bool.TryParse(source, out bool boolValue) ? boolValue : source.Equals("Y", StringComparison.OrdinalIgnoreCase));

                return true;
            }

            if (typeof(T) == typeof(Uri))
            {
                try
                {
                    result = (T)(object)new Uri(source.ToString());

                    return true;
                }
                catch
                {
                    return false;
                }
            }

            var tryParseMethodInfo = CacheMethodInfo<T>("TryParse", (parameters) => parameters[0].ParameterType == typeof(string));
            var parseMethodInfo = CacheMethodInfo<T>("Parse", (parameters) => parameters[0].ParameterType == typeof(string));

            bool parsed = false;
            Type targetType = typeof(T);

            if (tryParseMethodInfo != null)
            {
                object[] args = new object[] { source, default(T) };

                parsed = (bool)tryParseMethodInfo.Invoke(targetType, args);

                result = parsed ? (T)args[1] : default;
            }

            if (!parsed && parseMethodInfo != null)
            {
                try
                {
                    result = (T)parseMethodInfo.Invoke(targetType, new object[] { source });

                    parsed = true;
                }
                catch
                {
                    // supress because we are trying to parse
                }
            }

            return parsed;
        }

        public static bool Is(this string source, string value)
        {
            if (source == value) return true;

            if (source is null && value is not null) return false;
            if (source is not null && value is null) return false;

            return source.Equals(value, StringComparison.OrdinalIgnoreCase);
        }
        public static bool IsNullOrEmpty(this string source) => string.IsNullOrEmpty(source);
        public static bool IsNullOrWhiteSpace(this string source) => string.IsNullOrWhiteSpace(source);

        public static string SeperateUpperCamelCase(this string source) => RegexCompiled.Replace(source, "(?!^)([A-Z])([a-z0-9]+)", " $1$2", RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant);


        public static bool IsJwtTokenValid(this string source)
        {
            if (string.IsNullOrWhiteSpace(source)) return false;
            try
            {
                return jwtSecurityTokenHandler.ValidateToken(source, tokenValidationParameters, out var validatedToken) != null && validatedToken != null;

            }
            catch
            {
                return false;
            }
        }

        public static byte[] Compress(this string source)
        {
            if (source.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(source));

            using var sourceStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(source)) { Position = 0 };
            using var memoryStream = new MemoryStream();
            using var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress);

            sourceStream.CopyTo(gZipStream);

            gZipStream.Flush();

            return memoryStream.ToArray();
        }

        public static string ToTitleCase(this string source) => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(source.ToLower());

    }
}
