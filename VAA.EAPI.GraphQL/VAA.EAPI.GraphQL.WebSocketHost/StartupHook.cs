using System.Reflection;

// The .NET startup-hook contract requires this exact type in the global namespace.
internal static class StartupHook
{
    public static void Initialize()
    {


        var name = Assembly.GetExecutingAssembly().GetName().Name!;
        var existing = Environment.GetEnvironmentVariable("ASPNETCORE_HOSTINGSTARTUPASSEMBLIES") ?? "";
        if (!existing.Split(';', StringSplitOptions.RemoveEmptyEntries).Contains(name, StringComparer.Ordinal))
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_HOSTINGSTARTUPASSEMBLIES",
                existing.Length == 0 ? name : existing + ";" + name);
        }
    }
}
