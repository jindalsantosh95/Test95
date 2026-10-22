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
using VAA.EAPI.Microservices.Retail.Reference.Domain.Interface;
using VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Models;

namespace VAA.EAPI.Microservices.Retail.Reference.Application.Query
{
    public class DistanceQueryHandler : IRequestHandler<DistanceQuery, Result<Response<string>>>
    {
        private readonly IScvRefDBRepository _scvRefDBRepository;
        private readonly ILogger<DistanceQueryHandler> _logger;

        public DistanceQueryHandler(IScvRefDBRepository scvRefDBRepository, ILogger<DistanceQueryHandler> logger)
        {
            _scvRefDBRepository = scvRefDBRepository ?? throw new ArgumentNullException(nameof(scvRefDBRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<string>>> Handle(DistanceQuery request, CancellationToken cancellationToken)
        {
            try
            {
                int distance = await _scvRefDBRepository.GetDistanceDetails(new Distance() { OriginCountryCode = request.OriginCountryCode, DestinationCountryCode = request.DestinationCountryCode });
                bool hasDistance = distance != 0;
                return Result.Success(new Response<string>
                {
                    Status = hasDistance ? VAA.EAPI.Domain.Entities.Common.Status.OK : VAA.EAPI.Domain.Entities.Common.Status.BusinessWarnings,
                    Value = hasDistance ? Convert.ToString(distance) : null,
                    Errors = hasDistance ? null : ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.SegmentZeroDistanceError), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                });
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, $"SqlException in Segment Distance. {ex.Message}");
                return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.SegmentDistanceError), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in Segment Distance. {ex.Message}");
                return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(EAPI.Infrastructure.Constants.Constants.TechnicalError), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }
    }
}
