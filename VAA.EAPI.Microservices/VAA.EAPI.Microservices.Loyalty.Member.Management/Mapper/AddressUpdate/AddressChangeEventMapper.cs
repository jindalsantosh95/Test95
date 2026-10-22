using System;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Customer;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.AddressUpdate;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.AddressUpdate
{
    public class AddressChangeEventMapper
    {
        public static CustomerSearchUpdateEventData CustomerSearchDocUpdateRequestToEventDataMapper(AddressChangeEventData eventData, string memberNumber)
        {
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));

            return new CustomerSearchUpdateEventData()
            {
                LoyaltyMemberNumber = memberNumber,
                PostCode = eventData.AddressDetail.PostalCode,
                Country = eventData.AddressDetail.Country
            };
        }

        public static CustomerUpdateEventData CustomerDocUpdateRequestToEventDataMapper(AddressChangeEventData eventData, Address currentAddress)
        {
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));

            return new CustomerUpdateEventData()
            {
                CustomerId = eventData.CustomerId,
                LastUpdatedBy = eventData.InitiatedBy,
                LastUpdatedOn = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),
                AddressDetails = new Address[] { new Address()
                {
                    SourceIdentifier = currentAddress.SourceIdentifier,
                    SourceSystem = currentAddress.SourceSystem,
                    CreatedBy = currentAddress.CreatedBy,
                    CreatedOn = currentAddress.CreatedOn,
                    LastUpdatedBy = eventData.InitiatedBy,
                    LastUpdatedOn = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),
                    AddressId = currentAddress.AddressId,
                    AddressLine1 = string.IsNullOrEmpty(eventData.AddressDetail.AddressLine1) ? string.Empty : eventData.AddressDetail.AddressLine1,
                    AddressLine2 = string.IsNullOrEmpty(eventData.AddressDetail.AddressLine2) ? string.Empty : eventData.AddressDetail.AddressLine2,
                    AddressLine3 = string.IsNullOrEmpty(eventData.AddressDetail.AddressLine3) ? string.Empty : eventData.AddressDetail.AddressLine3,
                    PostalStatus = string.IsNullOrEmpty(eventData.AddressDetail.PostalStatus) ? string.Empty : eventData.AddressDetail.PostalStatus,
                    AddressType = string.IsNullOrEmpty(eventData.AddressDetail.AddressType) ? string.Empty : eventData.AddressDetail.AddressType,
                    City = string.IsNullOrEmpty(eventData.AddressDetail.City) ? string.Empty : eventData.AddressDetail.City,
                    AdditionalComments = string.Empty,
                    Country = string.IsNullOrEmpty(eventData.AddressDetail.Country) ? string.Empty : eventData.AddressDetail.Country,
                    GeoCodeValidFlag = string.Empty,
                    LandlordOrCompanyName = string.IsNullOrEmpty(eventData.AddressDetail.CompanyName) ? string.Empty : eventData.AddressDetail.CompanyName,
                    MetroArea = string.IsNullOrEmpty(eventData.AddressDetail.MetroArea) ? string.Empty : eventData.AddressDetail.MetroArea,
                    Valid = string.IsNullOrEmpty(eventData.AddressDetail.Valid) ? string.Empty : eventData.AddressDetail.Valid,
                    Province = string.IsNullOrEmpty(eventData.AddressDetail.Province) ? string.Empty : eventData.AddressDetail.Province,
                    AddressVerified = string.IsNullOrEmpty(eventData.AddressDetail.AddressVerified) ? string.Empty : eventData.AddressDetail.AddressVerified,
                    AddressVerificationStatus = string.Empty,
                    PostCode = string.IsNullOrEmpty(eventData.AddressDetail.PostalCode) ? string.Empty : eventData.AddressDetail.PostalCode,
                    GroupAddress = string.Empty,
                    PartnerFileId = string.Empty,
                    Primary = string.IsNullOrEmpty(eventData.AddressDetail.Primary) ? string.Empty : eventData.AddressDetail.Primary
                } }
            };
        }

        public static AddressHistoryCreateEventData AddressHistoryCreateRequestToEventDataMapper(AddressChangeEventData eventData, Address currentAddress)
        {
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));

            return new AddressHistoryCreateEventData()
            {
                PartitionKey = eventData.CustomerId,
                CustomerId = eventData.CustomerId,
                SourceIdentifier = string.IsNullOrEmpty(currentAddress.SourceIdentifier) ? string.Empty : currentAddress.SourceIdentifier,
                SourceSystem = string.IsNullOrEmpty(currentAddress.SourceSystem) ? string.Empty : currentAddress.SourceSystem,
                CreatedOn = string.IsNullOrEmpty(currentAddress.CreatedOn) ? string.Empty : Convert.ToDateTime(currentAddress.CreatedOn).ToString(Constants.Application.ISODateTimeFormat),
                CreatedBy = string.IsNullOrEmpty(currentAddress.CreatedBy) ? string.Empty : currentAddress.CreatedBy,
                LastUpdatedOn = string.IsNullOrEmpty(currentAddress.CreatedOn) ? string.Empty : Convert.ToDateTime(currentAddress.LastUpdatedOn).ToString(Constants.Application.ISODateTimeFormat),
                LastUpdatedBy = string.IsNullOrEmpty(currentAddress.LastUpdatedBy) ? string.Empty : currentAddress.LastUpdatedBy,
                AddressId = string.IsNullOrEmpty(currentAddress.AddressId) ? string.Empty : currentAddress.AddressId,
                AddressLine1 = string.IsNullOrEmpty(currentAddress.AddressLine1) ? string.Empty : currentAddress.AddressLine1,
                AddressLine2 = string.IsNullOrEmpty(currentAddress.AddressLine2) ? string.Empty : currentAddress.AddressLine2,
                AddressLine3 = string.IsNullOrEmpty(currentAddress.AddressLine3) ? string.Empty : currentAddress.AddressLine3,
                PostalStatus = string.IsNullOrEmpty(currentAddress.PostalStatus) ? string.Empty : currentAddress.PostalStatus,
                AddressType = string.IsNullOrEmpty(currentAddress.AddressType) ? string.Empty : currentAddress.AddressType,
                City = string.IsNullOrEmpty(currentAddress.City) ? string.Empty : currentAddress.City,
                AdditionalComments = string.IsNullOrEmpty(currentAddress.AdditionalComments) ? string.Empty : currentAddress.AdditionalComments,
                Country = string.IsNullOrEmpty(currentAddress.Country) ? string.Empty : currentAddress.Country,
                GeoCodeValidFlag = string.IsNullOrEmpty(currentAddress.GeoCodeValidFlag) ? string.Empty : currentAddress.GeoCodeValidFlag,
                LandlordOrCompanyName = string.IsNullOrEmpty(currentAddress.LandlordOrCompanyName) ? string.Empty : currentAddress.LandlordOrCompanyName,
                MetroArea = string.IsNullOrEmpty(currentAddress.MetroArea) ? string.Empty : currentAddress.MetroArea,
                Valid = string.IsNullOrEmpty(currentAddress.Valid) ? string.Empty : currentAddress.Valid,
                Province = string.IsNullOrEmpty(currentAddress.Province) ? string.Empty : currentAddress.Province,
                AddressVerified = string.IsNullOrEmpty(currentAddress.AddressVerified) ? string.Empty : currentAddress.AddressVerified,
                AddressVerificationStatus = string.IsNullOrEmpty(currentAddress.AddressVerificationStatus) ? string.Empty : currentAddress.AddressVerificationStatus,
                PostCode = string.IsNullOrEmpty(currentAddress.PostCode) ? string.Empty : currentAddress.PostCode,
                GroupAddress = string.IsNullOrEmpty(currentAddress.GroupAddress) ? string.Empty : currentAddress.GroupAddress,
                PartnerFileId = string.IsNullOrEmpty(currentAddress.PartnerFileId) ? string.Empty : currentAddress.PartnerFileId,
                Primary = string.IsNullOrEmpty(currentAddress.Primary) ? string.Empty : currentAddress.Primary
            };
        }
    }
}

