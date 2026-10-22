using System.Linq;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.Enums;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Mapper.OrderChange
{
    public static class OrderChangeSeatResponseMapper
    {
        public static OrderChangeResponse ResponseMapper(SeatFulfilmentResponse seatFulfillmentRs)
        {
            return new OrderChangeResponse()
            {
                OrderDetails = new OrderDetails()
                {
                    OrderItems = seatFulfillmentRs.OrderItems.Select(x => new Domain.OrderChange.OrderItems()
                    {
                        ProductType = EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.SeatEnum,
                        CurrentSeatAssignment = x.SeatNumber,
                        SegmentRefId = x.FlightIdRef,
                        AccountableDocuments = [
                           new()
                            {
                                 DocumentNumber = x.ElectronicMiscellaneousDocumentNumber,
                                 DocumentType = EAPI.Domain.Virgin.Models.Ticket.AccountableDocumentRef.DocumentTypes.YEnum,
                            }],
                        PaymentDetails = x.PaymentDetails.First()
                    })
                }
            };
        }
    }
}
