using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Mapper
{
    public class AuthTokenResponseMapper
    {
        public static TokenClaimResponse ResponseMapper(IEnumerable<Claim> tokenResponse)
        {
            return new TokenClaimResponse()
            {
                CustomerId = tokenResponse.FirstOrDefault(claim => claim.Type.Is("CustomerId"))?.Value,
                EmailAddress = tokenResponse.FirstOrDefault(claim => claim.Type.Is("EmailAddress"))?.Value,
                FamilyName = tokenResponse.FirstOrDefault(claim => claim.Type.Is("family_name"))?.Value,
                ExpTime = tokenResponse.FirstOrDefault(claim => claim.Type.Is("exp"))?.Value,
                FcNumber = tokenResponse.FirstOrDefault(claim => claim.Type.Is("flyingclubNumber"))?.Value,
                GivenName = tokenResponse.FirstOrDefault(claim => claim.Type.Is("name"))?.Value,
                MemberType = tokenResponse.FirstOrDefault(claim => claim.Type.Is("memberType"))?.Value,
                MemberStatus = tokenResponse.FirstOrDefault(claim => claim.Type.Is("memberStatus"))?.Value
            };
        }
    }
}
