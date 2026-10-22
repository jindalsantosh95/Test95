using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Domain.Virgin.Models.Ticket;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetElectronicDocument;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Mapper
{
    public class IssueTicketDataMapper
    {
        public static AccountableDocumentRef AccountableDocumentsMapper(AccountableDocumentRef documentRef, GetElectronicDocumentRS couponRS, OrderDocument orderDocument)
        {
            var document = (TicketingDocumentTicketED)couponRS.DocumentDetailsDisplay.Item;

            documentRef.FareCalculation = document.FareCalculation.New.Value;
            documentRef.DocumentType = AccountableDocumentRef.DocumentTypes.TEnum;
            documentRef.IssueDate = document.Details.LocalIssueDateTime.ToString(Constants.Application.DateOnlyFormat);
            documentRef.Coupons = document.ServiceCoupon.Any() ? GetCouponInfo(documentRef.DocumentNumber, document.ServiceCoupon, orderDocument.DataList.SegmentDetails) : null;
            documentRef.OriginalDocumentNumber = document.RelatedDocument != null ? document.RelatedDocument.Original.Number : null;
            documentRef.OriginalIssueDate = document.RelatedDocument != null ? document.RelatedDocument.Original.IssueDate.ToString(Constants.Application.DateOnlyFormat) : null;

            return documentRef;
        }


        private static IEnumerable<Coupon> GetCouponInfo(string ticketNumber, IEnumerable<TicketingDocumentServiceCouponTicketED> coupons, IEnumerable<Segment> segmentDetails)
        {
            return coupons.Select(x =>
                new Coupon()
                {
                    CouponNumber = x.coupon,
                    TicketNumber = ticketNumber,
                    CouponStatus = x.CurrentStatus,
                    TicketExpirationDate = x.NotValidAfterDate.ToString(Constants.Application.DateFormat),
                    SegmentRefId = segmentDetails?.First(s => s.OriginAirportCode == x.StartLocation.Value && s.DestinationAirportCode == x.EndLocation.Value).SegmentId,
                    FareBasis = x.FareBasis
                });
        }
    }
}
