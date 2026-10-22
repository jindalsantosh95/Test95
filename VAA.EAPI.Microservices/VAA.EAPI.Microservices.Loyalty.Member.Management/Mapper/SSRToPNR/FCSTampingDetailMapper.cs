using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.SSRToPNR;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.SSRToPNR
{
    public class FCSTampingDetailMapper
    {       
        public static MemberDetailsToAddSSR FCSTampingDataMapper(MemberDetailsToAddSSR memberDetail, string fileName,MemberDetailsToAddSSR.Statuses status, AddFCNumberToPNRQueryResponse addSSRResponse)
        {
            return new MemberDetailsToAddSSR()
            {
                PartitionKey = fileName,
                RowKey = memberDetail.OrderId,
                AmperityId = memberDetail.AmperityId,
                MemberNumber = memberDetail.MemberNumber,
                PNR = memberDetail.PNR,
                FirstName = memberDetail.FirstName,
                LastName = memberDetail.LastName,
                Pos = memberDetail.Pos,
                OrderId = memberDetail.OrderId,
                Status = (memberDetail.Response == Constants.Application.Faliure) ? MemberDetailsToAddSSR.Statuses.Failed.ToString() : status.ToString(),
                Response = addSSRResponse?.Response,
                Error = addSSRResponse?.ErrorMessage                
            };
        }
    }
}
