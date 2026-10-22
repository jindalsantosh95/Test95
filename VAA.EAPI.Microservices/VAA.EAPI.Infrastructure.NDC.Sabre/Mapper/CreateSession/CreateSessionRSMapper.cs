using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.CreateSession;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Mapper.CreateSession
{
    public class CreateSessionRSMapper
    {
        public static string SessionCreateRSMapper(SessionCreateRQResponse sessionCreateRQResponse)
        {
            return sessionCreateRQResponse?.SessionCreateRS?.status == "Approved" ? sessionCreateRQResponse.Security.BinarySecurityToken.Value : null;
        }
    }
}
