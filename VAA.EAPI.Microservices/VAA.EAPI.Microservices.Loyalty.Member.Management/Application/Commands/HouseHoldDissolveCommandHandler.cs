using CSharpFunctionalExtensions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.HoHAccount;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Models;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.FlyingClubMember;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands
{
    public class HouseHoldDissolveCommandHandler : IRequestHandler<HouseHoldDissolveCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<HouseHoldDissolveCommandHandler> _logger;
        private readonly ICustomerProvider _customerProvider;

        public HouseHoldDissolveCommandHandler(IMediator mediator,
            ILogger<HouseHoldDissolveCommandHandler> logger,
            ICustomerProvider customerProvider)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
        }

        private async Task<Result<Response<string>>> DelinkHouseholdChild(string headCustomerId,
            string headEmail,
            string headMemberId,
            string headFirstName,
            string headLastName,
            string childCustomerId,
            string initiatedBy, int childCount)
        {
            try
            {
                CustomerDTO childCustomer = await _customerProvider.GetCustomer(childCustomerId, null);

                if (childCustomer == null ||
                    childCustomer.Customer == null ||
                    string.IsNullOrWhiteSpace(childCustomer.Customer.CustomerId))
                {
                    return Result.Failure<Response<string>>("Invalid Request - CustomerId / MemberId");
                }

                HouseHoldAccount headAccount = new HouseHoldAccount()
                {
                    CustomerId = headCustomerId,
                    MemberId = headMemberId,
                    EmailId = headEmail,
                    FirstName = headFirstName,
                    LastName = headLastName
                };

                HouseHoldAccount childAccount = new HouseHoldAccount()
                {
                    CustomerId = childCustomer.Customer?.CustomerId,
                    MemberId = childCustomer.Customer?.LoyaltyNumber,
                    EmailId = childCustomer.Customer?.ContactDetails?.EmailDetails?.FirstOrDefault()?.EmailId?.Trim(),
                    FirstName = childCustomer.Customer?.FirstName,
                    LastName = childCustomer.Customer?.LastName
                };

                await _mediator.Send(new HouseHoldAccountDelinkingCommand()
                {
                    InitiatedBy = initiatedBy,
                    HeadAccount = headAccount,
                    ChildAccount = childAccount,
                    RestrictEmailTrigger = childCount == 1 ? false : true
                });

                return new Response<string>
                {
                    Status = Status.OK,
                    Value = "Request processed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error has occured in tion MS method HouseHoldDissolveCommandHandler:DelinkHouseholdChild call with Customer id:{headCustomerId}");
                throw;
            }
        }

        private async Task UpdateHoHHeadFCMDoc(string headAccountCustomerId,
            string initiatedBy,
            DateTime currentTime)
        {
            FlyingClubMember memberDoc = new FlyingClubMember()
            {
                Id = headAccountCustomerId,
                CustomerId = headAccountCustomerId,
                MemberType = Constants.Application.FCMemberTypeIndividual,
                LastUpdatedBy = initiatedBy,
                LastUpdatedOn = currentTime
            };

            FlyingClubMemberPatchEvent fcMemberPatchEvent = new FlyingClubMemberPatchEvent()
            {
                FlyingClubMemberDocument = memberDoc,
                CustomerId = headAccountCustomerId
            };

            var cloudEvent = FlyingClubMemberPatchEventToCloudEventMapper.CommandToEventMapper(fcMemberPatchEvent);

            try
            {
                await _mediator.Send(new EventRequestInsertCommand
                {
                    Id = cloudEvent.Id,
                    Type = cloudEvent.Type,
                    Data = cloudEvent,
                    Subject = cloudEvent.Subject
                });
                await _mediator.Publish(cloudEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Process failed due to Exception - {ex.Message}");
                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, CancellationToken.None);
                throw;
            }
        }

        public async Task<Result<Response<string>>> Handle(HouseHoldDissolveCommand request, CancellationToken cancellationToken)
        {

            HouseHoldDissolveRequest dissolveRequest = Json.Deserialize<HouseHoldDissolveRequest>(Convert.ToString(request.Data));

            if (dissolveRequest == null ||
                (string.IsNullOrWhiteSpace(dissolveRequest.CustomerId) && string.IsNullOrWhiteSpace(dissolveRequest.MemberId)))
            {
                return Result.Failure<Response<string>>("Invalid Request - CustomerId / MemberId");
            }

            CustomerDTO headCustomer = await _customerProvider.GetCustomer(dissolveRequest.CustomerId, dissolveRequest.MemberId);
            if (headCustomer == null ||
                headCustomer.Customer == null ||
                string.IsNullOrWhiteSpace(headCustomer.Customer.CustomerId))
            {
                return Result.Failure<Response<string>>("Invalid Request - CustomerId / MemberId");
            }

            Result<Response<HouseHoldGroup>> response = await _mediator.Send(new GroupDocumentServiceQuery { CustomerId = headCustomer.Customer.CustomerId });
            HouseHoldGroup hohDetails = response.Value.Value;

            if (hohDetails == null ||
                hohDetails.GroupHistory == null &&
                hohDetails.GroupHistory.Count <= 0)
            {
                return Result.Failure<Response<string>>("Invalid Request - no group history to dissolve");
            }

            int childCount = hohDetails.GroupHistory.Where(i => (i.ExitDate == null || !(i.ExitDate.HasValue))).Count();

            await Task.WhenAll(hohDetails.GroupHistory.Where(i => (i.ExitDate == null || !(i.ExitDate.HasValue)))
                .Select(i => DelinkHouseholdChild(headCustomer.Customer?.CustomerId,
                headCustomer.Customer?.ContactDetails?.EmailDetails?.FirstOrDefault()?.EmailId?.Trim(),
                headCustomer.Customer?.LoyaltyNumber,
                headCustomer.Customer?.FirstName,
                headCustomer.Customer?.LastName,
                i.CustomerId,
                dissolveRequest.InitiatedBy,
                childCount--)));

            //call update hoh account
            await UpdateHoHHeadFCMDoc(headCustomer.Customer.CustomerId,
                dissolveRequest.InitiatedBy,
                DateTime.UtcNow);
            return new Response<string>
            {
                Status = Status.OK,
                Value = "Request processed successfully"
            };
        }
    }
}