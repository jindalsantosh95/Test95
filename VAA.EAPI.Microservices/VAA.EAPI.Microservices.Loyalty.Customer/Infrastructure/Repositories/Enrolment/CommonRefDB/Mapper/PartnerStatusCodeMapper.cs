using System;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CommonReferenceDetails;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.CommonRefDB.Models;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.CommonRefDB.Mapper
{
    public class PartnerStatusCodeMapper
    {
        public static List<PartnerStatusCodeDetails> MapPartnerStatusCodeData(IEnumerable<PartnerStatusCodes> partnerStatusCode)
        {
            List<PartnerStatusCodeDetails> partnerStatusCodeDetails = new List<PartnerStatusCodeDetails>();
            foreach (var p in partnerStatusCode ?? Array.Empty<PartnerStatusCodes>())
            {
                PartnerStatusCodeDetails partnerstatuscodedetails = new PartnerStatusCodeDetails()
                {
                    StatusCode = p?.StatusCode
                };
                partnerStatusCodeDetails.Add(partnerstatuscodedetails);
            }
            return partnerStatusCodeDetails;
        }
    }
}

