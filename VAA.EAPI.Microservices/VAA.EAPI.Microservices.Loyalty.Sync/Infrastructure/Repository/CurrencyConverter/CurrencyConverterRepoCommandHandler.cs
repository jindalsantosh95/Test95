using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Sync.Domain.Entities.PointsCurrencyConverter;
using VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Services.CurrencyConverter;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Repository.CurrencyConverter
{
    public class CurrencyConverterRepoCommandHandler : IRequestHandler<CurrencyConverterRepoCommand, Result<Response<IEnumerable<PointsCurrencyConverterResponse>>>>
    {
        private readonly IMediator _mediator;

        public CurrencyConverterRepoCommandHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Result<Response<IEnumerable<PointsCurrencyConverterResponse>>>> Handle(CurrencyConverterRepoCommand request, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new CurrencyConverterServiceCommand() { CurrencyConverterRequest = request.Request }, cancellationToken);
        }
    }
}