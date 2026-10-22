using System;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.CommonReferenceDetails;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.CommonRefDB.Models;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.CommonRefDB.Mapper
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

