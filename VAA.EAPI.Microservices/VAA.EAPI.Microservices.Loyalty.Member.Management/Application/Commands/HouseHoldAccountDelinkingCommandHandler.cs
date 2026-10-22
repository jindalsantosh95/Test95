using CSharpFunctionalExtensions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.HoHAccount;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.GroupDoc;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands
{
    public class HouseHoldAccountDelinkingCommandHandler : IRequestHandler<HouseHoldAccountDelinkingCommand, Result<Response<HouseHoldAccountDelinkingResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<HouseHoldAccountDelinkingCommandHandler> _logger;
        private readonly IValidator<HouseHoldAccountDelinkingCommand> _validator;

        public HouseHoldAccountDelinkingCommandHandler(IMediator mediator,
            ILogger<HouseHoldAccountDelinkingCommandHandler> logger,
            IValidator<HouseHoldAccountDelinkingCommand> validator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        private async Task UpdateGroupDoc(string groupDocId,
            string childAccountCustomerId,
            string initiatedBy,
            DateTime currentTime)
        {
            _logger.LogInformation("HouseHoldAccountDelinkingCommandHandler - UpdateGroupDoc - Start");

            GroupHistory groupHistory = new GroupHistory()
            {
                LastUpdatedBy = initiatedBy,
                LastUpdatedOn = currentTime,
                ExitDate = currentTime,
                CustomerId = childAccountCustomerId
            };

            List<GroupHistory> groupHistoryList = new List<GroupHistory> { groupHistory };
            HouseHoldGroup updateGroup = new HouseHoldGroup()
            {
                GroupId = groupDocId,
                LastUpdatedBy = initiatedBy,
                LastUpdatedOn = currentTime,
                GroupHistory = groupHistoryList
            };
            HouseHoldGroupUpdateEvent updateEvent = new HouseHoldGroupUpdateEvent()
            {
                GroupDoc = updateGroup,
            };
            var cloudEvent = UpdateGroupDocEventToCloudEventMapper.CommandToEventMapper(updateEvent);

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
            finally
            {
                _logger.LogInformation("HouseHoldAccountDelinkingCommandHandler - UpdateGroupDoc - End");
            }
        }

        private async Task UpdateHoHChildFCMDoc(string childAccountCustomerId,
            string initiatedBy,
            DateTime currentTime)
        {
            _logger.LogInformation("HouseHoldAccountDelinkingCommandHandler - UpdateHoHChildFCMDoc - Start");
            FlyingClubMember memberDoc = new FlyingClubMember()
            {
                Id = childAccountCustomerId,
                CustomerId = childAccountCustomerId,
                HeadOfGroupMemberNumber = string.Empty,
                LastUpdatedBy = initiatedBy,
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
            finally
            {
                _logger.LogInformation("HouseHoldAccountDelinkingCommandHandler - UpdateHoHChildFCMDoc - End");
            }
        }

        private async Task<bool> TriggerEmail(HouseHoldAccount account,
            string emailTemplate)
        {
            _logger.LogInformation("HouseHoldAccountDelinkingCommandHandler - TriggerEmail - start");
            try
            {
                return await _mediator.Send(new HouseHoldAccountEmailTriggerCommand()
                {
                    Id = Guid.NewGuid(),
                    FirstName = account.FirstName,
                    LastName = account.LastName,
                    CustomerId = account.CustomerId,
                    MemberNumber = account.MemberId,
                    Email = account.EmailId,
                    EmailTemplate = emailTemplate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occured due to exception - {ex.Message}");
                return false;
            }
            finally
            {
                _logger.LogInformation("HouseHoldAccountDelinkingCommandHandler - TriggerEmail - End");
            }
        }

        public async Task<Result<Response<HouseHoldAccountDelinkingResponse>>> Handle(HouseHoldAccountDelinkingCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("HouseHoldAccountDelinkingCommandHandler - Start");
            try
            {
                var commandValidator = await _validator.ValidateAsync(request);
                if (!commandValidator.IsValid)
                {
                    _logger.LogError("HouseHoldAccountDelinkingCommandHandler - Error in request validation");
                    List<string> errorList = new List<string>();
                    foreach (var error in commandValidator.Errors)
                    {
                        errorList.Add(error.ErrorMessage);
                    }
                    return Result.Failure<Response<HouseHoldAccountDelinkingResponse>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(errorList[0]), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                }

                if (request.HeadAccount.CustomerId == request.ChildAccount.CustomerId)
                {
                    _logger.LogError("HouseHoldAccountDelinkingCommandHandler - CustomerId is same for Head and child account");
                    return Result.Failure<Response<HouseHoldAccountDelinkingResponse>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.SameHoHAccountAndChildAccount), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                }

                Result<Response<FlyingClubMember>> response = await _mediator.Send(request: new FlyingClubMemberDocumentServiceQuery { CustomerId = request.HeadAccount.CustomerId });
                FlyingClubMember flyingClubMemberDoc = response.Value.Value;

                if (flyingClubMemberDoc == null)
                {
                    _logger.LogError("HouseHoldAccountDelinkingCommandHandler - Head of the account - InvalidCustomerId");
                    return Result.Failure<Response<HouseHoldAccountDelinkingResponse>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidCustomerId), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                }

                if (string.IsNullOrWhiteSpace(flyingClubMemberDoc.GroupId))
                {
                    _logger.LogError("HouseHoldAccountDelinkingCommandHandler - GroupId is not found for specified head customer");
                    return Result.Failure<Response<HouseHoldAccountDelinkingResponse>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.HoHDelinkGroupIdNotFoundError), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                }

                await UpdateHoHChildFCMDoc(request.ChildAccount.CustomerId,
                    request.InitiatedBy,
                    DateTime.UtcNow);

                await UpdateGroupDoc(flyingClubMemberDoc.GroupId,
                    request.ChildAccount.CustomerId,
                    request.InitiatedBy,
                    DateTime.UtcNow);

                if (!request.RestrictEmailTrigger)
                {
                    //Trrigger email to HoH Head on delinking
                    if (!await TriggerEmail(request.HeadAccount, Constants.Environment.HoHDelinkHeadEmailTemplate))
                    {
                        _logger.LogError("HouseHoldAccountDelinkingCommandHandler - HouseHold account email trigger error");
                        return Result.Failure<Response<HouseHoldAccountDelinkingResponse>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.HoHEmailTriggerError), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                    }
                }

                return new Response<HouseHoldAccountDelinkingResponse>
                {
                    Status = Status.OK,
                    Value = new HouseHoldAccountDelinkingResponse() { Message = Constants.Application.HoHLinkingInitiatedMessage }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error has occured in HouseHoldAccountDelinking functionality with Customer id:{request.HeadAccount.CustomerId} and child customerId: {request.ChildAccount.CustomerId}");
                return Result.Failure<Response<HouseHoldAccountDelinkingResponse>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.MiscellaneousTechnicalError), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
            }
            finally
            {
                _logger.LogInformation("HouseHoldAccountDelinkingCommandHandler - Executed");
            }
        }
    }
}