using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Reference.Domain;
using VAA.EAPI.Microservices.Retail.Reference.Domain.Interface;

namespace VAA.EAPI.Microservices.Retail.Reference.Application.Query
{
    public class GetBookingDetailsQueryHandler : IRequestHandler<GetBookingDetailsQuery, Response<IEnumerable<BookingDetails>>>
    {
        private readonly IScvRefDBRepository _scvRefDBRepository;
        private readonly ILogger<GetBookingDetailsQueryHandler> _logger;

        public GetBookingDetailsQueryHandler(IScvRefDBRepository scvRefDBRepository, ILogger<GetBookingDetailsQueryHandler> logger)
        {
            _scvRefDBRepository = scvRefDBRepository ?? throw new ArgumentNullException(nameof(scvRefDBRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        public async Task<Response<IEnumerable<BookingDetails>>> Handle(GetBookingDetailsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var bookingDetails = await _scvRefDBRepository.GetBookingDetails(request.flownDetails);

                return new Response<IEnumerable<BookingDetails>>()
                {
                    Status = Status.OK,
                    Value = bookingDetails,
                    Errors = null
                };
            }
            catch (SqlException ex)
            {                
                _logger.LogError(ex, $"SqlException in Get Booking Class. {ex.Message}");
                return new Response<IEnumerable<BookingDetails>>()
                {
                    Status = Status.BusinessException,
                    Value = null,
                    Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.ValidateBookingError), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                };
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in Get Booking Class. {ex.Message}");
                return new Response<IEnumerable<BookingDetails>>()
                {
                    Status = Status.BusinessException,
                    Value = null,
                    Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(EAPI.Infrastructure.Constants.Constants.TechnicalError), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)
                };
            }
        }       
    }
}
