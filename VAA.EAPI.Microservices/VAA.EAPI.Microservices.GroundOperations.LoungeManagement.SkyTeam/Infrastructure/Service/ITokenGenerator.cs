
namespace VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Infrastructure.Services
{
    public interface ITokenGenerator
    {
        public Task<string> GenerateToken(string key, string uri, Dictionary<string, string> clientCredentials);
    }
}
