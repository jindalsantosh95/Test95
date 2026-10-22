using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Loyalty.Identity.Mapper;
using VAA.EAPI.Microservices.Loyalty.Identity.Validation;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.ManageAccount
{
    public class UpdateAzureB2CDetailsCommandHandler : IRequestHandler<UpdateAzureB2CDetailsCommand, Result<bool>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UpdateAzureB2CDetailsCommandHandler> _logger;

        public UpdateAzureB2CDetailsCommandHandler(IMediator mediator, ILogger<UpdateAzureB2CDetailsCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<bool>> Handle(UpdateAzureB2CDetailsCommand request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            try
            {
                var updateData = Json.DeserializeObject<MemberUpdateRequest>((JToken)request.Data);

                bool IsValidEmailId = ValidateEmail.Validate(updateData.NewEmailId);
                if (!IsValidEmailId)
                    return false;

                bool response = await _mediator.Send(new UpdateB2CRepoCommand(updateData), cancellationToken);

                if (response)
                {
                    var publishTasks = new List<Action>
                    {
                        async () => await _mediator.Publish(ManageAccountEventDataMapper.EmailEventDataMapperB2C(updateData.NewEmailId), CancellationToken.None),
                        async () => await _mediator.Publish(ManageAccountEventDataMapper.EmailEventDataMapperB2C(updateData.CurrentEmailId), CancellationToken.None)
                    };

                    Parallel.Invoke(publishTasks.ToArray());
                }
                return response;
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, $"Azure B2C email update failed.");
                return false;
            }
        }
    }
}
