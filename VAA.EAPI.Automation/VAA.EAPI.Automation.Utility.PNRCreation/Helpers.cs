using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.Utility.PNRCreation
{
    public static class Helpers
    {
        public static T Ask<T>(string prompt, bool required = true, T @default = default)
        {
            ArgumentNullException.ThrowIfNull(prompt, nameof(prompt));

            prompt = prompt.Trim().TrimEnd(':');

            if (string.IsNullOrWhiteSpace(prompt)) throw new ArgumentNullException(nameof(prompt));

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{prompt}: ");
            Console.ForegroundColor = ConsoleColor.Yellow;

            T value = @default;
            var (Left, Top) = Console.GetCursorPosition();
            string input = Console.ReadLine()?.Trim() ?? string.Empty;

            while (true)
            {
                if (required && string.IsNullOrWhiteSpace(input))
                    Console.SetCursorPosition(Left, Top);
                else if (!required && string.IsNullOrWhiteSpace(input))
                {
                    Console.SetCursorPosition(Left, Top);

                    Console.WriteLine($"{@default}");

                    break;
                }
                else if (!string.IsNullOrWhiteSpace(input))
                {
                    if (!input.TryParse(out value))
                    {
                        Console.SetCursorPosition(Left, Top);

                        Console.Write(new string(' ', input.Length));

                        Console.SetCursorPosition(Left, Top);
                    }
                    else
                        break;
                }

                input = Console.ReadLine()?.Trim() ?? string.Empty;
            }

            Console.ResetColor();

            return value;
        }
    }
}
