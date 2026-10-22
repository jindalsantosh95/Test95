using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Reference.Application.Interface;
using VAA.EAPI.Microservices.Retail.Reference.Domain;
using VAA.EAPI.Microservices.Retail.Reference.Domain.Interface;
using VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Models;

namespace VAA.EAPI.Microservices.Retail.Reference.Application.Query
{
    public class DistanceAndCountryCodeQueryHandler : IDistanceAndCountryCodeQueryHandler, IRequestHandler<DistanceAndCountryCodeQuery, Result<Response<DistanceAndCountryCodeResponse>>>
    {
        private readonly IScvRefDBRepository _scvRefDBRepository;
        private readonly ILogger<DistanceAndCountryCodeQueryHandler> _logger;

        public DistanceAndCountryCodeQueryHandler(IScvRefDBRepository scvRefDBRepository, ILogger<DistanceAndCountryCodeQueryHandler> logger)
        {
            _scvRefDBRepository = scvRefDBRepository ?? throw new ArgumentNullException(nameof(scvRefDBRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<DistanceAndCountryCodeResponse>>> Handle(DistanceAndCountryCodeQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _scvRefDBRepository.GetDistanceWithCountryCode(new Distance() { OriginCountryCode = request.OriginAirportCode, DestinationCountryCode = request.DestinationAirportCode });

                return Result.Success(new Response<DistanceAndCountryCodeResponse>
                {
                    Status = response is not null ? Status.OK : Status.BusinessWarnings,
                    Value = response,
                    Errors = response is not null ? null : ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.SegmentZeroDistanceError), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                });
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, $"SqlException in Segment Distance. {ex.Message}");
                return Result.Failure<Response<DistanceAndCountryCodeResponse>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.SegmentDistanceError), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in Segment Distance. {ex.Message}");
                return Result.Failure<Response<DistanceAndCountryCodeResponse>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(EAPI.Infrastructure.Constants.Constants.TechnicalError), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }
    }
}
