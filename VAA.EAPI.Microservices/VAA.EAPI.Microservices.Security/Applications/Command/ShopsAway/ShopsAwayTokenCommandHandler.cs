using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Security.Helper;

namespace VAA.EAPI.Microservices.Security.Applications.Command.ShopsAway
{
    public class ShopsAwayTokenCommandHandler : IRequestHandler<ShopsAwayTokenCommand, Response<string>>
    {
        private readonly ILogger<ShopsAwayTokenCommandHandler> _logger;

        public ShopsAwayTokenCommandHandler(ILogger<ShopsAwayTokenCommandHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Response<string>> Handle(ShopsAwayTokenCommand request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            string shopsAwayAESSecret = ConfigReader.Get(Constants.Environment.ShopsAwayAESSecret);

            if (shopsAwayAESSecret == null) throw new ArgumentNullException(nameof(shopsAwayAESSecret));

            return new Response<string>()
            {
                Status = Status.OK,
                Value = TokenHelper.EncryptDataWithAes(request.MemberDetails, shopsAwayAESSecret.Replace("-", string.Empty)),
                Errors = null
            };

        }
    }
}
