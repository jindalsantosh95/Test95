using System;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.CommonReferenceDetails;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.CommonRefDB.Models;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.CommonRefDB.Mapper
{
    public class MediaCodeDataMapper
    {
        public static List<MediaCodeDetails> MapMediaCodeData(IEnumerable<MediaCodes> mediaCodes)
        {
            List<MediaCodeDetails> mediaCodeDetails = new List<MediaCodeDetails>();
            foreach (var m in mediaCodes ?? Array.Empty<MediaCodes>())
            {
                MediaCodeDetails mediaCodeDetails1 = new MediaCodeDetails()
                {
                    MediaCode = m?.MediaCode
                };
                mediaCodeDetails.Add(mediaCodeDetails1);
            }
            return mediaCodeDetails;
        }
    }
}

