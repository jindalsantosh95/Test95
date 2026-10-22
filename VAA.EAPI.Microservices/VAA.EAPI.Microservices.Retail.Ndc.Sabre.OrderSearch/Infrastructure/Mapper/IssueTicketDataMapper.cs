using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Domain.Virgin.Models.Ticket;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetElectronicDocument;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Domain;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Infrastructure.Mapper
{
    public class IssueTicketDataMapper
    {
        public static AccountableDocumentRef AccountableDocumentsMapper(AccountableDocumentRef documentRef, GetElectronicDocumentRS couponRS, OrderDocument orderDocument)
        {
            var coupon = (TicketingDocumentTicketED)couponRS.DocumentDetailsDisplay.Item;

            documentRef.FareCalculation = coupon.FareCalculation.New.Value;
            documentRef.DocumentType = AccountableDocumentRef.DocumentTypes.TEnum;
            documentRef.IssueDate = coupon.Details.LocalIssueDateTime.ToString(Constants.Application.DateOnlyFormat);
            documentRef.Coupons = GetCouponInfo(documentRef.DocumentNumber, couponRS, orderDocument.DataList.SegmentDetails);

            return documentRef;
        }

        private static IEnumerable<Coupon> GetCouponInfo(string ticketNumber, GetElectronicDocumentRS couponRS, IEnumerable<Segment> segmentDetails)
        {
            return (couponRS.DocumentDetailsDisplay.Item as TicketingDocumentTicketED).ServiceCoupon.Where(x => x.CurrentStatus != null).Select(x =>
            new Coupon()
            {
                CouponNumber = x.coupon,
                TicketNumber = ticketNumber,
                CouponStatus = x.CurrentStatus,
                TicketEffectiveDate = x.NotValidBeforeDate.ToString(Constants.Application.DateFormat),
                TicketExpirationDate = x.NotValidAfterDate.ToString(Constants.Application.DateFormat),
                SegmentRefId = segmentDetails.Any() ? segmentDetails.First(s => s.OriginAirportCode == x.StartLocation.Value && s.DestinationAirportCode == x.EndLocation.Value).SegmentId : null,
                FareBasis = x.FareBasis
            });
        }
    }
}
