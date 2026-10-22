using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Model;

namespace VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Helper
{
    public interface ICacheConfig
    {
        public Task<ChannelEntity> GetChannelEntity(string channelId);
        public List<ChannelEntity> GetChannelEntities();
    }
}
