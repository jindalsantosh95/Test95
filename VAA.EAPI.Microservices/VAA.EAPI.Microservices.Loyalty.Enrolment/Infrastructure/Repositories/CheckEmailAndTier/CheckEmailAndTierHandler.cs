using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Enum;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Interface.IRepository.DuplicateCheck;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.CheckEmailAndTier.Entities;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.DuplicateCheck.Entities;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.CheckEmailAndTier
{
    public class CheckEmailAndTierHandler : IRequestHandler<CheckEmailAndTierQuery, bool>
    {
        private readonly IMediator _mediator;
        private readonly IDeDupRepository _deduprepository;
        private readonly ILogger<CheckEmailAndTierHandler> _logger;

        public CheckEmailAndTierHandler(IMediator mediator, IDeDupRepository deDupRepository, ILogger<CheckEmailAndTierHandler> logger)
        {
            _mediator = mediator;
            _deduprepository = deDupRepository;
            _logger = logger;
        }
        public async Task<bool> Handle(CheckEmailAndTierQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"CheckEmailAndTierHandler - Start");
            var fcNumber = await _deduprepository.GetCustomerDetailsWithEmail(request.EnrolmentRequest);

            if (!string.IsNullOrEmpty(fcNumber))
            {
                List<KeyValuePair<string, object>> keyValues = new()
                {
                    new KeyValuePair<string, object>("id", fcNumber),
                    new KeyValuePair<string, object>("docType", DocumentType.FlyingClubMember.ToString())
                };
                var memberProfileResponse = await _mediator.Send(CosmosQueryHandlerMapper.Mapper(ConfigReader.Get(Constants.Environment.SCVCustTxnDBMmbrPrfleContnr), ConfigReader.Get(Constants.Environment.SCVCustTxnDBEndPoint), ConfigReader.Get(Constants.Environment.SCVCustTxnDB), keyValues, Constants.Application.SearchDbQuery), cancellationToken);
                if (memberProfileResponse.Count > 0)
                {
                    var memberProfile = Json.DeserializeObject<MemberProfile>((JToken)memberProfileResponse.FirstOrDefault());
                    if (memberProfile.TierCode == TierCode.Glass.ToString())
                    {
                        request.CreateCustomerEntities.CustomerId = memberProfile.CustomerId;
                        request.CreateCustomerEntities.FcNumber = memberProfile.Id;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
