using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Security.Infrastructure.Helper;
using VAA.EAPI.Microservices.Security.Infrastructure.Mapper.APIMUser;
using VAA.EAPI.Microservices.Security.Infrastructure.Models.APIMUserResponse;
using VAA.EAPI.Microservices.Security.Infrastructure.Services.APIMUser;


namespace VAA.EAPI.Microservices.Security.Infrastructure.Repositories.APIMUser
{
    public class CreateAPIMUserRepoCommandHandler : IRequestHandler<CreateAPIMUserRepoCommand, string>
    {

        private readonly IMediator _mediator;
        private readonly ILogger<CreateAPIMUserRepoCommandHandler> _logger;

        public CreateAPIMUserRepoCommandHandler(IMediator mediator, ILogger<CreateAPIMUserRepoCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        public async Task<string> Handle(CreateAPIMUserRepoCommand request, CancellationToken cancellationToken)
        {

            var config = APIMSubscriptionConfigHelper.GetAPIMConfiguration(request.request.Channel, request.request.Environment);

            var apimUserId = string.Empty;
            string userID = Regex.Replace(request.request.UserAccount.EmailAddress, "\\W", "-");
            var userCreationRequest = APIMUserCreationRequestMapper.MapToMSAPIMUserRequest(request.request.UserAccount);
            string createApimUserUri = APIMSubscriptionConfigHelper.GetCreateAPIMUserURI(config, userID);

            var serviceResponse = await _mediator.Send(new CreateAPIMUserServiceCommand(userCreationRequest, createApimUserUri), cancellationToken);

            if (!serviceResponse.Key)
            {
            }
            if (serviceResponse.Value != null)
            {
                var userCreationResponse = Json.Deserialize<UserCreationResponse>(serviceResponse.Value);
                apimUserId = userCreationResponse?.Id;

            }
            return apimUserId;
        }

    }
}
