using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services
{
    public class RefundAPISvcCommandHandler : IRequestHandler<RefundAPISvcCommand, KeyValuePair<bool, string>>
    {
        private readonly ILogger<RefundAPISvcCommandHandler> _logger;

        public RefundAPISvcCommandHandler(ILogger<RefundAPISvcCommandHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<KeyValuePair<bool, string>> Handle(RefundAPISvcCommand request, CancellationToken cancellationToken)
        {
            //Placeholder to write logic for CPD API call
            return new KeyValuePair<bool, string>(true, string.Empty);
        }
    }
}