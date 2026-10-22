using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Customer;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.MillionMiler;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.HoHAccount;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Validations
{
    public class HouseHoldAccountInviteMemberValidator
    {
        public static HouseHoldAccountValidationResponse MapInviteMemberValidator(CustomerDTO individualCustomer, CustomerDTO leaderCustomer, FlyingClubMember individualFlyingClubMember)
        {
            var errorDetailsList= new List<ErrorDetails>();

            if (individualFlyingClubMember != null)
            {
                if (individualFlyingClubMember.MemberType == Constants.Application.HouseholdGroupTypeCode || (individualFlyingClubMember.MemberType == Constants.Application.FCMemberTypeIndividual && !string.IsNullOrEmpty(individualFlyingClubMember.HeadOfGroupMemberNumber)) )
                {
                    errorDetailsList.Add(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.ExistingMemberInvite, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                }
            }

            if (leaderCustomer?.Customer != null && individualCustomer?.Customer != null)
            {
                if (leaderCustomer.Customer.Status != Constants.Application.FCMemberActiveStatus)
                {
                    errorDetailsList.Add(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.FCMemberAccountNotActive), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());

                    return new HouseHoldAccountValidationResponse
                    {
                        ErrorDetails = errorDetailsList
                    };
                }

                if (!(individualCustomer.Customer.Status == Constants.Application.FCMemberActiveStatus || individualCustomer.Customer.Status == Constants.Application.FCMemberActivePendingStatus))
                {
                    errorDetailsList.Add(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.InvalidMemberStatus, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                }

                var individualAddressDetails = individualCustomer.Customer.AddressDetails.Where(a => a.PreferredFlag == "Y").FirstOrDefault();
                var leaderAddressDetails = leaderCustomer.Customer.AddressDetails.Where(a => a.PreferredFlag == "Y").FirstOrDefault();

                var individualAddressLine = RemoveSpecialCharandTrim(individualAddressDetails.AddressLine1).ToLower();
                var leaderAddressLine = RemoveSpecialCharandTrim(leaderAddressDetails.AddressLine1).ToLower();
                var individualPostCode = RemoveSpecialCharandTrim(individualAddressDetails.PostalCode);
                var leaderPostCode = RemoveSpecialCharandTrim(leaderAddressDetails.PostalCode);
                var individualCity = RemoveSpecialCharandTrim(individualAddressDetails.City).ToLower();
                var leaderCity = RemoveSpecialCharandTrim(leaderAddressDetails.City).ToLower();
                var leaderCountry = RemoveSpecialCharandTrim(leaderAddressDetails.Country).ToLower();
                var individualCountry = RemoveSpecialCharandTrim(individualAddressDetails.Country).ToLower();


                if ((individualAddressDetails.Country == Constants.Application.CountryUK && leaderAddressDetails.Country == Constants.Application.CountryUK) || (individualAddressDetails.Country == Constants.Application.CountryUS && leaderAddressDetails.Country == Constants.Application.CountryUS))
                {
                    if (!leaderPostCode.Is(individualPostCode) || !(individualAddressLine.Contains(leaderAddressLine) || leaderAddressLine.Contains(individualAddressLine)))
                    {
                        errorDetailsList.Add(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.InvalidAddressMatch, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                    }
                }
                else
                {
                    if (!leaderCountry.Is(individualCountry) || !(leaderCity.Contains(individualCity) || individualCity.Contains(leaderCity)) || !(individualAddressLine.Contains(leaderAddressLine) || leaderAddressLine.Contains(individualAddressLine)))
                    {
                        errorDetailsList.Add(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.InvalidAddressMatch, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                    }
                
                    else if ( !individualPostCode.IsNullOrEmpty() && !leaderPostCode.IsNullOrEmpty())
                    {
                        if (!leaderPostCode.Is(individualPostCode))
                        {
                            errorDetailsList.Add(ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.InvalidAddressMatch, Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault());
                        }
                    }
                }


            }

            return new HouseHoldAccountValidationResponse()
            {
                ErrorDetails= errorDetailsList
            };
        }

        private static string RemoveSpecialCharandTrim(string value)
        {
            var response =  RegExHelper.RemoveSpecificCharacter(value);
            return RegExHelper.RemoveWhiteSpace(response);
        }

    }

}
