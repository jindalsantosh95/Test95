using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.TicketingProcessV3Proxy;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.Enums;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Mapper.OrderChange
{
    public class DocumentRefMapper
    {
        public static List<AccountableDocumentReference> Air4ToOrderChangeDocumentRefMapper(PerformRevenueResponseType performRevenueResponse, OrderChangeRequestData request, List<Passenger> passengers)
        {
            List<AccountableDocumentReference> accountableDocumentReferences = new();
            foreach (var pd in performRevenueResponse.Pnr.TicketCouponInfo)
            {
                AccountableDocumentReference accountableDocumentReference = new()
                {
                    DocumentType = DocumentTypes.TEnum,
                    DocumentNumber = pd.TicketNumber,
                    CheckDigit = pd.CheckDigit,
                    CouponNumber = pd.CouponNumber,
                    CouponStatus = pd.CouponStatus,
                    SegmentRefId = performRevenueResponse.Pnr.SegmentDl.FirstOrDefault(x => x.DeparturePoint == pd.DeparturePoint && x.ArrivalPoint == pd.ArrivalPoint).SegmentNumber,
                    DocumentIssuer = "",
                    IssueDate = performRevenueResponse.Pnr.TicketData.FirstOrDefault(x => x.TicketNumber == pd.TicketNumber).TicketIssueDate,
                    CustomerRefId = passengers.FirstOrDefault(x => x.LastNameNumber == pd.LastNameNumber).CustomerId,
                    CurrentSeatAssignment = ""
                };
                accountableDocumentReferences.Add(accountableDocumentReference);
            }
            return accountableDocumentReferences;
        }
    }
}
