using System.Threading.Tasks;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Interfaces
{
    public interface ICheckInOpeningService
    {
        public Task SynchronizeAsync();
    }
}
