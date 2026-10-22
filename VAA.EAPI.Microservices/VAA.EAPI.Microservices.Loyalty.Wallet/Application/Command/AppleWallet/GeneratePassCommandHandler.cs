using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Wallet.Constants;
using VAA.EAPI.Microservices.Loyalty.Wallet.Infrastructure.Repository;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Application.Command.AppleWallet
{
    public class GeneratePassCommandHandler : IRequestHandler<GeneratePassCommand, Response<byte[]>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GeneratePassCommandHandler> _logger;

        public GeneratePassCommandHandler(IMediator mediator, ILogger<GeneratePassCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Response<byte[]>> Handle(GeneratePassCommand request, CancellationToken cancellationToken)
        {
            try
            {
                return new Response<byte[]>
                {
                    Status = Status.OK,
                    Value = await _mediator.Send(new GeneratePassRepoCommand() { FcNumber = request.FcNumber, MemberName = request.MemberName, TierCode = request.TierCode, ValidUntil = request.ValidUntil != null ? DateTime.Parse(request.ValidUntil).ToString(Constants.Application.ValidUntilDateFormat) : string.Empty }),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Generate pass failed for serial number : {request.FcNumber} with error : {ex.Message}");
                return GetErrorResponse(ErrorCodes.GeneratePassError);
            }

        }

        private static Response<byte[]> GetErrorResponse(string errorCode)
        {
            return new Response<byte[]>()
            {
                Status = Status.BusinessException,
                Errors = new()
                {
                    ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(errorCode), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault()
                }
            };

        }
    }
}
