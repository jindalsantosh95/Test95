using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Entity.VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Entity;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Entity;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Mapper
{
    public class AddressDetailDataMapper
    {
        public static List<AddressDetail> GetAddressDetails(List<CompleteAddressDetails> completeAddressDetails, AddressDetailsRequest request)
        {
            // Filtering QAS service response with provided Postcode & Address Line1, just to get relevant records 
            return completeAddressDetails.Where(item => $"{item.AddressDetails.Building} {item.AddressDetails.Address1}".Contains(request.AddressLine1, StringComparison.OrdinalIgnoreCase) && item.AddressDetails.PostOrZipCode.Replace(" ", "").ToLower().Contains(request.PostCode.Replace(" ", "").ToLower())).Select(x => new AddressDetail
            {

                AddressLine1 = $"{x.AddressDetails.Building} {x.AddressDetails.Address1}",
                AddressLine2 = x.AddressDetails.Address2,
                City = x.AddressDetails.City,
                Region = x.AddressDetails.CountyOrStateOrProvince,
                CountryCode = request.CountryCode,
                CountryName = x.AddressDetails.Country,
                PostalCode = x.AddressDetails.PostOrZipCode,
                Valid = "Y",
                AddressVerified = true

            }).ToList();
        }

        public static IEnumerable<AddressDetail> GetAddressDetails(GbgResponse<RetrieveResponse> gbgRetrieveRs)
        {
            return gbgRetrieveRs?.Items?.Select(address => new AddressDetail()
            {
                AddressLine1 = address.Line1,
                AddressLine2 = address.Line2,
                AddressLine3 = address.Line3,
                City = address.City,
                PostalCode = address.PostalCode,
                Region = address.Province,
                CountryName = address.CountryName,
                CountryCode = address.CountryIso2,
                AddressVerified = true,
                Valid = "Y"
            });
        }

        public static IEnumerable<AddressDetail> GetAddressDetails(GbgResponse<FindResponse> gbgFindRs)
        {

            return gbgFindRs?.Items?.Where(item => item.Type.Equals("Address", StringComparison.OrdinalIgnoreCase))
                    .Select(item => new AddressDetail
                    {
                        AddressId = item.Id,
                        AddressLine1 = $"{item.Text} {item.Description}",
                    }) ?? Enumerable.Empty<AddressDetail>();

        }
    }
}
