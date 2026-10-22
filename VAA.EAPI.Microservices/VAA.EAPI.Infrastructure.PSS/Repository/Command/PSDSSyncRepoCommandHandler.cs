using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.PSS.Mapper;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.PSS.Repository.Command
{
    public class PSDSSyncRepoCommandHandler : IRequestHandler<PSDSSyncRepoCommand, bool>
    {
        private readonly ILogger<PSDSSyncRepoCommandHandler> _logger;
        private readonly IAir4PassengerV4 _air4PassengerV4;

        public PSDSSyncRepoCommandHandler(ILogger<PSDSSyncRepoCommandHandler> logger, IAir4PassengerV4 air4PassengerV4)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _air4PassengerV4 = air4PassengerV4 ?? throw new ArgumentNullException(nameof(air4PassengerV4));
        }

        public Task<bool> Handle(PSDSSyncRepoCommand request, CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            try
            {
                var updateSFPDRequest = PSDSSyncRQMapper.UpdateSFPDRequest(request.UpdateSFPDRequest);

                return Task.FromResult(_air4PassengerV4.PSDSSync(updateSFPDRequest).Result.MQPostingSuccessIndicator);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Fault Exception from PSDS Sync service");
                throw;
            }
        }
    }
}
