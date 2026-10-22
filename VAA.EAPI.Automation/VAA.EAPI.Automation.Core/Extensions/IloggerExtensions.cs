using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Automation.Core.Diagnostics;

namespace VAA.EAPI.Automation.Core.Extensions
{
    public static class IloggerExtensions
    {
        public static Trace<T> CreateTrace<T>(this ILogger<T> source, [CallerMemberName] string origin = "") => Trace<T>.Create(source, origin);
        public static Trace<T> CreateTrace<T>(this ILogger source, [CallerMemberName] string origin = "") => Trace<T>.Create(source, origin);
    }
}
