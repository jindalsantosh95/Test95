using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Configuration.Domain;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Interface;
using VAA.EAPI.Microservices.Configuration.Mapper;

namespace VAA.EAPI.Microservices.Configuration.Application
{
    public class CurrencyDBQueryHandler : IRequestHandler<CurrencyDBQuery, Result<Response<CurrencyData>>>
    {
        private readonly ICurrencyDetails _currencyDetails;
        public CurrencyDBQueryHandler(ICurrencyDetails currencyDetails)
        {
            _currencyDetails = currencyDetails ?? throw new ArgumentNullException(nameof(currencyDetails));
        }

        public async Task<Result<Response<CurrencyData>>> Handle(CurrencyDBQuery request, CancellationToken cancellationToken)
        {
            var currencyDetails = await _currencyDetails.GetCurrencyDetails(request.CountryCode);
            return Result.Success(new Response<CurrencyData>
            {
                Status = Status.OK,
                Value = ResponseMapper.MapCurrencyDetails(currencyDetails)
            });
        }
    }
}
