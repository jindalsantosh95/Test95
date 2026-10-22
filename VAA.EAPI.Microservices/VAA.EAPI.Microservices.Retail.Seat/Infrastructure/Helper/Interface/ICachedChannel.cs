using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Seat.Domain;

namespace VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Helper.Interface
{
    public interface ICachedChannel
    {
        public Task<Channel> GetChannelEntity(string channelId);
        public List<Channel> GetChannelEntities();
    }
}
