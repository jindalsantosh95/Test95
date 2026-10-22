using System.Text;

namespace VAA.EAPI.Automation.Core.Extensions
{
    public static class StringEnumerableExtensions
    {
        public static bool AllArgumentsExist(this IEnumerable<string> source, params string[] names) => source.AllArgumentsExist(out _, names);
        public static bool AllArgumentsExist(this IEnumerable<string> source, out string message, params string[] names)
        {
            message = string.Empty;

            StringBuilder stringBuilder = new StringBuilder();

            bool all = names.All(arg =>
            {
                bool exists = source.ArgumentExists(arg, out string singleMessage);

                if (!exists)
                    stringBuilder.AppendLine(singleMessage);

                return exists;
            });

            message = stringBuilder.ToString();

            return all;
        }

        public static bool AnyArgumentExists(this IEnumerable<string> source, params string[] names) => source.AnyArgumentExists(out _, names);
        public static bool AnyArgumentExists(this IEnumerable<string> source, out string message, params string[] names)
        {
            message = string.Empty;

            StringBuilder stringBuilder = new StringBuilder();

            bool any = names.Any(arg =>
            {
                bool exists = source.ArgumentExists(arg, out string singleMessage);

                if (!exists)
                    stringBuilder.AppendLine(singleMessage);

                return exists;
            });

            message = stringBuilder.ToString();

            return any;
        }

        public static bool ArgumentExists(this IEnumerable<string> source, string name) => source.ArgumentExists(name, out _);
        public static bool ArgumentExists(this IEnumerable<string> source, string name, out string message)
        {
            message = string.Empty;

            if (!(source?.Any() ?? false))
                return false;

            name = name.ToLower().Trim('-', ':');

            if (!source.Any(arg => arg.ToLower().StartsWith($"-{name}:")))
            {
                message = "";
                return false;
            }

            return true;
        }

        public static string Argument(this IEnumerable<string> source, string name) => source.Argument(name, string.Empty);
        public static T Argument<T>(this IEnumerable<string> source, string name, T @default = default)
        {
            @default ??= default;

            if (!(source?.Any() ?? false))
                return @default;
            if (!source.ArgumentExists(name))
                return @default;

            name = name.ToLower().Trim('-', ':');

            string argument = source.First(x => x.ToLower().StartsWith($"-{name}:"));

            if (argument.Substring(argument.IndexOf(':') + 1).TryParse(out T value))
                return value;

            return @default;
        }

        public static bool IsNullOrEmpty(this IEnumerable<string> source) => source is null || !source.Any();
    }
}
