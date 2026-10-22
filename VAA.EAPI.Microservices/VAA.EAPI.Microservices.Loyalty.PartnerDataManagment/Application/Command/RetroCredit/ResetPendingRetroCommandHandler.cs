using MediatR;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.FlownDB.Interface;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Command.RetroCredit
{

    public class ResetPendingRetroCommandHandler : IRequestHandler<ResetPendingRetroCommand,string>
    {
        private readonly ILogger<ResetPendingRetroCommandHandler> _logger;
        private readonly IFlownDbRepository _flownDBRepository;

        public ResetPendingRetroCommandHandler(IFlownDbRepository flownDBRepository, ILogger<ResetPendingRetroCommandHandler> logger)
        {
            _flownDBRepository = flownDBRepository ?? throw new ArgumentNullException(nameof(flownDBRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> Handle(ResetPendingRetroCommand request, CancellationToken cancellationToken)
        {
            try
            {
                 await _flownDBRepository.ResetPendingFeedbackAutoRetroAsync(request.Days);

            }
            catch (Exception ex)
            {

                _logger.LogError(ex, $"AutoRetroInboundflowTimerCommandHandler exceptions");
                throw;
            }
           
            return Constants.Application.ResetPendingFeedBackAutoRetroProcessSuccessMessage;

        }

    }

}

