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
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.IRepository;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories
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

        public async Task<string> GetCustomerDetailsWithMatchString(CreateCustomerEntities enrolmentRequest)
        {
            List<KeyValuePair<string, object>> parameters = new()
            {
                new KeyValuePair<string, object>("matchString", MatchString.GetMatchString(enrolmentRequest))
            };
            var customerSearchResponse = await _mediator.Send(CosmosQueryHandlerMapper.Mapper(ConfigReader.Get(Constants.Environment.SCVCustDBCustSearchColCntnr), ConfigReader.Get(Constants.Environment.SCVCustDBEndPoint), ConfigReader.Get(Constants.Environment.SCVCustDB), parameters, Constants.Application.MatchStringSqlQuery));
            if (customerSearchResponse.Any())
            {
                return Json.DeserializeObject<CustomerSearch>((JToken)customerSearchResponse.FirstOrDefault()).LoyaltyMemberNumber;

            }

            return string.Empty;
        }

        public async Task<string> GetCustomerDetailsWithEmail(CreateCustomerEntities enrolmentRequest)
        {
            string EmailId = enrolmentRequest.ContactDetails.EmailDetails.FirstOrDefault().EmailId.ToLower();
            List<KeyValuePair<string, object>> parameters = new()
            {
                new KeyValuePair<string, object>("emailId", EmailId)
            };
            var customerSearchResponse = await _mediator.Send(CosmosQueryHandlerMapper.Mapper(ConfigReader.Get(Constants.Environment.SCVCustDBCustSearchColCntnr), ConfigReader.Get(Constants.Environment.SCVCustDBEndPoint), ConfigReader.Get(Constants.Environment.SCVCustDB), parameters, Constants.Application.EmailIdSqlQuery));

            if (customerSearchResponse.Any() && !string.IsNullOrEmpty(EmailId))
            {
                var doc = Json.DeserializeObject<CustomerSearch>((JToken)customerSearchResponse.FirstOrDefault());
                return enrolmentRequest.CustomerType.ToString().Equals(CustomerTypeEnum.Loyalty) || enrolmentRequest.EnrollmentChannel == ConfigReader.Get(Constants.Environment.WebSiteChannelId) ? doc.LoyaltyMemberNumber : doc.CustomerId;
            }
            return string.Empty;
        }

        public async Task<string> GetCustomerDetailsWithEmailAndMatchString(CreateCustomerEntities enrolmentRequest)
        {
            string matchstring = MatchString.GetMatchString(enrolmentRequest);
            string EmailId = enrolmentRequest.ContactDetails.EmailDetails.FirstOrDefault().EmailId.ToLower();
            List<KeyValuePair<string, object>> parameters = new()
            {
                new KeyValuePair<string, object>("emailId", EmailId),
                new KeyValuePair<string, object>("matchString", matchstring)

            };
            var customerSearchResponse = await _mediator.Send(CosmosQueryHandlerMapper.Mapper(ConfigReader.Get(Constants.Environment.SCVCustDBCustSearchColCntnr), ConfigReader.Get(Constants.Environment.SCVCustDBEndPoint), ConfigReader.Get(Constants.Environment.SCVCustDB), parameters, Constants.Application.MatchStringAndEmailIdSqlQuery));
            if (customerSearchResponse.Any())
            {
                return Json.DeserializeObject<CustomerSearch>((JToken)customerSearchResponse.FirstOrDefault()).LoyaltyMemberNumber;
            }
            return string.Empty;
        }

        public async Task<List<CreateCustomerEntities>> GetVredDetails(CreateCustomerEntities enrolmentRequest)
        {
            List<KeyValuePair<string, object>> parameters = new()
            {
                new KeyValuePair<string, object>("firstName", enrolmentRequest?.FirstName),
                new KeyValuePair<string, object>("lastName", enrolmentRequest?.LastName),
                new KeyValuePair<string, object>("dateOfBirth", DateTimeHelper.ConvertDateTimeFormat(enrolmentRequest?.DateOfBirth, Constants.Application.EAPIEnrolmentDOBFormat, Constants.Application.EAPIEnrolmentDOBFormat)?.ToString(Constants.Application.EAPIEnrolmentDOBFormat)),
                new KeyValuePair<string, object>("province", enrolmentRequest?.AddressDetails.FirstOrDefault().Region)
            };
            var customerSearchResponse = await _mediator.Send(CosmosQueryHandlerMapper.Mapper(ConfigReader.Get(Constants.Environment.SCVCustDBCustColCntnr), ConfigReader.Get(Constants.Environment.SCVCustDBEndPoint), ConfigReader.Get(Constants.Environment.SCVCustDB), parameters, Constants.Application.VredDupCheckSqlQuery));
            if (customerSearchResponse.Any())
            {
                return customerSearchResponse.ConvertAll(x => Json.DeserializeObject<CreateCustomerEntities>((JToken)x));
            }
            return null;
        }
    }
}
