using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Mapper.CreateSession;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public class CreateSessionCommandHandler : IRequestHandler<CreateSessionCommand, string>
    {
        private readonly ICreateSession _createSession;

        public CreateSessionCommandHandler(ICreateSession createSession)
        {
            _createSession = createSession ?? throw new ArgumentNullException(nameof(createSession));
        }

        public async Task<string> Handle(CreateSessionCommand request, CancellationToken cancellationToken)
        {
            var createSessionResponse = await _createSession.CreateSessionAsync(CreateSessionRQMapper.SessionCreateRQMapper(request.PCC));

            return CreateSessionRSMapper.SessionCreateRSMapper(createSessionResponse);
        }
    }
}
