using System;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CommonReferenceDetails;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.CommonRefDB.Models;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.CommonRefDB.Mapper
{
    public class PartnerCodeMapper
    {
        public static List<PartnerProfileDetails> MapPartnerCodeData(IEnumerable<PartnerProfile> partnerStatusCode)
        {
            List<PartnerProfileDetails> partnerCodeDetails = new List<PartnerProfileDetails>();
            foreach (var p in partnerStatusCode ?? Array.Empty<PartnerProfile>())
            {
                PartnerProfileDetails partnercodedetails = new PartnerProfileDetails()
                {
                    PartnerCode = p?.PartnerCode,
                    RowId = p?.RowId
                };
                partnerCodeDetails.Add(partnercodedetails);
            }
            return partnerCodeDetails;
        }
    }
}
