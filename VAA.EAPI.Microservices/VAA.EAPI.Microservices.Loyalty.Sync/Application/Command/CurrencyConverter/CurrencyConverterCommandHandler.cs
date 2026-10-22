using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Sync.Domain.Entities.PointsCurrencyConverter;
using VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Repository.CurrencyConverter;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Application.Command.CurrencyConverter
{
    public class CurrencyConverterCommandHandler : IRequestHandler<CurrencyConverterCommand, Result<Response<IEnumerable<PointsCurrencyConverterResponse>>>>
    {
        private readonly IMediator _mediator;

        public CurrencyConverterCommandHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Result<Response<IEnumerable<PointsCurrencyConverterResponse>>>> Handle(CurrencyConverterCommand request, CancellationToken cancellationToken)
        {
            var currencyconverterResponse = await _mediator.Send(new CurrencyConverterRepoCommand() { Request = request.Request }, cancellationToken);

            return new Response<IEnumerable<PointsCurrencyConverterResponse>>
            {
                Status = Status.OK,
                Value = currencyconverterResponse.Value.Value
            };
        }
    }
}
