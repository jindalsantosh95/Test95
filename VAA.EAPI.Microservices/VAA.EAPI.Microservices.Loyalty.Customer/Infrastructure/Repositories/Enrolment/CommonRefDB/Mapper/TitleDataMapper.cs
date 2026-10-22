using System;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CommonReferenceDetails;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.CommonRefDB.Models;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.CommonRefDB.Mapper
{
    public class TitleDataMapper
    {
        public static List<TitleDetails> MapTitleData(IEnumerable<TitleMaster> titleMaster)
        {
            List<TitleDetails> titleDetailsList = new List<TitleDetails>();
            foreach (var m in titleMaster ?? Array.Empty<TitleMaster>())
            {
                TitleDetails titleDetails = new TitleDetails()
                {
                    Title = m?.Title
                };
                titleDetailsList.Add(titleDetails);
            }
            return titleDetailsList;
        }
    }
}
