using VAA.EAPI.Microservices.Loyalty.Sync.LoyaltyProfileSyncServiceRef;
using System;
using System.Linq;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Sync.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Services.ManageAccount;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Mapper
{
    public class UpdateOCDBRequestMapper
    {
        public static UpdateRequestType ToOcdbSyncRequest(UpdateOCDBSyncServiceCommand syncRequest)
        {
            AddressDetail address = syncRequest.UpdateOCDBSyncRepoQuery.AddressDetails.Any(x => x.PreferredFlag == "Y") ? syncRequest.UpdateOCDBSyncRepoQuery.AddressDetails.First(x => x.PreferredFlag == "Y") : syncRequest.UpdateOCDBSyncRepoQuery.AddressDetails.First();

            return new UpdateRequestType
            {
                RequestInfo = new RequestInfo
                {
                    AppChannelName = ConfigReader.Get(Constants.Environment.AppChannelName),
                    ApplicationId = ConfigReader.Get(Constants.Environment.ApplicationId)
                },
                AccountInfo = new AccountInfoType
                {
                    EnrollmentDate = DateTime.Now.ToString(Constants.Application.DateFormat),
                    EnrollmentSourceInd = "Y",
                    LrecModificationIndicator = Constants.Application.LrecModificationIndicator,
                    TenDigitLoyaltyNumber = syncRequest.UpdateOCDBSyncRepoQuery.FcNumber
                },
                NameInfo = new NameInfoType
                {
                    FirstName = syncRequest.UpdateOCDBSyncRepoQuery.FirstName,
                    LastName = syncRequest.UpdateOCDBSyncRepoQuery.TestMember == true ? Constants.Application.VirginTest : syncRequest.UpdateOCDBSyncRepoQuery.LastName,
                    LrecModificationIndicator = Constants.Application.LrecModificationIndicator,
                    TenDigitLoyaltyNumber = syncRequest.UpdateOCDBSyncRepoQuery.FcNumber
                },
                AddressInfo = new AddressInfoType
                {
                    AddressLineOne = address?.AddressLine1,
                    AddressLineTwo = address?.AddressLine2,
                    AddressLocationCode = GetLocationCode.AddressLocationCode(address.AddressType),
                    City = address?.City,
                    CountryCode = address?.CountryCode,
                    LrecModificationIndicator = Constants.Application.LrecModificationIndicator,
                    PrimaryAddressInd = address?.PreferredFlag == "Y" ? "Y" : "N",
                    StateCode = string.IsNullOrEmpty(address.Region) ? null : address?.Region?.Substring(0, 2).ToUpper(),
                    TenDigitLoyaltyNumber = syncRequest.UpdateOCDBSyncRepoQuery.FcNumber,
                    TerritoryOrPrefecture = address?.CountryCode,
                    ZipOrPostalCode = address?.PostalCode,
                },
                ProgramInfo = new[]
                {
                   new ProgramInfoType
                   {
                       CustomerProgTypeEffDate = DateTime.Now.ToString(Constants.Application.DateFormat),
                       CustomerProgramCode = "FF",
                       LrecModificationIndicator = Constants.Application.LrecModificationIndicator,
                       TenDigitLoyaltyNumber = syncRequest.UpdateOCDBSyncRepoQuery.FcNumber
                   }
                },
                PhoneInfo = new[]
                {
                    new PhoneInfoType
                    {
                        AreaOrCityCode = syncRequest.UpdateOCDBSyncRepoQuery.ContactDetails.PhoneDetails.FirstOrDefault()?.PhoneNumber.Insert(0,"0").Substring(0,5),
                        DialedCountryCode =  syncRequest.UpdateOCDBSyncRepoQuery.ContactDetails.PhoneDetails.FirstOrDefault()?.CountryCode,
                        LrecModificationIndicator = Constants.Application.LrecModificationIndicator,
                        PhoneLocationCode = GetLocationCode.PhoneLocationCode(syncRequest.UpdateOCDBSyncRepoQuery.ContactDetails.PhoneDetails.FirstOrDefault() ?.PhoneType),
                        PhoneNumber = syncRequest.UpdateOCDBSyncRepoQuery.ContactDetails.PhoneDetails.FirstOrDefault() ?.PhoneNumber.Insert(0, "0").Substring(4, 6),
                        PrimaryPhoneIndicator = syncRequest.UpdateOCDBSyncRepoQuery.ContactDetails?.PhoneDetails?.FirstOrDefault()?.PreferredFlag == "Y" ? "Y":"N",
                        TenDigitLoyaltyNumber =syncRequest.UpdateOCDBSyncRepoQuery.FcNumber
                    }
                },
                EmailInfo = new EmailInfoType
                {
                    EmailAddress = syncRequest.UpdateOCDBSyncRepoQuery.ContactDetails?.EmailDetails?.FirstOrDefault()?.EmailId,
                    EmailAddressLocationCode = GetLocationCode.GetEmailAddressLocationCode(syncRequest.UpdateOCDBSyncRepoQuery.ContactDetails?.EmailDetails?.FirstOrDefault()?.Usage),
                    LrecModificationIndicator = Constants.Application.LrecModificationIndicator,
                    PrimaryEmailIndicatorInd = syncRequest.UpdateOCDBSyncRepoQuery.ContactDetails?.EmailDetails?.FirstOrDefault()?.PreferredFlag == "Y" ? "Y" : "N",
                    TenDigitLoyaltyNumber = syncRequest.UpdateOCDBSyncRepoQuery.FcNumber
                }
            };
        }
    }
}
