using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.HoHAccount;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.GroupDoc;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands
{
    public class HouseHoldAccountLinkingCommandHandler(IValidator<HouseHoldAccountLinkingCommand> validator, FlyingClubMemberDocumentServiceQueryHandler flyingClubMemberDocumentServiceQueryHandler, HouseHoldAccountEmailTriggerCommandHandler houseHoldAccountEmailTriggerCommandHandler, EventStoreRequestInsertCommandHandler eventStoreRequestInsertCommandHandler, EventStoreRequestDeleteCommandHandler eventStoreRequestDeleteCommandHandler, ServiceBusTopicMessageEventCommandHandler serviceBusTopicMessageEventHandler, ILogger<HouseHoldAccountLinkingCommandHandler> logger) : IHouseHoldAccountLinkingCommand
    {
        private readonly IValidator<HouseHoldAccountLinkingCommand> _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        private readonly FlyingClubMemberDocumentServiceQueryHandler _flyingClubMemberDocumentServiceQueryHandler = flyingClubMemberDocumentServiceQueryHandler ?? throw new ArgumentNullException(nameof(flyingClubMemberDocumentServiceQueryHandler));
        private readonly HouseHoldAccountEmailTriggerCommandHandler _houseHoldAccountEmailTriggerCommandhandler = houseHoldAccountEmailTriggerCommandHandler ?? throw new ArgumentNullException(nameof(houseHoldAccountEmailTriggerCommandHandler));
        private readonly EventStoreRequestInsertCommandHandler _eventStoreRequestInsertCommandHandler = eventStoreRequestInsertCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestInsertCommandHandler));
        private readonly EventStoreRequestDeleteCommandHandler _eventStoreRequestDeleteCommandHandler = eventStoreRequestDeleteCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestDeleteCommandHandler));
        private readonly ServiceBusTopicMessageEventCommandHandler _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventHandler ?? throw new ArgumentNullException(nameof(serviceBusTopicMessageEventHandler));
        private readonly ILogger<HouseHoldAccountLinkingCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<Result<Response<HouseHoldAccountLinkingResponse>>> Handle(HouseHoldAccountLinkingCommand request, CancellationToken cancellationToken)
        {
            {
                try
                {
                    var validationResult = await _validator.ValidateAsync(request, cancellationToken);

                    if (!validationResult.IsValid)
                    {
                        return Result.Failure<Response<HouseHoldAccountLinkingResponse>>(validationResult.Errors[0].ErrorMessage);
                    }

                    if (request.HeadAccount.CustomerId == request.ChildAccount.CustomerId)
                    {
                        return Result.Failure<Response<HouseHoldAccountLinkingResponse>>(error: $"CustomerId is same for Head and Child account{Constants.ErrorCodes.SameHoHAccountAndChildAccount}");
                    }

                    var flyingClubMemberDoc = await GetFlyingClubMemberDocument(request.HeadAccount.CustomerId, cancellationToken);

                    if (flyingClubMemberDoc is null)
                    {
                        return Result.Failure<Response<HouseHoldAccountLinkingResponse>>(error: $"Invalid customer id{Constants.ErrorCodes.InvalidCustomerId}");
                    }

                    if (flyingClubMemberDoc.Status != Constants.Application.FCMemberActiveStatus)
                    {
                        return Result.Failure<Response<HouseHoldAccountLinkingResponse>>($"HouseHoldAccountLinkingCommandHandler - Head of the account status is not active.ErrorCode:{Constants.ErrorCodes.FCMemberAccountNotActive}");
                    }

                    if (flyingClubMemberDoc.TierCode != Constants.Application.FCMemberSilver &&
                                    flyingClubMemberDoc.TierCode != Constants.Application.FCMemberGold &&
                                    flyingClubMemberDoc.TierCode != Constants.Application.FCMemberRed)
                    {
                        return Result.Failure<Response<HouseHoldAccountLinkingResponse>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.HoHLinkingPermittedOnlyForGoldAndSilver), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                    }

                    var hohManaged = IsDummyEmail(request.ChildAccount?.EmailId ?? string.Empty);
                    string headEmailTemplate = hohManaged
                        ? Constants.Environment.YoungFlyerEnrolmentConfirmationEmailTemplate
                        : flyingClubMemberDoc.TierCode == Constants.Application.FCMemberSilver
                            ? Constants.Environment.HoHLinkHeadSilverEmalTemplate
                            : Constants.Environment.HoHLinkHeadRedEmalTemplate;
                    string childEmailTemplate = flyingClubMemberDoc.TierCode == Constants.Application.FCMemberSilver ? Constants.Environment.HoHLinkChildSilverEmalTemplate : Constants.Environment.HoHLinkChildRedEmalTemplate;

                    if (!hohManaged)
                    {
                        if (!await TriggerEmail(request.HeadAccount, headEmailTemplate, cancellationToken))
                        {
                            return Result.Failure<Response<HouseHoldAccountLinkingResponse>>($"HouseHoldAccountLinkingCommandHandler - HoH Head email trigger error. ErrorCode: {Constants.ErrorCodes.HoHEmailTriggerError}");
                        }

                        if (!await TriggerEmail(request.ChildAccount, childEmailTemplate, cancellationToken))
                        {
                            return Result.Failure<Response<HouseHoldAccountLinkingResponse>>(Constants.ErrorCodes.HoHEmailTriggerError);
                        }
                    }

                    var currentTime = DateTime.UtcNow;
                    request.HoHManaged = hohManaged;

                    string groupId = string.IsNullOrWhiteSpace(flyingClubMemberDoc.GroupId) ? Guid.NewGuid().ToString() : flyingClubMemberDoc.GroupId;

                    var groupTask = string.IsNullOrWhiteSpace(flyingClubMemberDoc.GroupId)
                    ? CreateGroupDoc(
                            groupId,
                            flyingClubMemberDoc.CustomerId,
                            flyingClubMemberDoc.NameOnCard,
                            request.ChildAccount.CustomerId,
                            request.InitiatedBy,
                            hohManaged,
                            currentTime,
                            cancellationToken)
                    : UpdateGroupDoc(
                         flyingClubMemberDoc.GroupId,
                         request.ChildAccount.CustomerId,
                         request.InitiatedBy,
                         hohManaged,
                         currentTime,
                         cancellationToken);

                    var childTask = UpdateHoHChildFCMDoc(
                            request.ChildAccount.CustomerId,
                            flyingClubMemberDoc.MemberNumber,
                            request.InitiatedBy,
                            currentTime,
                            cancellationToken);

                    var headTask = UpdateHoHHeadFCMDoc(
                            groupId,
                            flyingClubMemberDoc.CustomerId,
                            request.InitiatedBy,
                            currentTime,
                            cancellationToken);

                    await System.Threading.Tasks.Task.WhenAll(groupTask, childTask, headTask);

                    if (hohManaged && !await TriggerEmail(request.HeadAccount, headEmailTemplate, cancellationToken, request.ChildAccount))
                    {
                        return Result.Failure<Response<HouseHoldAccountLinkingResponse>>($"HouseHoldAccountLinkingCommandHandler - Young Flyer enrolment confirmation email trigger error. ErrorCode: {Constants.ErrorCodes.HoHEmailTriggerError}");
                    }

                    return new Response<HouseHoldAccountLinkingResponse>
                    {
                        Status = Status.OK,
                        Value = new HouseHoldAccountLinkingResponse
                        {
                            Message = Constants.Application.HoHLinkingInitiatedMessage
                        }
                    };
                }
                catch (Exception ex)
                {
                    return Result.Failure<Response<HouseHoldAccountLinkingResponse>>(
                        Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(
                            new Exception(Constants.ErrorCodes.MiscellaneousTechnicalError),
                            Guid.NewGuid().ToString(),
                            VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                }
            }
        }
        private async Task<FlyingClubMember> GetFlyingClubMemberDocument(string customerId, CancellationToken cancellationToken)
        {
            var response = await _flyingClubMemberDocumentServiceQueryHandler.Handle(new FlyingClubMemberDocumentServiceQuery
            {
                CustomerId = customerId
            }, cancellationToken);

            return response.IsSuccess ? response.Value.Value : null;
        }
        private async Task UpdateGroupDoc(string groupDocId, string childAccountCustomerId, string initiatedBy, bool hohManaged, DateTime currentTime, CancellationToken cancellationToken)
        {
            string historyId = Guid.NewGuid().ToString();
            GroupHistory groupHistory = new GroupHistory()
            {
                HistoryId = historyId,
                CreatedBy = initiatedBy,
                CreatedOn = currentTime,
                LastUpdatedBy = initiatedBy,
                LastUpdatedOn = currentTime,
                StartDate = currentTime,
                ExitDate = null,
                HoHManaged = hohManaged,
                CustomerId = childAccountCustomerId
            };

            List<GroupHistory> groupHistoryList = new List<GroupHistory> { groupHistory };
            HouseHoldGroup updateGroup = new HouseHoldGroup()
            {
                GroupId = groupDocId,
                LastUpdatedBy = initiatedBy,
                LastUpdatedOn = currentTime,
                GroupHistory = groupHistoryList,
            };
            HouseHoldGroupUpdateEvent updateEvent = new HouseHoldGroupUpdateEvent()
            {
                GroupDoc = updateGroup,
            };
            var cloudEvent = UpdateGroupDocEventToCloudEventMapper.CommandToEventMapper(updateEvent);

            try
            {
                await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);

                await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand { Message = cloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), ServiceBusTopicName = cloudEvent.Source }, cancellationToken);
            }
            catch (Exception ex)
            {
                await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
                throw;
            }
        }
        private async Task CreateGroupDoc(string groupDocId, string headOfAccountCustomerId, string headMemberName, string childAccountCustomerId, string initiatedBy, bool hohManaged, DateTime currentTime, CancellationToken cancellationToken)
        {
            string historyId = Guid.NewGuid().ToString();
            GroupHistory groupHistory = new GroupHistory()
            {
                HistoryId = historyId,
                CreatedBy = initiatedBy,
                CreatedOn = currentTime,
                LastUpdatedBy = initiatedBy,
                LastUpdatedOn = currentTime,
                StartDate = currentTime,
                ExitDate = null,
                HoHManaged = hohManaged,
                CustomerId = childAccountCustomerId
            };
            List<GroupHistory> groupHistoryList = new List<GroupHistory> { groupHistory };
            HouseHoldGroup houseHoldGroup = new HouseHoldGroup()
            {
                Brand = Constants.Application.Brand,
                SchemaVersion = Constants.Application.SchemaVersion,
                DocType = ConfigReader.Get(Constants.Environment.GroupDocType),
                Id = groupDocId,
                GroupId = groupDocId,
                CreatedOn = currentTime,
                CreatedBy = initiatedBy,
                LastUpdatedOn = currentTime,
                LastUpdatedBy = initiatedBy,
                GroupName = headMemberName,
                GroupTypeCode = Constants.Application.HouseholdGroupTypeCode,
                Status = Constants.Application.FCMemberActiveStatus,
                HeadOfGroupCustomerId = headOfAccountCustomerId,
                GroupHistory = groupHistoryList,
            };
            HouseHoldGroupCreateEvent createEvent = new HouseHoldGroupCreateEvent()
            {
                GroupDoc = houseHoldGroup
            };
            var cloudEvent = CreateGroupDocEventToCloudEventMapper.CommandToEventMapper(createEvent);

            try
            {
                await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent, Subject = cloudEvent.Subject }, cancellationToken);

                await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand { Message = cloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), ServiceBusTopicName = cloudEvent.Source }, cancellationToken);
            }
            catch (Exception ex)
            {
                await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
                throw;
            }
        }
        private async Task UpdateHoHChildFCMDoc(string childAccountCustomerId, string headOfGroupMemberNumber, string initiatedBy, DateTime currentTime, CancellationToken cancellationToken)
        {
            FlyingClubMember memberDoc = new FlyingClubMember()
            {
                Id = childAccountCustomerId,
                CustomerId = childAccountCustomerId,
                HeadOfGroupMemberNumber = headOfGroupMemberNumber,
                GroupId = string.Empty,
                LastUpdatedBy = initiatedBy,
                MemberType = Constants.Application.FCMemberTypeIndividual,
                LastUpdatedOn = currentTime
            };

            FlyingClubMemberPatchEvent fcMemberPatchEvent = new FlyingClubMemberPatchEvent()
            {
                FlyingClubMemberDocument = memberDoc,
                CustomerId = childAccountCustomerId
            };
            var cloudEvent = FlyingClubMemberPatchEventToCloudEventMapper.CommandToEventMapper(fcMemberPatchEvent);

            try
            {
                await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand
                {
                    Id = cloudEvent.Id,
                    Type = cloudEvent.Type,
                    Data = cloudEvent.Data,
                    Subject = cloudEvent.Subject
                }, cancellationToken);

                await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand { Message = cloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), ServiceBusTopicName = cloudEvent.Source }, cancellationToken);
            }
            catch (Exception ex)
            {
                await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
                throw;
            }
        }
        private async Task UpdateHoHHeadFCMDoc(string groupDocId, string headAccountCustomerId, string initiatedBy, DateTime currentTime, CancellationToken cancellationToken)
        {
            FlyingClubMember memberDoc = new FlyingClubMember()
            {
                Id = headAccountCustomerId,
                CustomerId = headAccountCustomerId,
                GroupId = groupDocId,
                HeadOfGroupMemberNumber = string.Empty,
                MemberType = Constants.Application.HouseholdGroupTypeCode,
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
                await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);

                await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand { Message = cloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), ServiceBusTopicName = cloudEvent.Source }, cancellationToken);
            }
            catch (Exception ex)
            {
                await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
                throw;
            }
        }
        private async Task<bool> TriggerEmail(HouseHoldAccount account, string emailTemplate, CancellationToken cancellationToken, HouseHoldAccount childAccount = null)
        {
            try
            {
                return await _houseHoldAccountEmailTriggerCommandhandler.Handle(new HouseHoldAccountEmailTriggerCommand()
                {
                    Id = Guid.NewGuid(),
                    FirstName = account.FirstName,
                    LastName = account.LastName,
                    CustomerId = account.CustomerId,
                    MemberNumber = account.MemberId,
                    Email = account.EmailId,
                    EmailTemplate = emailTemplate,
                    ChildFirstName = childAccount?.FirstName,
                    ChildLastName = childAccount?.LastName,
                    ChildMemberNumber = childAccount?.MemberId,
                    IsYoungFlyerParentEmail = childAccount is not null
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError("HouseHoldAccountLinkingCommandHandler - TriggerEmail failed. ExceptionType: {ExceptionType}", ex.GetType().Name);
                return false;
            }
        }
        private static bool IsDummyEmail(string email)
        {
            if (email.IsNullOrWhiteSpace())
            {
                return false;
            }

            var trimmedEmail = email.Trim();
            var separatorIndex = trimmedEmail.LastIndexOf('@');

            if (separatorIndex <= 0 || separatorIndex == trimmedEmail.Length - 1)
            {
                return false;
            }

            var domain = trimmedEmail[(separatorIndex + 1)..].TrimEnd('.');

            return domain.Is(ConfigReader.Get(Constants.Application.DummyEmailDomain));
        }
    }
}
