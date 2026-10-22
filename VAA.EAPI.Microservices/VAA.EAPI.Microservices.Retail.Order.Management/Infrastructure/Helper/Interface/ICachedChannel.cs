using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Helper.Interface
{
    public interface ICachedChannel
    {
        public Task<Channel> GetChannelEntity(string channelId);
        public List<Channel> GetChannelEntities();
    }
}
