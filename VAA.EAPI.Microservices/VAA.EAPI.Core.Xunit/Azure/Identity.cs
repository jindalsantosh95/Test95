using System.Security.Principal;

namespace VAA.EAPI.Core.Xunit.Azure
{
    public sealed class Identity : IIdentity
    {
        public string AuthenticationType => "TestAuthentication";
        public bool IsAuthenticated => true;
        public string Name => "TestUser";
    }
}
