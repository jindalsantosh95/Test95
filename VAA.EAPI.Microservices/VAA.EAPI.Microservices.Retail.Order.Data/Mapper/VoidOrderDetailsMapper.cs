using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Mapper
{
    public class VoidOrderDetailsMapper
    {
        public static VoidOrderDetails MapVoidOrderDetails(PnrSearch orderDetails, OrderReadRequestData requestData)
        {
            return new VoidOrderDetails
            {
                Pnr = requestData?.Pnr,
                IssueDate = requestData?.IssueDate,
                Tickets = requestData.Tickets,
                OrderData = new PnrSearch()
                {
                    OrderRef = orderDetails?.OrderRef,
                    SchemaVersion = orderDetails?.SchemaVersion,
                    DocType = orderDetails?.DocType,
                    OrderCreationDate = orderDetails?.OrderCreationDate,
                    OrderCreationTime = orderDetails?.OrderCreationTime,
                    OrderStatus = orderDetails?.OrderStatus,
                    Channel = orderDetails?.Channel,
                    ETicket = orderDetails?.ETicket,
                    Payments = orderDetails?.Payments,
                    CreatedBy = orderDetails?.CreatedBy,
                    CreatedDateTime = orderDetails?.CreatedDateTime,
                    UpdateBy = orderDetails?.UpdateBy,
                    UpdatedDateTime = orderDetails?.UpdatedDateTime,
                    Id = orderDetails?.Id,
                    PartitionKey = orderDetails?.PartitionKey
                }
            };
        }
    }
}
