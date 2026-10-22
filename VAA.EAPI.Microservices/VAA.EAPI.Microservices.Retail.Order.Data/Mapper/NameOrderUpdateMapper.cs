using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Ticket;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.Order;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Mapper
{
    public static class NameOrderUpdateMapper
    {
        public static OrderDocument UpdateNameCorrectionDocument(OrderDocument response, OrderDocument orderDocument)
        {
            var accountableDocRef = response.OrderItems.First().AccountableDocumentRefs?.First();
            var matchingCust = orderDocument.DataList.CustomerDetails.First(x => x.CustId == response.DataList.CustomerDetails.First().CustId);
            var matchingOrderItem = orderDocument.OrderItems.First(x => x.Services.Any(x => x.ServiceBeneficiaries.Contains(matchingCust.CustId)));

            matchingCust.FirstName = response.DataList.CustomerDetails.First().FirstName;
            matchingCust.LastName = response.DataList.CustomerDetails.First().LastName;
            matchingCust.IsNameChanged  = true;

            var orderDocCoupons = orderDocument.OrderItems.Select(x => x.AccountableDocumentRefs?.FirstOrDefault(docs => docs.CustomerRefId == accountableDocRef.CustomerRefId)?.Coupons).Where(coupons => coupons != null).SelectMany(coupons => coupons);
            matchingOrderItem.AccountableDocumentRefs = accountableDocRef == null ? matchingOrderItem.AccountableDocumentRefs : matchingOrderItem.AccountableDocumentRefs.Select(docRef => docRef.CustomerRefId == accountableDocRef.CustomerRefId ? new AccountableDocumentRef
            {
                DocumentType = accountableDocRef?.DocumentType,
                DocumentNumber = accountableDocRef?.DocumentNumber,
                IssueDate = accountableDocRef?.IssueDate,
                CustomerRefId = accountableDocRef?.CustomerRefId,
                FareCalculation = accountableDocRef?.FareCalculation,
                Coupons = accountableDocRef?.Coupons?.Select(x =>
                 new Coupon()
                 {
                     SegmentRefId = orderDocCoupons?.FirstOrDefault(doc => doc.CouponNumber == x.CouponNumber)?.SegmentRefId,
                     CouponNumber = x.CouponNumber,
                     TicketNumber = x.TicketNumber,
                     CouponStatus = x.CouponStatus,
                     FareBasis = x.FareBasis,
                     TicketExpirationDate = x.TicketExpirationDate
                 }).ToArray(),
                OriginalDocumentNumber = accountableDocRef?.OriginalDocumentNumber,
                OriginalIssueDate = accountableDocRef?.OriginalIssueDate
            } : docRef).ToArray();

            orderDocument.ChangeType = response?.ChangeType; 
            orderDocument.ChangeDateTime= response?.ChangeDateTime;
            orderDocument.Version = orderDocument.Version + 1;
            return orderDocument;
        }
    }
}
