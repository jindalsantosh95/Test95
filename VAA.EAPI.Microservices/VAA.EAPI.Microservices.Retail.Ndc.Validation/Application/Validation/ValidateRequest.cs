using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Application.Query.AgencyValidate;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Constants;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Application.Validation
{
    public class ValidateRequest
    {

        public static Result<Response<string>> RequestValidator(AgencyValidateQuery requestData)
        {
            if (requestData == null) throw new ArgumentNullException(nameof(requestData));

            var errorList = new List<ErrorDetails>();
            var response = new Response<string>
            {
                Status = Status.OK,
                Errors = null
            };

            //environment is missing in request
            if (string.IsNullOrEmpty(requestData.Environment))
                response = GetErrorResponse(errorList, ErrorCodes.MissingEnvironment);

            //subscription key is missing in request
            else if (string.IsNullOrEmpty(requestData.AgentSubscriptionKey) && !requestData.OCNValidation)
                response = GetErrorResponse(errorList, ErrorCodes.MissingSubscriptionKey);

            //missing agencyID or participants check
            else if (requestData.Parties?.Participants == null || requestData.Parties?.Participants?.Count() < 1)
                response = GetErrorResponse(errorList, ErrorCodes.MissingParticipants);

            else if (string.IsNullOrEmpty(requestData.KeyType))
                response = GetErrorResponse(errorList, ErrorCodes.MissingKeyType);

            else if (requestData.Parties.Participants.Any(participant => string.IsNullOrEmpty(participant.AgencyID)))
                response = GetErrorResponse(errorList, ErrorCodes.MissingParticipants);

            else if (!requestData.Parties.Participants.Any(participant => participant.PartyType == Participant.PartyTypeEnum.Seller))
                response = GetErrorResponse(errorList, ErrorCodes.InvalidAgencyType);

            //check length and numeric regex of Agency and Distributor ID
            else if (requestData?.Parties?.Participants?.Any(participant => (!RegexCompiled.Match(participant?.AgencyID, Constants.Application.RegexToValidateNumeric).Success && (participant.AgencyID.Length != 8 || participant.AgencyID.Length != 5)) && errorList.Count == 0) ?? false)
                response = GetErrorResponse(errorList, ErrorCodes.InvalidAgencyID);

            return response;
        }

        public static Result<Response<string>> AgentTicketingAuthorityValidation(AgencyValidateQuery requestData, AgentTktAuthResponse retrieveTktAuthResponse, List<AgencyData> agencyDetails)
        {
            if (requestData == null) throw new ArgumentNullException(nameof(requestData));

            var errorList = new List<ErrorDetails>();
            var response = new Response<string>
            {
                Status = Status.OK,
                Errors = null
            };

            if (retrieveTktAuthResponse.Error != null)
            {
                var airlineAuthorityInd = retrieveTktAuthResponse.IATAObj?.FirstOrDefault()?.AirlineTicketingAuthorityInd;
                if (airlineAuthorityInd == false)
                    response = GetErrorResponse(errorList, ErrorCodes.InvalidAgentTicketAuthorityIndicator);
            }
            else
            {
                response = GetErrorResponse(errorList, ErrorCodes.ErrorFromAgentTicketAuthorityService);
            }

            return response;
        }

        public static Result<Response<string>> AgencyDetailsValidator(AgencyValidateQuery requestData, List<AgencyData> agencyList)
        {
            if (requestData == null) throw new ArgumentNullException(nameof(requestData));

            var errorList = new List<ErrorDetails>();
            var response = new Response<string>
            {
                Status = Status.OK,
                Errors = null
            };

            //Direct Agency check
            if (requestData.Parties?.Participants?.Count() == 1)
            {
                if (requestData.Parties?.Participants?.FirstOrDefault()?.PartyType != Participant.PartyTypeEnum.Seller)
                    response = GetErrorResponse(errorList, ErrorCodes.InvalidAgencyType);

                var requestAgencyID = requestData.Parties?.Participants?.FirstOrDefault()?.AgencyID;

                var agencyDBData = agencyList?.Where(x => x.RequestAgencyIdType == Constants.Application.IATA && x.KeyType == Constants.Application.KeyTypeDirect);

                if (agencyList.Any(x => x.RequestAgencyIdType == "TIDS")) agencyDBData = agencyList?.Where(x => x.RequestAgencyIdType == "TIDS");

                if (agencyDBData == null || agencyDBData.FirstOrDefault()?.KeyStatus != Constants.Application.ActiveStatus)
                    response = GetErrorResponse(errorList, ErrorCodes.InvalidAgencyID);

                //check if corporate
                else if (!string.IsNullOrEmpty(requestData.Parties?.CorporateAccount?.CorporateCode))
                {
                    var requestCorporateID = requestData.Parties?.CorporateAccount?.CorporateCode;

                    if (!agencyDBData.Any(x => x.CorporateId == requestCorporateID))
                    {
                        response = GetErrorResponse(errorList, ErrorCodes.InvalidAgencyCorpID);
                    }
                }

                //if aggregator is present in DB but missing in request
                else if (agencyDBData.First().AgencyId != requestAgencyID)
                    response = GetErrorResponse(errorList, ErrorCodes.InvalidAgencyID);

                // check tkting authority is false on offer price call 
                else if (agencyDBData.First().AirlineTicketingAuthorityInd.Equals(false) && requestData.checkAgentTktAuthority.Equals(true) && agencyDBData.First().BusinessType.Equals(Constants.Application.IATA, StringComparison.CurrentCultureIgnoreCase))
                    response = GetErrorResponse(errorList, ErrorCodes.InvalidAgentTicketAuthorityIndicator);

                // check tkting authority is false for aggregator
                else if (!agencyDBData.First().AirlineTicketingAuthorityInd.GetValueOrDefault() && requestData.checkAgentTktAuthority && agencyDBData.First().BusinessType.Equals(Constants.Application.AGGREGATOR, StringComparison.CurrentCultureIgnoreCase))
                    response = GetErrorResponse(errorList, ErrorCodes.InvalidAgentTicketAuthorityIndicator);

            }

            else //aggregator request validation
            {
                //check on party type
                if (!requestData.Parties.Participants.Any(participant => participant.PartyType == Participant.PartyTypeEnum.Seller) || !requestData.Parties.Participants.Any(participant => participant.PartyType == Participant.PartyTypeEnum.Distributor))
                    response = GetErrorResponse(errorList, ErrorCodes.InvalidAgencyType);

                var requestAggregatorID = requestData.Parties?.Participants?.FirstOrDefault(participant => participant.PartyType == Participant.PartyTypeEnum.Distributor)?.AgencyID;
                var requestSellerID = requestData.Parties?.Participants?.FirstOrDefault(participant => participant.PartyType == Participant.PartyTypeEnum.Seller)?.AgencyID;

                var agencyDBData = agencyList?.Where(x => x.BusinessType == Constants.Application.AGGREGATOR || x.BusinessType == Constants.Application.IATA || x.BusinessType == Constants.Application.TidsBusinessType);

                if (agencyList?.Any(x => x.AgencyIdType == Constants.Application.TtcAgencyType) ?? false)
                {
                    agencyDBData = agencyList?.Where(x => x.AgencyId == requestAggregatorID || ((x.AgencyId == requestSellerID && x.AgencyIdType == Constants.Application.IATA) && x.KeyType == "INDIRECT"));
                }

                var agency = agencyDBData?.FirstOrDefault(x => x.RequestAgencyId == requestSellerID);

                if (agencyDBData == null || !agencyDBData.Any() || agency is null || agencyDBData.Any(x => x.KeyStatus != Constants.Application.ActiveStatus))
                    response = GetErrorResponse(errorList, ErrorCodes.InvalidAgencyID);

                // check tkting authority is false on offer price call 

                else if (agency is not null && agency.AirlineTicketingAuthorityInd == false && requestData.checkAgentTktAuthority && string.Equals(agency.RequestAgencyIdType, Constants.Application.IATA, StringComparison.CurrentCultureIgnoreCase))
                    response = GetErrorResponse(errorList, ErrorCodes.InvalidAgentTicketAuthorityIndicator);

                //check if corporate
                else if (!string.IsNullOrEmpty(requestData?.Parties?.CorporateAccount?.CorporateCode))
                {
                    var requestCorporateID = requestData.Parties?.CorporateAccount?.CorporateCode;

                    if (agencyDBData.First().CorporateId != requestCorporateID)
                        response = GetErrorResponse(errorList, ErrorCodes.InvalidAgencyCorpID);
                }

                else if (!agencyDBData.Any(x => x.AgencyId == requestAggregatorID))
                    response = GetErrorResponse(errorList, ErrorCodes.InvalidAgencyAggeregatorID);

            }

            return response;
        }
        public static Result<Response<string>> SubscriptionDetailsValidator(AgencyValidateQuery requestData, SubscriptionResponse retrieveSubscriptionResponse)
        {
            if (requestData == null) throw new ArgumentNullException(nameof(requestData));

            var errorList = new List<ErrorDetails>();
            var response = new Response<string>
            {
                Status = Status.OK,
                Errors = null
            };

            //check response from securityMS for any error
            if (retrieveSubscriptionResponse?.Error != null || retrieveSubscriptionResponse?.Error?.Code != null)
                response = GetErrorResponse(errorList, ErrorCodes.InvalidSubscriptionKey);

            //if subscription Key is disabled
            else if (retrieveSubscriptionResponse?.SubscriptionState != null
                     && !retrieveSubscriptionResponse.SubscriptionState.Is(
                         Constants.Application.EnabledStatus))
                response = GetErrorResponse(errorList, ErrorCodes.InvalidSubscriptionKey);

            //if subscription key is different
            else if (retrieveSubscriptionResponse?.SubscriptionKeys?.FirstOrDefault()?.Key != requestData.AgentSubscriptionKey)
                response = GetErrorResponse(errorList, ErrorCodes.InvalidSubscriptionKey);

            return response;
        }


        private static Response<string> GetErrorResponse(List<ErrorDetails> errorList, string errorCode)
        {
            errorList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(errorCode), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());

            return new Response<string>
            {
                Status = Status.BusinessException,
                Errors = errorList
            };
        }
    }
}
