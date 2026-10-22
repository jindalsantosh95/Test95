using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Validations;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands
{
    public class HouseHoldAccountValidationCommandHandler : IRequestHandler<HouseHoldAccountValidationCommand, Response<string>>
    {
        private readonly CustomerDetailsServiceQueryHandler _customerDetailsServiceQueryHandler;
        private readonly FlyingClubMemberDocumentServiceQueryHandler _flyingClubMemberDocumentServiceQueryHandler;
        private readonly ILogger<HouseHoldAccountValidationCommandHandler> _logger;
        
        public HouseHoldAccountValidationCommandHandler(
            CustomerDetailsServiceQueryHandler customerDetailsServiceQueryHandler,
            FlyingClubMemberDocumentServiceQueryHandler flyingClubMemberDocumentServiceQueryHandler,
            ILogger<HouseHoldAccountValidationCommandHandler> logger)
        {
            _customerDetailsServiceQueryHandler = customerDetailsServiceQueryHandler ?? throw new ArgumentNullException(nameof(customerDetailsServiceQueryHandler));
            _flyingClubMemberDocumentServiceQueryHandler = flyingClubMemberDocumentServiceQueryHandler ?? throw new ArgumentNullException(nameof(flyingClubMemberDocumentServiceQueryHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Response<string>> Handle(HouseHoldAccountValidationCommand request, CancellationToken cancellationToken)
        {          

            try
            {
                 if (string.IsNullOrEmpty(request.IndividualFcNumber) || string.IsNullOrEmpty(request.LeaderFcNumber))
                 {
                    return InvalidFcNumber();
                 }

                var individualCustomerTask = _customerDetailsServiceQueryHandler.Handle(
                    new CustomerDetailsServiceQuery { MemberId = request.IndividualFcNumber },
                    cancellationToken);
                var leaderCustomerTask = _customerDetailsServiceQueryHandler.Handle(
                    new CustomerDetailsServiceQuery { MemberId = request.LeaderFcNumber },
                    cancellationToken);

                try
                {
                    await Task.WhenAll(individualCustomerTask, leaderCustomerTask);
                }
                catch
                {
                    var unprocessableEntityException = FindUnprocessableEntityException(individualCustomerTask, leaderCustomerTask);
                    if (unprocessableEntityException is not null)
                    {
                        _logger.LogError(unprocessableEntityException, "Error occurred while Household account validation of FC number");
                        return InvalidFcNumber();
                    }

                    throw;
                }

                var individualcustomerResponse = (await individualCustomerTask).Value.Value;
                var leaderCustomerResponse = (await leaderCustomerTask).Value.Value;

                var flyingClubResponse = (await _flyingClubMemberDocumentServiceQueryHandler.Handle(new FlyingClubMemberDocumentServiceQuery { CustomerId = individualcustomerResponse.Customer.CustomerId }, cancellationToken)).Value.Value;

                var response = HouseHoldAccountInviteMemberValidator.MapInviteMemberValidator(individualcustomerResponse, leaderCustomerResponse, flyingClubResponse);

                if (response.ErrorDetails.Any())
                    return new Response<string>()
                    {
                        Status = Status.BusinessException,
                        IgnoreNullValues = false,
                        Errors = (List<ErrorDetails>)response.ErrorDetails
                    };

                return new Response<string>()
                {
                    Status = Status.OK,
                    IgnoreNullValues = false,
                    Value = "HouseHold Account Validation Successfully"
                };
            }

            catch (HttpRequestException ex) when (IsUnprocessableEntity(ex))
            {
                _logger.LogError(ex, "Error occurred while Household account validation of FC number");
                return InvalidFcNumber();
            }
        }

        private static bool IsUnprocessableEntity(HttpRequestException exception) =>
            exception.StatusCode == HttpStatusCode.UnprocessableEntity ||
            exception.Data["StatusCode"]?.ToString() == HttpStatusCode.UnprocessableEntity.ToString();

        private static HttpRequestException? FindUnprocessableEntityException(params Task[] tasks) =>
            tasks
                .Where(task => task.Exception is not null)
                .SelectMany(task => task.Exception!.Flatten().InnerExceptions)
                .OfType<HttpRequestException>()
                .FirstOrDefault(IsUnprocessableEntity);

        private Response<string> InvalidFcNumber()
        {
            return new Response<string>()
            {
                Status = Status.BusinessException,
                Errors = new()
                {
                    ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.InvalidFcNumber, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault()
                },
            };
        }
    }
}
