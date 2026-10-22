using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using VAA.EAPI.Web.Common.Diagnostics;

namespace VAA.EAPI.Web.Common.Extensions
{
    public static class IloggerExtensions
    {
        public static Trace<T> CreateTrace<T>(this ILogger<T> source, [CallerMemberName] string origin = "") => Trace<T>.Create(source, origin);
        public static Trace<T> CreateTrace<T>(this ILogger source, [CallerMemberName] string origin = "") => Trace<T>.Create(source, origin);
    }
}
