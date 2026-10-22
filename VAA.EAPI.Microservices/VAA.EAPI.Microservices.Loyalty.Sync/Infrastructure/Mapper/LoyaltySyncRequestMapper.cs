using System;
using System.Globalization;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Sync.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Services.Loyalty.SyncService;
using VAA.EAPI.Microservices.Loyalty.Sync.LoyaltyProfileSyncServiceRef;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Mapper
{
    public class LoyaltySyncRequestMapper
    {
        public static AddRequestType ToOcdbSyncRequest(LoyaltySyncServiceCommand syncRequest)
        {
            AddressDetail address = syncRequest.LoyaltySyncRepoQuery.LoyaltySyncQuery.Data.AddressDetails.Any(x => x.PreferredFlag == "Y") ? syncRequest.LoyaltySyncRepoQuery.LoyaltySyncQuery.Data.AddressDetails.First(x => x.PreferredFlag == "Y") : syncRequest.LoyaltySyncRepoQuery.LoyaltySyncQuery.Data.AddressDetails.First();

            return new AddRequestType
            {
                RequestInfo = new RequestInfo
                {
                    AppChannelName = ConfigReader.Get(Constants.Environment.AppChannelName),
                    ApplicationId = ConfigReader.Get(Constants.Environment.ApplicationId),
                    ApplicationVersion = Constants.Application.ApplicationVersion,
                    DebugOptions = Constants.Application.DebugOption,
                    TestLab = ConfigReader.Get(Constants.Environment.TestLab)
                },
                AccountInfo = new AccountInfoType
                {
                    EnrollmentDate = DateTime.Now.ToString("yyyyMMdd"),
                    EnrollmentSourceInd = "Y",
                    LrecModificationIndicator = Constants.Application.LrecModificationIndicator,
                    TenDigitLoyaltyNumber = syncRequest.LoyaltySyncRepoQuery.LoyaltySyncQuery.Data.FcNumber
                },
                NameInfo = new NameInfoType
                {
                    DateOfBirth = DateTime.ParseExact(syncRequest.LoyaltySyncRepoQuery.LoyaltySyncQuery.Data.DateOfBirth, "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("yyyyMMdd"),
                    FirstName = syncRequest?.LoyaltySyncRepoQuery?.LoyaltySyncQuery?.Data?.FirstName,
                    GenderCode = syncRequest?.LoyaltySyncRepoQuery?.LoyaltySyncQuery?.Data?.Gender,
                    LastName = syncRequest?.LoyaltySyncRepoQuery?.LoyaltySyncQuery?.Data?.TestMember == true ? "VirginTest" : syncRequest?.LoyaltySyncRepoQuery?.LoyaltySyncQuery?.Data?.LastName,
                    LrecModificationIndicator = Constants.Application.LrecModificationIndicator,
                    Prefix = syncRequest?.LoyaltySyncRepoQuery?.LoyaltySyncQuery?.Data?.AbbreviatedTitle,
                    PreferredLanguage = "EN",
                    TenDigitLoyaltyNumber = syncRequest?.LoyaltySyncRepoQuery?.LoyaltySyncQuery?.Data?.FcNumber
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
                    TenDigitLoyaltyNumber = syncRequest?.LoyaltySyncRepoQuery?.LoyaltySyncQuery?.Data?.FcNumber,
                    TerritoryOrPrefecture = address?.CountryCode,
                    ZipOrPostalCode = address?.PostalCode,
                },
                ProgramInfo = new[]
                {
                   new ProgramInfoType
                   {
                       ActiveOrInactiveInd = "Y",
                       CustomerProgTypeEffDate = DateTime.Now.ToString("yyyyMMdd"),
                       CustomerProgramCode = "FF",
                       LrecModificationIndicator = Constants.Application.LrecModificationIndicator,
                       TenDigitLoyaltyNumber = syncRequest?.LoyaltySyncRepoQuery?.LoyaltySyncQuery?.Data?.FcNumber
                   }
                },
                PhoneInfo = new[]
                {
                    new PhoneInfoType
                    {
                        AreaOrCityCode = syncRequest?.LoyaltySyncRepoQuery?.LoyaltySyncQuery?.Data?.ContactDetails?.PhoneDetails?.FirstOrDefault()?.PhoneNumber.Insert(0,"0").Substring(0,5),
                        DialedCountryCode =  syncRequest?.LoyaltySyncRepoQuery?.LoyaltySyncQuery?.Data?.ContactDetails?.PhoneDetails?.FirstOrDefault()?.CountryCode,
                        LrecModificationIndicator = Constants.Application.LrecModificationIndicator,
                        PhoneLocationCode = GetLocationCode.PhoneLocationCode(syncRequest?.LoyaltySyncRepoQuery?.LoyaltySyncQuery?.Data?.ContactDetails?.PhoneDetails?.FirstOrDefault()?.PhoneType),
                        PhoneNumber = syncRequest?.LoyaltySyncRepoQuery?.LoyaltySyncQuery?.Data?.ContactDetails?.PhoneDetails?.FirstOrDefault()?.PhoneNumber.Insert(0,"0").Substring(4,6),
                        PrimaryPhoneIndicator = syncRequest?.LoyaltySyncRepoQuery?.LoyaltySyncQuery?.Data?.ContactDetails?.PhoneDetails?.FirstOrDefault()?.PreferredFlag == "Y" ? "Y":"N",
                        TenDigitLoyaltyNumber =syncRequest?.LoyaltySyncRepoQuery?.LoyaltySyncQuery?.Data?.FcNumber
                    }
                },
                EmailInfo = new EmailInfoType
                {
                    EmailAddress = syncRequest?.LoyaltySyncRepoQuery?.LoyaltySyncQuery?.Data?.ContactDetails?.EmailDetails?.FirstOrDefault()?.EmailId,
                    EmailAddressLocationCode = GetLocationCode.GetEmailAddressLocationCode(syncRequest?.LoyaltySyncRepoQuery?.LoyaltySyncQuery?.Data?.ContactDetails?.EmailDetails?.FirstOrDefault()?.Usage),
                    LrecModificationIndicator = Constants.Application.LrecModificationIndicator,
                    PrimaryEmailIndicatorInd = syncRequest?.LoyaltySyncRepoQuery?.LoyaltySyncQuery?.Data?.ContactDetails?.EmailDetails?.FirstOrDefault()?.PreferredFlag == "Y" ? "Y" : "N",
                    TenDigitLoyaltyNumber = syncRequest?.LoyaltySyncRepoQuery?.LoyaltySyncQuery?.Data?.FcNumber
                }
            };
        }
    }
}
