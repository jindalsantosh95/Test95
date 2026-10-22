using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.CustomerDetails;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Interface.IRepository.DuplicateCheck;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Helper;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.DuplicateCheck.Entities;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.DuplicateCheck.Repository
{
    public class DeDupValidationRepository : IDeDupRepository
    {
        public readonly ILogger<DeDupValidationRepository> _logger;
        private readonly IMediator _mediator;

        public DeDupValidationRepository(ILogger<DeDupValidationRepository> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<string> GetCustomerDetailsWithMatchString(EnrolmentRequest enrolmentRequest)
        {
            List<KeyValuePair<string, object>> parameters = new()
            {
                new KeyValuePair<string, object>("matchString", MatchString.GetMatchString(enrolmentRequest))
            };
            var customerSearchResponse = await _mediator.Send(CosmosQueryHandlerMapper.Mapper(ConfigReader.Get(Constants.Environment.SCVCustDBCustSearchColCntnr), ConfigReader.Get(Constants.Environment.SCVCustDBEndPoint), ConfigReader.Get(Constants.Environment.SCVCustDB), parameters, Constants.Application.MatchStringSqlQuery));
            if (customerSearchResponse.Count > 0)
            {
                var doc = Json.DeserializeObject<CustomerSearch>((JToken)customerSearchResponse.FirstOrDefault());
                return doc.LoyaltyMemberNumber;
            }

            return string.Empty;
        }

        public async Task<string> GetCustomerDetailsWithEmail(EnrolmentRequest enrolmentRequest)
        {
            string EmailId = enrolmentRequest.CustomerDetails.ContactDetails.EmailDetails.FirstOrDefault().EmailId.ToLower();
            List<KeyValuePair<string, object>> parameters = new()
            {
                new KeyValuePair<string, object>("emailId", EmailId)
            };
            var customerSearchResponse = await _mediator.Send(CosmosQueryHandlerMapper.Mapper(ConfigReader.Get(Constants.Environment.SCVCustDBCustSearchColCntnr), ConfigReader.Get(Constants.Environment.SCVCustDBEndPoint), ConfigReader.Get(Constants.Environment.SCVCustDB), parameters, Constants.Application.EmailIdSqlQuery));

            if (customerSearchResponse.Count > 0 && !string.IsNullOrEmpty(EmailId))
            {
                var doc = Json.DeserializeObject<CustomerSearch>((JToken)customerSearchResponse.FirstOrDefault());
                return enrolmentRequest.CustomerDetails.CustomerType == Domain.CustomerDetails.Customer.CustomerTypeEnum.Loyalty || enrolmentRequest.CustomerDetails.LoyaltyMember.EnrollmentChannel == ConfigReader.Get(Constants.Environment.WebSiteChannelId) ? doc.LoyaltyMemberNumber : doc.CustomerId;
            }
            return string.Empty;
        }

        public async Task<string> GetCustomerDetailsWithEmailAndMatchString(EnrolmentRequest enrolmentRequest)
        {
            string matchstring = MatchString.GetMatchString(enrolmentRequest);
            string EmailId = enrolmentRequest.CustomerDetails.ContactDetails.EmailDetails.FirstOrDefault().EmailId.ToLower();
            List<KeyValuePair<string, object>> parameters = new()
            {
                new KeyValuePair<string, object>("emailId", EmailId),
                new KeyValuePair<string, object>("matchString", matchstring)

            };
            var customerSearchResponse = await _mediator.Send(CosmosQueryHandlerMapper.Mapper(ConfigReader.Get(Constants.Environment.SCVCustDBCustSearchColCntnr), ConfigReader.Get(Constants.Environment.SCVCustDBEndPoint), ConfigReader.Get(Constants.Environment.SCVCustDB), parameters, Constants.Application.MatchStringAndEmailIdSqlQuery));
            if (customerSearchResponse.Count > 0)
            {
                var doc = Json.DeserializeObject<CustomerSearch>((JToken)customerSearchResponse.FirstOrDefault());
                return doc.LoyaltyMemberNumber;
            }
            return string.Empty;
        }

        public async Task<List<Entities.Customer>> GetVredDetails(EnrolmentRequest enrolmentRequest)
        {
            List<KeyValuePair<string, object>> parameters = new()
            {
                new KeyValuePair<string, object>("firstName", enrolmentRequest?.CustomerDetails?.FirstName),
                new KeyValuePair<string, object>("lastName", enrolmentRequest?.CustomerDetails?.LastName),
                new KeyValuePair<string, object>("dateOfBirth", DateTimeHelper.ConvertDateTimeFormat(enrolmentRequest?.CustomerDetails?.DateOfBirth, Constants.Application.EAPIEnrolmentDOBFormat, Constants.Application.EAPIEnrolmentDOBFormat)?.ToString(Constants.Application.EAPIEnrolmentDOBFormat)),
                new KeyValuePair<string, object>("province", enrolmentRequest?.CustomerDetails?.AddressDetails.FirstOrDefault().Region)
            };
            var customerSearchResponse = await _mediator.Send(CosmosQueryHandlerMapper.Mapper(ConfigReader.Get(Constants.Environment.SCVCustDBCustColCntnr), ConfigReader.Get(Constants.Environment.SCVCustDBEndPoint), ConfigReader.Get(Constants.Environment.SCVCustDB), parameters, Constants.Application.VredDupCheckSqlQuery));
            if (customerSearchResponse.Count > 0)
            {
                var documents = customerSearchResponse.ConvertAll(x => Json.DeserializeObject<Entities.Customer>((JToken)x));
                return documents;
            }
            return null;
        }
    }
}
