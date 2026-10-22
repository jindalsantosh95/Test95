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
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.Enrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IRepository.DuplicateCheck;
using VAA.EAPI.Microservices.Loyalty.Customer.Helper.Enrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.DuplicateCheck.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.DuplicateCheck.Repository
{
    public class DeDupValidationRepositoryEnrolment : IDeDupRepositoryEnrolment
    {
        public readonly ILogger<DeDupValidationRepositoryEnrolment> _logger;
        private readonly IMediator _mediator;

        public DeDupValidationRepositoryEnrolment(ILogger<DeDupValidationRepositoryEnrolment> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<string> GetCustomerDetailsWithMatchString(EnrolmentRequestCommand enrolmentRequest)
        {
            List<KeyValuePair<string, object>> parameters = new()
            {
                new KeyValuePair<string, object>("matchString", MatchString.GetMatchString(enrolmentRequest)),
                new KeyValuePair<string, object>("partitionKey", Constants.Application.CustomerSearchDocType)
            };

            var customerSearchResponse = await _mediator.Send(CosmosQueryHandlerMapper.Mapper(ConfigReader.Get(Constants.Environment.SCVCustDBCustSearchColCntnrTest), ConfigReader.Get(Constants.Environment.SCVCustDBEndPoint), ConfigReader.Get(Constants.Environment.SCVCustDB), parameters, Constants.Application.MatchStringSqlQuery));

            if (customerSearchResponse.Count > 0)
            {
                return Json.DeserializeObject<CustomerSearch>((JToken)customerSearchResponse.FirstOrDefault()).LoyaltyMemberNumber;
            }

            return string.Empty;
        }

        public async Task<string> GetCustomerDetailsWithEmail(EnrolmentRequestCommand enrolmentRequest)
        {
            string emailId = enrolmentRequest?.Data?.CustomerDetails.ContactDetails.EmailDetails.FirstOrDefault().EmailId.ToLower();

            List<KeyValuePair<string, object>> parameters = new()
            {
                new KeyValuePair<string, object>("partitionKey", Constants.Application.CustomerSearchDocType),
                new KeyValuePair<string, object>("emailId", emailId)
            };

            var customerSearchResponse = await _mediator.Send(CosmosQueryHandlerMapper.Mapper(ConfigReader.Get(Constants.Environment.SCVCustDBCustSearchColCntnrTest), ConfigReader.Get(Constants.Environment.SCVCustDBEndPoint), ConfigReader.Get(Constants.Environment.SCVCustDB), parameters, Constants.Application.EmailIdSqlQuery));

            if (customerSearchResponse.Count > 0 && !string.IsNullOrEmpty(emailId))
            {
                return Json.DeserializeObject<CustomerSearch>((JToken)customerSearchResponse.FirstOrDefault()).LoyaltyMemberNumber;
            }

            return string.Empty;
        }

        public async Task<List<Entities.Customer>> GetCustomersWithEmail(EnrolmentRequestCommand enrolmentRequest)
        {
            string emailId = enrolmentRequest?.Data?.CustomerDetails.ContactDetails.EmailDetails.FirstOrDefault().EmailId.ToLower();

            List<KeyValuePair<string, object>> parameters = new()
            {
                new KeyValuePair<string, object>("partitionKey", Constants.Application.CustomerSearchDocType),
                new KeyValuePair<string, object>("emailId", emailId)
            };

            var customerSearchResponse = await _mediator.Send(CosmosQueryHandlerMapper.Mapper(ConfigReader.Get(Constants.Environment.SCVCustDBCustSearchColCntnrTest), ConfigReader.Get(Constants.Environment.SCVCustDBEndPoint), ConfigReader.Get(Constants.Environment.SCVCustDB), parameters, Constants.Application.EmailIdSqlQuery));

            if (customerSearchResponse.Count > 0)
            {
                return customerSearchResponse.ConvertAll(x => Json.DeserializeObject<Entities.Customer>((JToken)x));
            }

            return null;
        }
        public async Task<List<Entities.Customer>> GetCustomerDetailWithFcMember(string LoyaltyMemberNumber)
        {

            List<KeyValuePair<string, object>> parameters = new()
            {
                new KeyValuePair<string, object>("LoyaltyMemberNumber", LoyaltyMemberNumber),
                new KeyValuePair<string, object>("DocType", DocumentType.Customer.ToString()),
            };

            var customerSearchResponse = await _mediator.Send(CosmosQueryHandlerMapper.Mapper(ConfigReader.Get(Constants.Environment.SCVCustDBCustSearchColCntnr),
                                                               ConfigReader.Get(Constants.Environment.SCVCustDBEndPoint)
                                                              , ConfigReader.Get(Constants.Environment.SCVCustDB)
                                                              , parameters
                                                              , Constants.Application.FcNumberSqlQuery));

            if (customerSearchResponse.Any())
            {
                return customerSearchResponse.ConvertAll(x => Json.DeserializeObject<Entities.Customer>((JToken)x));
            }
            return null;
        }
        public async Task<string> GetCustomerDetailsWithEmailAndMatchString(EnrolmentRequestCommand enrolmentRequest)
        {
            string matchstring = MatchString.GetMatchString(enrolmentRequest);
            string emailId = enrolmentRequest?.Data?.CustomerDetails.ContactDetails.EmailDetails.First().EmailId.ToLower();

            List<KeyValuePair<string, object>> parameters = new()
            {
                new KeyValuePair<string, object>("emailId", emailId),
                new KeyValuePair<string, object>("matchString", matchstring),
                new KeyValuePair<string, object>("partitionKey", Constants.Application.CustomerSearchDocType)
            };

            var customerSearchResponse = await _mediator.Send(CosmosQueryHandlerMapper.Mapper(ConfigReader.Get(Constants.Environment.SCVCustDBCustSearchColCntnrTest)
                                                              , ConfigReader.Get(Constants.Environment.SCVCustDBEndPoint)
                                                              , ConfigReader.Get(Constants.Environment.SCVCustDB)
                                                              , parameters
                                                              , Constants.Application.MatchStringAndEmailIdSqlQuery));

            if (customerSearchResponse.Count > 0)
            {
                return Json.DeserializeObject<CustomerSearch>((JToken)customerSearchResponse.First()).LoyaltyMemberNumber;
            }

            return string.Empty;
        }

        public async Task<string> GetVredDetails(EnrolmentRequestCommand enrolmentRequest)
        {
            string matchstring = MatchString.GetMatchString(enrolmentRequest);

            List<KeyValuePair<string, object>> parameters = new()
            {
                new KeyValuePair<string, object>("province", enrolmentRequest?.Data?.CustomerDetails?.AddressDetails.FirstOrDefault().Region),
                new KeyValuePair<string, object>("matchString", matchstring),
                new KeyValuePair<string, object>("partitionKey", Constants.Application.CustomerSearchDocType)
            };

            var customerSearchResponse = await _mediator.Send(CosmosQueryHandlerMapper.Mapper(ConfigReader.Get(Constants.Environment.SCVCustDBCustSearchColCntnrTest),
                                                               ConfigReader.Get(Constants.Environment.SCVCustDBEndPoint)
                                                              , ConfigReader.Get(Constants.Environment.SCVCustDB)
                                                              , parameters
                                                              , Constants.Application.VredDupCheckSqlQuery));

            if (customerSearchResponse.Count > 0)
            {
                return Json.DeserializeObject<CustomerSearch>((JToken)customerSearchResponse.First()).LoyaltyMemberNumber;
            }

            return string.Empty;
        }

        public async Task<string> GetCustomersWithEmail(string emailId)
        {
            List<KeyValuePair<string, object>> parameters = new()
            {
                new KeyValuePair<string, object>("partitionKey", Constants.Application.CustomerSearchDocType),
                new KeyValuePair<string, object>("emailId", emailId)
            };

            var customerSearchResponse = await _mediator.Send(CosmosQueryHandlerMapper.Mapper(ConfigReader.Get(Constants.Environment.SCVCustDBCustSearchColCntnrTest), ConfigReader.Get(Constants.Environment.SCVCustDBEndPoint), ConfigReader.Get(Constants.Environment.SCVCustDB), parameters, Constants.Application.EmailIdSqlQuery));

            if (customerSearchResponse.Count > 0)
            {
                return Json.DeserializeObject<CustomerSearch>((JToken)customerSearchResponse.First()).LoyaltyMemberNumber;
            }

            return string.Empty;
        }

        public async Task<List<Entities.Customer>> GetCustomersDetailsWithEmail(string emailId)
        {            
            List<KeyValuePair<string, object>> parameters = new()
            {
                new KeyValuePair<string, object>("partitionKey", Constants.Application.CustomerSearchDocType),
                new KeyValuePair<string, object>("emailId", emailId)
            };

            var customerSearchResponse = await _mediator.Send(CosmosQueryHandlerMapper.Mapper(ConfigReader.Get(Constants.Environment.SCVCustDBCustSearchColCntnrTest), ConfigReader.Get(Constants.Environment.SCVCustDBEndPoint), ConfigReader.Get(Constants.Environment.SCVCustDB), parameters, Constants.Application.EmailIdSqlQuery));

            if (customerSearchResponse.Count > 0)
            {
                return customerSearchResponse.ConvertAll(x => Json.DeserializeObject<Entities.Customer>((JToken)x));
            }

            return null;
        }
    }
}
