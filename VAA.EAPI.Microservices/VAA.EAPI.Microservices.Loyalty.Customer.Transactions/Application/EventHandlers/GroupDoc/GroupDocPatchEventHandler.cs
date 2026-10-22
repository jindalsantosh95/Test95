using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Exceptions;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Validators;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.GroupDoc;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Group;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.GroupDoc
{
    public class GroupDocPatchEventHandler : TransactionBaseEventHandler, IRequestHandler<GroupDocPatchEvent, Result<Unit>>
    {
        public GroupDocPatchEventHandler(IMediator mediator, ILogger<TransactionBaseEventHandler> logger) : base(mediator, logger)
        {
        }

        public Task<Result<Unit>> Handle(GroupDocPatchEvent request, CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }

        private async Task UpdateFCMDocument(string customerId, string updatedBy, DateTime currentTime)
        {
            _logger.LogInformation("GroupDocPatchEventHandler - UpdateFCMDocument - Start");
            try
            {
                Domain.FlyingClubMember.FlyingClubMember memberDoc = new()
                {
                    Id = customerId,
                    CustomerId = customerId,
                    MemberType = Constants.Application.FlyingClubMemberTypeIndividual,
                    LastUpdatedBy = updatedBy,
                    LastUpdatedOn = currentTime
                };

                FlyingClubMemberCommandBase fcMemberPatchcommand = new()
                {
                    FlyingClubMemberDocument = memberDoc,
                    CustomerId = customerId
                };
                var cloudEvent = FlyingClubMemberPatchCommandToEventMapper.CommandToEventMapper(fcMemberPatchcommand);

                await _mediator.Send(new EventRequestInsertCommand
                {
                    Id = cloudEvent.Id,
                    Type = cloudEvent.Type,
                    Data = cloudEvent,
                    Subject = cloudEvent.Subject
                });

                try
                {
                    await _mediator.Publish(cloudEvent);
                }
                catch
                {
                    await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, CancellationToken.None);
                    throw;
                }
            }
            finally
            {
                _logger.LogInformation("GroupDocPatchEventHandler - UpdateFCMDocument - End");
            }
        }

        protected async override Task<CosmosDocumentEvent> ValidateEvent(TransactionBaseEvent transactionEvent, CancellationToken cancellationToken)
        {
            GroupDocCommandBase patchCommand = Json.DeserializeObject<GroupDocCommandBase>((JToken)transactionEvent.Data);

            var validationResult = new GroupDocPatchCommandValidator().Validate(patchCommand);
            if (!validationResult.IsValid)
            {
                _logger.LogError($"GroupDocPatchEventHandler.Handle - GroupDocument patch request contains invalid data or missing mandatory fields. - {validationResult.Errors}");
                throw new RequestValidationException($"GroupDocPatchEventHandler.Handle - GroupDocument patch request contains invalid data or missing mandatory fields. - {validationResult.Errors}");
            }

            var existingDoc = (await GroupDocCommandBase.GetGroupDocuments(_mediator, patchCommand.GroupDoc.GroupId, cancellationToken))
                                                        .FirstOrDefault(n => n.GroupId == patchCommand.GroupDoc.GroupId);
            if (existingDoc is null)
            {
                _logger.LogError($"GroupDocPatchEventHandler.Handle - Error has occured -  Group Document not found for GroupId :{patchCommand.GroupDoc.GroupId}");
                throw new RequestValidationException(ErrorCodes.GroupIdNotFoundMessage);
            }

            existingDoc.LastUpdatedBy = !string.IsNullOrWhiteSpace(patchCommand.GroupDoc.LastUpdatedBy) ? patchCommand.GroupDoc.LastUpdatedBy : existingDoc.LastUpdatedBy;
            existingDoc.LastUpdatedOn = patchCommand.GroupDoc.LastUpdatedOn ?? existingDoc.LastUpdatedOn;

            if (patchCommand.GroupDoc.GroupHistory != null && patchCommand.GroupDoc.GroupHistory.Count > 0)
            {
                if (existingDoc.GroupHistory == null)
                {
                    existingDoc.GroupHistory = new List<GroupHistory>();
                }

                //calculate the number of active members only in the household to apply validation on member limit
                if (existingDoc.GroupHistory?.Count(x => !x.ExitDate.HasValue) < Constants.Application.MaxGroupHistoryCount)
                {
                    GroupHistory sameHistory = existingDoc.GroupHistory.Find(x => x.CustomerId == patchCommand.GroupDoc.GroupHistory.FirstOrDefault().CustomerId && !x.ExitDate.HasValue);
                    if (sameHistory == null && !patchCommand.GroupDoc.GroupHistory.FirstOrDefault().ExitDate.HasValue)
                    {
                        existingDoc.GroupHistory.Add(patchCommand.GroupDoc.GroupHistory.FirstOrDefault());
                    }
                    else
                    {
                        GroupHistory existingHistory = patchCommand.GroupDoc.GroupHistory.FirstOrDefault();
                        if (existingHistory.ExitDate.HasValue)
                        {
                            int index = existingDoc.GroupHistory.FindIndex(s => s.CustomerId == patchCommand.GroupDoc.GroupHistory.FirstOrDefault().CustomerId && !s.ExitDate.HasValue);
                            sameHistory.ExitDate = existingHistory.ExitDate;
                            sameHistory.LastUpdatedBy = existingDoc.LastUpdatedBy;
                            sameHistory.LastUpdatedOn = existingDoc.LastUpdatedOn;
                            if (index != -1)
                                existingDoc.GroupHistory[index] = sameHistory;

                            //Check for valid/without exit date member from history
                            GroupHistory history = existingDoc.GroupHistory.Where(s => !s.ExitDate.HasValue).FirstOrDefault();

                            if (history == null)
                            {
                                //The history is null means there is not valid group memeber
                                //send FCMDoc update event
                                try
                                {
                                    await UpdateFCMDocument(existingDoc.HeadOfGroupCustomerId,
                                        existingDoc.LastUpdatedBy,
                                        existingDoc.LastUpdatedOn.Value);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, $"GroupDocPatchEventHandler.Handle - Error has occured -  GroupId:{patchCommand.GroupDoc.GroupId} - {ex.Message}");
                                }
                            }
                        }
                        else if (existingHistory.HoHManaged != sameHistory.HoHManaged)
                        {
                            int index = existingDoc.GroupHistory.IndexOf(sameHistory);
                            sameHistory.HoHManaged = existingHistory.HoHManaged;
                            sameHistory.LastUpdatedBy = existingDoc.LastUpdatedBy;
                            sameHistory.LastUpdatedOn = existingDoc.LastUpdatedOn;

                            if (index != -1)
                            {
                                existingDoc.GroupHistory[index] = sameHistory;
                            }
                        }
                        else
                        {
                            _logger.LogError($"GroupDocPatchEventHandler.Handle - Error has occured -  GroupId:{patchCommand.GroupDoc.GroupId} - same member can't be linked twice or invalid GroupDoc");
                        }
                    }
                }
                else
                {
                    _logger.LogError($"GroupDocPatchEventHandler.Handle - Error has occured - GroupId:{patchCommand.GroupDoc.GroupId} - Maximum number of linking posible is : {Constants.Application.MaxGroupHistoryCount}");
                }
            }

            ItemRequestOptions requestOptions = new ItemRequestOptions { IfMatchEtag = existingDoc._etag };

            return new CosmosDocumentEvent(new GroupDocumentDTO(existingDoc), transactionEvent.EventType, requestOptions);
        }
    }
}