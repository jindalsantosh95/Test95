using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BookingClassRetrieval;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.PromotionEnrolledMembers;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query
{
    public class ValidateBookingClassQueryHandler : IRequestHandler<ValidateBookingClassQuery, Result<Response<BookingDetailsResult>>>
    {
        private readonly ICommonRefDBServices _commonRefDBService;
        private readonly ILogger<ValidateBookingClassQueryHandler> _logger;

        public ValidateBookingClassQueryHandler(ICommonRefDBServices commonRefDBService, ILogger<ValidateBookingClassQueryHandler> logger)
        {
            _commonRefDBService = commonRefDBService ?? throw new ArgumentNullException(nameof(commonRefDBService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<BookingDetailsResult>>> Handle(ValidateBookingClassQuery request, CancellationToken cancellationToken)
        {
            try
            {
                Result<Response<string>> response = new Result<Response<string>>();

                return new Response<BookingDetailsResult>
                {
                    Status = Status.OK,
                    Value = _commonRefDBService.ProcessBookingClassData(request.ValidationRequest.PartnerBookingData, request.ValidationRequest.bookingClass, request.ValidationRequest.flownClass, request.ValidationRequest.activityDate, request.ValidationRequest.listFareValue, request.ValidationRequest.listTicketValue, request.ValidationRequest.listPaxValue)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Validate Booking Class Query Handler failed due to Exception");
                return new Response<BookingDetailsResult>
                {
                    Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(EAPI.Infrastructure.Constants.Constants.TechnicalError), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath),
                    IgnoreNullValues = false
                };
            }
        }
    }
}
