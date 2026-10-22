using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BirthdayBonus;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.BirthdayBonus;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands
{
    public class BirthdayBonusCommand : IRequest<string>
    {
        public Guid Id { get; set; }
    }

    public class BirthdayBonusCommandHandler : IRequestHandler<BirthdayBonusCommand, string>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<BirthdayBonusCommandHandler> _logger;

        public BirthdayBonusCommandHandler(IMediator mediator, ILogger<BirthdayBonusCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<string> Handle(BirthdayBonusCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var memberList = await _mediator.Send(new GetMembersForBirthdayBonusQuery());
                string productName = "Birthday Bonus";
                Product productDetail = await _mediator.Send(new GetProductRefrenceDetailsQuery { ProductName = productName });
                foreach (var member in memberList)
                {
                    try
                    {
                        var memberResponse = await _mediator.Send(request: new FlyingClubMemberDocumentServiceQuery { CustomerId = member.CustomerId });
                        FlyingClubMember memberData = memberResponse.Value.Value;
                        if ((memberData != null) && (memberData.TierCode == Constants.Application.BirthdayBonusTier) && (memberData.Status == Constants.Application.ActiveStatus))
                        {
                            BirthdayBonusRuleSetEntity tierRuleSetEntity = BirthdayBonusRuleSetEventMapper.MapBirthdayBonusRuleSetEventData(member, productDetail, memberData.Status);
                            var cloudEvent = BirthdayBonusRuleSetEventMapper.MapBirthdayBonusRuleSetEvent(tierRuleSetEntity);
                            _logger.LogDebug($"Store event in event store - Start");
                            await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent, Subject = cloudEvent.Subject });
                            _logger.LogDebug($"Store event in event store - End");
                            try
                            {
                                await _mediator.Publish(cloudEvent, CancellationToken.None);
                            }
                            catch
                            {
                                _logger.LogDebug($"Deleting the event from event store");
                                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"BirthdayBonus process failed for Member {member.MemberNumber} with Customer Id {member.CustomerId}");
                    }
                }

                return Constants.Application.BirthdayBonusProcessSuccessMessage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"BirthdayBonusCommandHandler {request.Id} failed due to Exception");
                return Constants.Application.Faliure;
            }
        }
    }
}
