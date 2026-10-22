using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.PSS.Mapper;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.PSS.Repository.Command
{
    public class CreateAndSwingCityCommandHandler : IRequestHandler<CreateAndSwingCityCommand, string>
    {
        private readonly IAir4HostContextV2 _iAir4HostContextV2;
        public CreateAndSwingCityCommandHandler(IAir4HostContextV2 iAir4HostContextV2)
        {
            _iAir4HostContextV2 = iAir4HostContextV2 ?? throw new ArgumentNullException(nameof(iAir4HostContextV2));
        }

        public async Task<string> Handle(CreateAndSwingCityCommand createAndSwingCityCommand, CancellationToken cancellationToken)
        {
            var session = (await _iAir4HostContextV2.Create(HostContextMapper.CreateRequestMapping(createAndSwingCityCommand))).CreateResponse1.RequestInfo.SessionId.ToString();
            await _iAir4HostContextV2.Swing(HostContextMapper.SwingCityRequestMapping(createAndSwingCityCommand, session));
            return session;
        }
    }

}