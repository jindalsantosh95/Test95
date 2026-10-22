using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Helper;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query
{
    public class FCMDocBySSMemberNumberServiceQueryHandler : IRequestHandler<FCMDocBySSMemberNumberServiceQuery, Result<Response<FlyingClubMember>>>
    {
        private readonly IHttpConnector _apiConnector;
        private readonly ILogger<FCMDocBySSMemberNumberServiceQueryHandler> _logger;

        public FCMDocBySSMemberNumberServiceQueryHandler(IHttpConnector apiConnector,
            ILogger<FCMDocBySSMemberNumberServiceQueryHandler> logger)
        {
            _apiConnector = apiConnector;
            _logger = logger;
        }

        public async Task<Result<Response<FlyingClubMember>>> Handle(FCMDocBySSMemberNumberServiceQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                string endpoint = ConfigReader.Get(Constants.Environment.CustomerTxnMSDetailsEndpoint).Replace("{1}", request.SSAMemberNumber).Replace("{0}", Constants.Application.FetchFCMDocBySSAMemberId);
                string token = await ConfigurationHelper.GetBearerToken();
                var headers = ConfigurationHelper.GetHeader();

                var result = await _apiConnector.GetAsync<FlyingClubMemberResponse>(uri: endpoint, headers: headers, bearerToken: token);

                return new Response<FlyingClubMember>
                {
                    Status = Status.OK,
                    Value = result.FlyingClubMemberDocuments.FirstOrDefault()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error has occured in Customer Transaction MS method FetchFlyingClubMemberDocumentBySSAMemberNumberAsync call with SSAMemberNumber id:{request.SSAMemberNumber}");
                throw;
            }
        }
    }
}

