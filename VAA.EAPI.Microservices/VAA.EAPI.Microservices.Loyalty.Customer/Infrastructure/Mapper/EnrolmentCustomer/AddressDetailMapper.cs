using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Mapper.EnrolmentCustomer
{
    public static class AddressDetailMapper
    {
        public static Address[] MapEAPIAddressToSCVAddress(CreateCustomerEntities createCustomerEntities)
        {
            return MapAddress(createCustomerEntities);
        }

        public static Address[] MapAddress(CreateCustomerEntities createCustomerEntities)
        {
            List<Address> addressGrp = new List<Address>();
            createCustomerEntities?.AddressDetails?.ForEach(ad =>
            {
                Address addressDetail = new Address()
                {
                    MetroArea = ad?.MetroArea,
                    City = ad?.City,
                    PostalStatus = string.Empty,
                    Province = ad?.Region,
                    AddressVerified = ad.AddressVerified == true ? "Y" : "N",
                    AdditionalComments = string.Empty,
                    AddressLine3 = ad?.AddressLine3,
                    AddressLine2 = ad?.AddressLine2,
                    AddressLine1 = ad?.AddressLine1,
                    Primary = ad?.PreferredFlag,
                    Country = ad.CountryName,
                    AddressType = Convert.ToString(ad.AddressType),
                    PostCode = ad?.PostalCode,
                    AddressId = Utility.GetId(),
                    LandlordOrCompanyName = ad?.CompanyName,
                    CreatedOn = DateTime.Now.ToString(Constants.Application.ISODateTimeFormat),
                    LastUpdatedBy = createCustomerEntities?.ChannelUserId ?? createCustomerEntities.EnrollmentChannel,
                    CreatedBy = createCustomerEntities?.ChannelUserId ?? createCustomerEntities.EnrollmentChannel,
                    LastUpdatedOn = DateTime.Now.ToString(Constants.Application.ISODateTimeFormat),
                    SourceIdentifier = string.Empty,
                    SourceSystem = string.Empty,
                    Valid = ad.Valid,
                    GeoCodeValidFlag = string.Empty,
                    AddressVerificationStatus = string.Empty,
                    GroupAddress = string.Empty,
                    PartnerFileId = string.Empty
                };

                addressGrp.Add(addressDetail);
            });
            return addressGrp.ToArray();
        }

    }
}
