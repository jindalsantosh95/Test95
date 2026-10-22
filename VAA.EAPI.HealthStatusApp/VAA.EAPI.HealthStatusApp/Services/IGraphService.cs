namespace VAA.EAPI.HealthStatusApp.Services
{
    public interface IGraphService
    {
        Task<bool> IsUserInGroupAsync(string userId, string groupId);
    }
}
