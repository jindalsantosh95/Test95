using Newtonsoft.Json;

namespace VAA.EAPI.GraphQL.Domain.VHols.Models;

public static class Extensions
{
    public static bool TryParseJson<T>(this string source, out T? result)
    {
        bool success = true;
        var settings = new JsonSerializerSettings
        {
            Error = (sender, args) =>
            {
                success = false;
                args.ErrorContext.Handled = true;
            },
            MissingMemberHandling = MissingMemberHandling.Error
        };

        result = JsonConvert.DeserializeObject<T>(source, settings);

        return success;
    }
}