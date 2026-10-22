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
using VAA.EAPI.Microservices.Loyalty.Customer.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Enum;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IRepository.DuplicateCheck;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.CheckEmailAndTier.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.DuplicateCheck.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.CheckEmailAndTier
{
    public class CheckEmailAndTierHandler : IRequestHandler<CheckEmailAndTierQuery, bool>
    {
        private readonly IMediator _mediator;
        private readonly IDeDupRepositoryEnrolment _deduprepository;
        private readonly ILogger<CheckEmailAndTierHandler> _logger;

        public CheckEmailAndTierHandler(IMediator mediator, IDeDupRepositoryEnrolment deDupRepository, ILogger<CheckEmailAndTierHandler> logger)
        {
            _mediator = mediator;
            _deduprepository = deDupRepository;
            _logger = logger;
        }
        public async Task<bool> Handle(CheckEmailAndTierQuery request, CancellationToken cancellationToken)
        {
            var customerDtls = await _deduprepository.GetCustomersWithEmail(request.EnrolmentRequest);

            if (customerDtls != null)
            {
                List<KeyValuePair<string, object>> keyValues = new()
                {
                    new KeyValuePair<string, object>("partitionKey", customerDtls.FirstOrDefault().CustomerId),
                    new KeyValuePair<string, object>("id", customerDtls.FirstOrDefault().LoyaltyMemberNumber),
                    new KeyValuePair<string, object>("docType", DocumentType.FlyingClubMember.ToString())
                };
                var memberProfileResponse = await _mediator.Send(CosmosQueryHandlerMapper.Mapper(ConfigReader.Get(Environment.SCVCustTxnDBMmbrPrfleContnr), ConfigReader.Get(Environment.SCVCustTxnDBEndPoint), ConfigReader.Get(Environment.SCVCustTxnDB), keyValues, Constants.Application.MemberPrfDbQuery), cancellationToken);
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
