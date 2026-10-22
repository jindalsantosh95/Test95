using System.Linq;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Enums;

namespace VAA.EAPI.Infrastructure.Extentions
{
    public static class AsyncContextExtensions
    {
        public static string GetDeltaChannel(this AsyncContext source)
        {
            var channels = source.Channel.Select(x => x.Parse(ChannelType.DFWEB))
                                         .Distinct()
                                         .ToArray();

            if (channels.Any(x => x == ChannelType.DFMOBILE)) return nameof(ChannelType.DFMOBILE);
            if (channels.Any(x => x == ChannelType.NDC)) return nameof(ChannelType.NDC);

            return nameof(ChannelType.DFWEB);
        }
    }
}
