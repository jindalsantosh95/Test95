using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignAssign;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Helper.Cache.Interface;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Models;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services.Interface;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.CampaignAssign;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Validations;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignAssign
{
    public class CampaignAssignEventProcessCommandHandler : IRequestHandler<CampaignAssignEventProcessCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CampaignAssignEventProcessCommandHandler> _logger;
        private readonly ICachedCampaignDetailsById _cachedCampaignDetailsById;
        private readonly ICustomerProvider _customerProvider;
        private readonly ITargetedMemberDocumentService _targetedMemberDocumentService;

        public CampaignAssignEventProcessCommandHandler(IMediator mediator, ILogger<CampaignAssignEventProcessCommandHandler> logger, ICachedCampaignDetailsById cachedCampaignDetailsById, ICustomerProvider customerProvider, ITargetedMemberDocumentService targetedMemberDocumentService)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cachedCampaignDetailsById = cachedCampaignDetailsById ?? throw new ArgumentNullException(nameof(cachedCampaignDetailsById));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
            _targetedMemberDocumentService = targetedMemberDocumentService ?? throw new ArgumentNullException(nameof(targetedMemberDocumentService));
        }

        public async Task<Result<Response<string>>> Handle(CampaignAssignEventProcessCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var camapignDetails = Json.Deserialize<CamapignAssignRequestData>(Convert.ToString(request.Data));

                if (new CampaignAssignRequestValidator().Validate(camapignDetails).IsValid)
                {
                    var campaignData = await _cachedCampaignDetailsById.GetCachedCampaignDetailsById(camapignDetails.CampaignId);

                    if (campaignData is not null)
                    {
                        string corporateCampaignTypes = EnumValidator.GetEnumValueByDescription<CorporateCampaignTypes>(campaignData.CampaignType);

                        if (!string.IsNullOrEmpty(camapignDetails.CustomerId))
                        {
                            var targatedresponse = await _targetedMemberDocumentService.GetMemberCampaigns(camapignDetails.CustomerId);
                            if ((bool)targatedresponse?.TargetedMembershipDocuments?.Where(x => x.ProfilingCampaignSourceId == camapignDetails.CampaignId).Any())
                            {
                                return Result.Success(new Response<string>
                                {
                                    Status = Status.BusinessException,
                                    Value = null,
                                    Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.CampaignCodeAlreadyAssigned), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                                });
                            }
                        }

                        if (string.IsNullOrEmpty(camapignDetails.CustomerId) || Enum.IsDefined(typeof(CorporateCampaignTypes), corporateCampaignTypes))
                        {
                            CustomerDTO customer = await _customerProvider.GetCustomer(camapignDetails.CustomerId, camapignDetails.MemberNumber);
                            if (customer == null || customer.Customer == null || string.IsNullOrWhiteSpace(customer.Customer.CustomerId))
                            {
                                _logger.LogInformation($"Invalid Request - CustomerId / MemberNumber");
                                return Result.Failure<Response<string>>("Invalid Request - CustomerId / MemberNumber");
                            }

                            if (string.IsNullOrEmpty(camapignDetails.CustomerId))
                            {
                                var targatedresponse = await _targetedMemberDocumentService.GetMemberCampaigns(customer.Customer.CustomerId);
                                if ((bool)targatedresponse?.TargetedMembershipDocuments?.Where(x => x.ProfilingCampaignSourceId == camapignDetails.CampaignId).Any())
                                {
                                    return Result.Success(new Response<string>
                                    {
                                        Status = Status.BusinessException,
                                        Value = null,
                                        Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.CampaignCodeAlreadyAssigned), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                                    });
                                }
                            }

                            camapignDetails.FirstName = customer.Customer.FirstName;
                            camapignDetails.LastName = customer.Customer.LastName;
                            camapignDetails.Email = customer.Customer.ContactDetails?.EmailDetails?.FirstOrDefault()?.EmailId?.Trim();
                            camapignDetails.Title = customer.Customer.Title;
                            camapignDetails.MemberNumber = !string.IsNullOrEmpty(camapignDetails.MemberNumber) ? camapignDetails.MemberNumber : customer.Customer?.LoyaltyNumber;
                            camapignDetails.CustomerId = !string.IsNullOrEmpty(camapignDetails.CustomerId) ? camapignDetails.CustomerId : customer.Customer?.CustomerId;

                        }

                        var eventData = CampaignAssignEventDataMapper.CampaignAssignRequestToEventDataMapper(camapignDetails, campaignData);
                        var cloudEvent = CampaignAssignEventMapper.CommandToAssignEventMapper(eventData);

                        await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent, Subject = cloudEvent.Subject });

                        try
                        {
                            await _mediator.Publish(cloudEvent);
                        }
                        catch
                        {
                            _logger.LogInformation($"Deleting the event from event store");
                            await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
                            throw;
                        }

                        return Result.Success(new Response<string>
                        {
                            Status = Status.OK,
                            Value = Constants.Application.CampaignAssignInitiated
                        });
                    }
                    else
                    {
                        return Result.Success(new Response<string>
                        {
                            Status = Status.BusinessException,
                            Value = null,
                            Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidCampaignId), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                        });
                    }
                }
                else
                {
                    return Result.Success(new Response<string>
                    {
                        Status = Status.BusinessException,
                        Value = null,
                        Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.MandatoryInputFieldsMissing), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Rollback from event store for CorrelationId: {AsyncContext.Current.CorrelationId} due to error in CampaignAssignEventCommandHandler");
                return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.MiscellaneousTechnicalError, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }


        }
    }
}
