using MediatR;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IRepository.DuplicateCheck;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper;
using Environment = VAA.EAPI.Microservices.Loyalty.Customer.Constants.Environment;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.CheckEmailAndTier
{
    public class CheckEmailQueryHandler : IRequestHandler<CheckEmailQuery, VacEmailCheckResponse>
    {

        private readonly IMediator _mediator;
        private readonly IDeDupRepositoryEnrolment _deduprepository;

        public CheckEmailQueryHandler(IMediator mediator, IDeDupRepositoryEnrolment deDupRepository)
        {
            _mediator = mediator;
            _deduprepository = deDupRepository;
        }

        public async Task<VacEmailCheckResponse> Handle(CheckEmailQuery request, CancellationToken cancellationToken)
        {
            var customerDetails = await _deduprepository.GetCustomersDetailsWithEmail(request.Email.ToLower());

            VacEmailCheckResponse response = new();
            if (customerDetails != null)
            {

                response.CustomerId = customerDetails.First().CustomerId;
                response.FcNumber = customerDetails.First().LoyaltyMemberNumber;

                List<KeyValuePair<string, object>> keyValues = new()
                {
                    new KeyValuePair<string, object>("partitionKey", customerDetails.First().CustomerId),
                    new KeyValuePair<string, object>("id", customerDetails.First().LoyaltyMemberNumber),
                    new KeyValuePair<string, object>("docType", DocumentType.FlyingClubMember.ToString())
                };

                var memberProfileResponse = await _mediator.Send(CosmosQueryHandlerMapper.Mapper(ConfigReader.Get(Environment.SCVCustTxnDBMmbrPrfleContnr), ConfigReader.Get(Environment.SCVCustTxnDBEndPoint), ConfigReader.Get(Environment.SCVCustTxnDB), keyValues, Constants.Application.MemberPrfDbQuery), cancellationToken);

                if (memberProfileResponse.Count > 0)
                {
                    var memberProfile = Json.DeserializeObject<MemberProfile>((JToken)memberProfileResponse.First());
                    response.Tier = memberProfile?.TierCode;

                    if ((memberProfile?.TierCode).Is(TierCode.Glass.ToString()))
                    {
                        response.isBasic = true;
                    }
                }
            }
            return response;
        }
    }
}
