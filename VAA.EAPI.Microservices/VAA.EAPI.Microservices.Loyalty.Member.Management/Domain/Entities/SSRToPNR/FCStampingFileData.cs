
namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.SSRToPNR
{
    public class FCStampingResponseFileData
    {
        public string AmperityId { get; set; }
        public string MemberNumber { get; set; }
        public string PNR { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Pos { get; set; }
        public string OrderId { get; set; }
        public string Response { get; set; }
        public string Error { get; set; }

        public static explicit operator FCStampingResponseFileData(MemberDetailsToAddSSR memberDetail)
        {
            return new()
            {
                AmperityId = memberDetail.AmperityId,
                MemberNumber = memberDetail.MemberNumber,
                PNR = memberDetail.PNR,
                FirstName = memberDetail.FirstName,
                LastName = memberDetail.LastName,
                Pos = memberDetail.Pos,
                OrderId = memberDetail.OrderId,
                Response = memberDetail.Response,
                Error = memberDetail.Error
            };
        }

    }
}

