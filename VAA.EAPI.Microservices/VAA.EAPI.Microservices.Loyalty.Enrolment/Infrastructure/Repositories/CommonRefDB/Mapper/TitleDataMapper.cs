using System;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.CommonReferenceDetails;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.CommonRefDB.Models;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.CommonRefDB.Mapper
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
