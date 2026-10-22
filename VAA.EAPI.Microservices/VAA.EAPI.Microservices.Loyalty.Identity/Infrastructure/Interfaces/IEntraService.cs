using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.EntraEntities;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces
{
    public interface IEntraService
    {
        Task<string> Start(IEnumerable<ChallengeType> challengeTypes, string urlEntraJourney, string username, CancellationToken cancellationToken = default);
        Task<string> Initiate(IEnumerable<ChallengeType> challengeTypes, string username, CancellationToken cancellationToken = default);
        Task<string> Challenge(IEnumerable<ChallengeType> challengeTypes, string urlEntraJourney, string continuationToken,CancellationToken cancellationToken = default, string mfa_id = null);
        Task<string> Continue(string continuationToken, string grantType, string grantTypeValue, string urlEntraJourney, CancellationToken cancellationToken = default);
        Task<string> Token(string continuationToken, IEnumerable<string> grantTypes, IEnumerable<string> scopes, string username = null, string password = null, string oob = null, CancellationToken cancellationToken = default);
        Task<string> Submit(string continuationToken, string newPassword, CancellationToken cancellationToken = default);
        Task<string> PollCommunication(string continuationToken, CancellationToken cancellationToken = default);
        Task<string> Introspect(string urlEntraJourney, string continuationToken, CancellationToken cancellationToken = default);
        Task<string> RegisterIntrospect(string continuationToken, CancellationToken cancellationToken = default);
        Task<string> RegisterChallenge(string continuationToken, string challengeTarget, CancellationToken cancellationToken = default);
        Task<string> RegisterContinue(string continuationToken, string grantType, string grantTypeValue = null, CancellationToken cancellationToken = default);

    }
}
