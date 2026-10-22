using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.Common;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Helper.Interface
{
    public interface ICachedChannel
    {
        public Task<Channel> GetChannelEntity(string channelId);
        public IEnumerable<Channel> GetChannelEntities();
    }
}
