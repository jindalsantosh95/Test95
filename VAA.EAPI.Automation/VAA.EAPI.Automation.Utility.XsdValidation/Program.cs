using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;

namespace VAA.EAPI.Automation.Utility.XsdValidation
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Xsd validation utility");
            Console.WriteLine();

            if (!args.ArgumentExists("-load:"))
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("-load:<Xml file path> required.");

                return;
            }

            if (!args.ArgumentExists("-schemas:"))
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("-schemas:<uri=filePath,..> required.");

                return;
            }

            var xmlFilePath = args.Argument("-load");
            var schemas = args.Argument("-schemas").Split(',').Select(x => x.Split('='));

            if (!File.Exists(xmlFilePath))
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"-load{xmlFilePath} xml file does not exists.");

                return;
            }

            foreach (var schema in schemas)
                if (!File.Exists(schema[1]))
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine($"-schemas:{schema[0]}={schema[1]} xsd file does not exists.");

                    return;
                }

            if (!SchemaValidator.XsdSchemaValidator(File.ReadAllText(xmlFilePath), schemas.Select(x => string.Join('=', x)).ToArray(), out string[] exceptions))
                foreach (var item in exceptions)
                {
                    if (item.StartsWith("Warning"))
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(item);
                        Console.ResetColor();
                    }
                    else if (item.StartsWith("Error"))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine(item);
                        Console.ResetColor();
                    }
                }
        }
    }
}