using System.Threading.Tasks;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Interfaces
{
    public interface ISeatSelectionReminderService
    {
        Task SynchronizeAsync();
    }
}
