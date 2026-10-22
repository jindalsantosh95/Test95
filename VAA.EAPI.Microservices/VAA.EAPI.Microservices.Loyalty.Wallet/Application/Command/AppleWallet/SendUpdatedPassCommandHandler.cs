using Gremlin.Net.Process.Traversal;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Wallet.Domain;
using VAA.EAPI.Microservices.Loyalty.Wallet.Infrastructure.Repository;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Application.Command.AppleWallet
{
    public class SendUpdatedPassCommandHandler : IRequestHandler<SendUpdatedPassCommand, Response<byte[]>>
    {
        private readonly IMediator _mediator;
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<SendUpdatedPassCommandHandler> _logger;

        public SendUpdatedPassCommandHandler(IMediator mediator, IOAuthTokenProvider tokenProvider, ILogger<SendUpdatedPassCommandHandler> logger, IHttpConnector httpConnector)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        public async Task<Response<byte[]>> Handle(SendUpdatedPassCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var customerResponse = await _httpConnector.GetAsync<CustomerResponse>($"{ConfigReader.Get(Constants.Environment.CustomerMsUrl)}{Constants.Application.GetCustomerDtlsByMemberID}".Replace("{memberid}", request.FcNumber), GetHeaders(), await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Constants.Environment.CustomerAppId)));

                var customerFcmDetails = await _httpConnector.GetAsync<FcMemberDetails>($"{ConfigReader.Get(Constants.Environment.CustomerTransUrl)}{Constants.Application.GetFcMemberDetails}".Replace("{customerId}", customerResponse.Customer.CustomerId), GetHeaders(), await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Constants.Environment.CustomerTransAppId)));

                string memberName = $"{customerResponse.Customer.PreferredFirstName} {customerResponse.Customer.PreferredLastName}";
                string validUntil = customerFcmDetails.FlyingClubMemberDocuments.First().memberShipCardDetails.ExpiryDate != null ? DateTime.Parse(customerFcmDetails.FlyingClubMemberDocuments.First().memberShipCardDetails.ExpiryDate).ToString(Constants.Application.ValidUntilDateFormat) : string.Empty;

                return new Response<byte[]>
                {
                    Status = Status.OK,
                    Value = await _mediator.Send(new GeneratePassRepoCommand() { FcNumber = request.FcNumber, MemberName = memberName, TierCode = customerResponse.Customer.MemberTier, ValidUntil = validUntil }),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while sending updated passes for FcNumber: {request.FcNumber} with error {ex.Message}");

                return new Response<byte[]>()
                {
                    Status = Status.BusinessException,
                    ErrorMessage = ex.Message
                };
            }

        }

        private static Dictionary<string, string> GetHeaders()
        {
            return new Dictionary<string, string>
            {
                {   Constants.Application.OcpKeyHeader , ConfigReader.Get(Constants.Environment.EapiOcpSubId) }
            };
        }
    }
}
