using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Models;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands
{
    public class LTGEmailTriggerCommandHandler : IRequestHandler<LTGEmailTriggerCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ICustomerProvider _customerProvider;
        private readonly ILogger<LTGEmailTriggerCommandHandler> _logger;

        public LTGEmailTriggerCommandHandler(IMediator mediator,
            ILogger<LTGEmailTriggerCommandHandler> logger,
            ICustomerProvider customerProvider)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
        }

        private async Task<bool> TriggerEmail(string firstName,
            string lastName,
            string customerId,
            string memberId,
            string email)
        {
            try
            {
                return await _mediator.Send(new HouseHoldAccountEmailTriggerCommand()
                {
                    Id = Guid.NewGuid(),
                    FirstName = firstName,
                    LastName = lastName,
                    CustomerId = customerId,
                    MemberNumber = memberId,
                    Email = email,
                    EmailTemplate = Constants.Environment.LTGAwardEmailTemplate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occured due to exception - {ex.Message}");
                return false;
            }
        }

        public async Task<Result<Response<string>>> Handle(LTGEmailTriggerCommand request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(request.CustomerId) &&
                !string.IsNullOrWhiteSpace(request.InitiatedBy) &&
                !string.IsNullOrWhiteSpace(request.MemberId) &&
                !string.IsNullOrWhiteSpace(request.Email) &&
                !string.IsNullOrWhiteSpace(request.FirstName) &&
                !string.IsNullOrWhiteSpace(request.LastName))
            {
                if (!await TriggerEmail(request.FirstName,
                    request.LastName,
                    request.CustomerId,
                    request.MemberId,
                    request.Email))
                {
                    _logger.LogError("LTGEmailTriggerCommandHandler - HouseHold account email trigger error");
                    return Result.Failure<Response<string>>("Email Trigger Faild");
                }
            }
            else if ((!string.IsNullOrWhiteSpace(request.CustomerId) ||
                !string.IsNullOrWhiteSpace(request.MemberId)) &&
                !string.IsNullOrWhiteSpace(request.InitiatedBy))
            {
                CustomerDTO customerDto = await _customerProvider.GetCustomer(request.CustomerId, request.MemberId);

                if (customerDto == null ||
                    customerDto.Customer == null ||
                    string.IsNullOrWhiteSpace(customerDto.Customer.CustomerId))
                {
                    return Result.Failure<Response<string>>("Invalid Request - CustomerId / MemberId");
                }

                if (!await TriggerEmail(customerDto.Customer.FirstName,
                    customerDto.Customer.LastName,
                    customerDto.Customer.CustomerId,
                    customerDto.Customer.LoyaltyNumber,
                    customerDto.Customer?.ContactDetails?.EmailDetails?.FirstOrDefault()?.EmailId?.Trim()))
                {
                    _logger.LogError("LTGEmailTriggerCommandHandler - HouseHold account email trigger error");
                    return Result.Failure<Response<string>>("Email Trigger Faild");
                }
            }
            else
            {
                return Result.Failure<Response<string>>("Invalid Request - CustomerId / MemberId / InitiatedBy fields are mandatory");
            }

            return new Response<string>
            {
                Status = Status.OK,
                Value = "Request processed successfully"
            };
        }
    }
}
