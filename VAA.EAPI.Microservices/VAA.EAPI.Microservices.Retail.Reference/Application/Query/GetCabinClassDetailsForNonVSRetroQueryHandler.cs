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
using VAA.EAPI.Microservices.Retail.Reference.Domain;
using VAA.EAPI.Microservices.Retail.Reference.Domain.Interface;

namespace VAA.EAPI.Microservices.Retail.Reference.Application.Query
{
    public class GetCabinClassDetailsForNonVSRetroQueryHandler : IRequestHandler<GetCabinClassDetailsForNonVSRetroQuery, Result<Response<NonVSRetroCabinClassDetails>>>
    {
        private readonly IScvRefDBRepository _scvRefDBRepository;
        private readonly ILogger<GetCabinClassDetailsForNonVSRetroQueryHandler> _logger;

        public GetCabinClassDetailsForNonVSRetroQueryHandler(IScvRefDBRepository scvRefDBRepository, ILogger<GetCabinClassDetailsForNonVSRetroQueryHandler> logger)
        {
            _scvRefDBRepository = scvRefDBRepository ?? throw new ArgumentNullException(nameof(scvRefDBRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        public async Task<Result<Response<NonVSRetroCabinClassDetails>>> Handle(GetCabinClassDetailsForNonVSRetroQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var cabinClassDetails = await _scvRefDBRepository.GetCabinClassForNonVSRetro(request);

                return new Response<NonVSRetroCabinClassDetails>()
                {
                    Status = Status.OK,
                    Value = cabinClassDetails,
                    Errors = null
                };
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, $"SqlException in Get cabin Class for non-VS retro. {ex.Message}");
                return Result.Failure<Response<NonVSRetroCabinClassDetails>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.ValidateCabinForNonVSRetroError), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in Get cabin Class for non-VS retro. {ex.Message}");
                return Result.Failure<Response<NonVSRetroCabinClassDetails>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(EAPI.Infrastructure.Constants.Constants.TechnicalError), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }
    }
}
