using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Sync.Domain.Entities.PointsCurrencyConverter;
using VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Mapper.CurrencyConvert;
using VAA.EAPI.Microservices.Loyalty.Sync.PointsCurrencyConvertServiceRef;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Services.CurrencyConverter
{
    public class CurrencyConverterServiceCommandHandler : IRequestHandler<CurrencyConverterServiceCommand, Result<Response<IEnumerable<PointsCurrencyConverterResponse>>>>
    {
        private readonly ILogger<CurrencyConverterServiceCommandHandler> _logger;
        private readonly CurrencyConvertClient _currencyConvert;

        public CurrencyConverterServiceCommandHandler(ILogger<CurrencyConverterServiceCommandHandler> logger, CurrencyConvertClient currencyConvert)
        {
            _currencyConvert = currencyConvert ?? throw new ArgumentNullException(nameof(currencyConvert));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<IEnumerable<PointsCurrencyConverterResponse>>>> Handle(CurrencyConverterServiceCommand request, CancellationToken cancellationToken)
        {
            try
            {
                ExecuteRequestType currencyConvertRequest = CurrencyConvertDeltaMapper.CurrencyConvertRequestMapper(request.CurrencyConverterRequest); //map request

                _logger.LogInformation($"MS RQ: {Json.Serialize(request.CurrencyConverterRequest)}");
                _logger.LogInformation($"Delta RQ: {Json.Serialize(currencyConvertRequest)}");

                if (_currencyConvert.ClientCredentials.ClientCertificate.Certificate == null)
                {
                    _currencyConvert.ClientCredentials.ClientCertificate.Certificate = CertificateHelper.GetCachedCertificate();
                }

                ExecuteResponse currencyConvertResponse = await _currencyConvert.ExecuteAsync(currencyConvertRequest);

                _logger.LogInformation($"Delta RS: {Json.Serialize(currencyConvertResponse)}");
                
                var response = CurrencyConvertDeltaMapper.CurrencyConverterResponseMapper(currencyConvertResponse);

                return Result.Success(new Response<IEnumerable<PointsCurrencyConverterResponse>>
                {
                    Status = Status.OK,
                    Value = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} Points Converter RQ-{Json.Serialize(request.CurrencyConverterRequest)} Error in CurrencyConverterServiceCommandHandler: - {ex}");
                return Result.Failure<Response<IEnumerable<PointsCurrencyConverterResponse>>>((ex.InnerException ?? ex).ToString());
            }
        }
    }
}