using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Mapper.SessionClose;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public class SessionCloseCommandHandler : IRequestHandler<SessionCloseCommand, bool>
    {
        private readonly ISessionClose _sessionClose;

        public SessionCloseCommandHandler(ISessionClose sessionClose)
        {
            _sessionClose = sessionClose ?? throw new ArgumentNullException(nameof(sessionClose));
        }

        public async Task<bool> Handle(SessionCloseCommand request, CancellationToken cancellationToken)
        {
            var sessionCloseResponse = await _sessionClose.SessionCloseAsync(SessionCloseRQMapper.CloseSessionRQMapper(request));

            return SessionCloseRSMapper.CloseSessionRSMapper(sessionCloseResponse);
        }
    }
}
