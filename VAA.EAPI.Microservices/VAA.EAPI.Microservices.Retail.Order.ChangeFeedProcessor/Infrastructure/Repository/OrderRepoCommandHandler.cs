using MediatR;
using VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Domain;
using VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Domain.Enum;
using VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Infrastructure.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Infrastructure.Repository
{
    public class OrderRepoCommandHandler : IRequestHandler<OrderRepoCommand, bool>
    {
        private IOrderService<CustomerOrderDetailsDTO> _orderService;

        public OrderRepoCommandHandler(IOrderService<CustomerOrderDetailsDTO> orderService)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        }

        public async Task<bool> Handle(OrderRepoCommand request, CancellationToken cancellationToken)
        {
            var incomingCustomers = request.CustomerOrderDetails;

            // Get OrderID from DTO
            var orderId = incomingCustomers.FirstOrDefault(x => !string.IsNullOrEmpty(x.OrderID))?.OrderID;
            if (string.IsNullOrEmpty(orderId))
                return false;

            // Fetch existing records from DB
            var existingCustomerOrderDoc = await _orderService.GetItemAsync(orderId, DocumentType.CustomerOrderAssociation.ToString());

            //  Prepare incoming CustomerIDs
            var incomingCustomerIds = incomingCustomers.Where(x => !string.IsNullOrEmpty(x.CustomerID)).Select(x => x.CustomerID)
                .Distinct()
                .ToList();

            //  Insert/ Update (based on OrderID + CustomerID)
            foreach (var customer in incomingCustomers)
            {
                if (string.IsNullOrEmpty(customer.CustomerID) ||
                    string.IsNullOrEmpty(customer.OrderID))
                    continue;

                var matchedRecord = existingCustomerOrderDoc.FirstOrDefault(x =>
                    x.CustomerID == customer.CustomerID &&
                    x.OrderID == customer.OrderID);

                if (matchedRecord != null)
                {
                    // Update
                    customer.Id = matchedRecord.Id;
                    customer.CreatedOn = matchedRecord.CreatedOn;

                    await _orderService.PatchItemAsync(
                        (CustomerOrderDetailsDTO)customer,
                        DocumentType.CustomerOrderAssociation);
                }
                else
                {
                    // Insert
                    await _orderService.InsertItemAsync(
                        (CustomerOrderDetailsDTO)customer,
                        DocumentType.CustomerOrderAssociation);
                }
            }

            // Delete (Not Matching Customer IDs)
            var recordsToDelete = existingCustomerOrderDoc.Where(x => !string.IsNullOrEmpty(x.CustomerID) && !incomingCustomerIds.Contains(x.CustomerID)) // Not in request
                .ToList();

            if (recordsToDelete.Any())
            {
                // Delete one by one (as per your requirement)
                foreach (var record in recordsToDelete)
                {
                    await _orderService.DeleteItemAsync(
                        record.OrderID,
                        record.CustomerID,
                        record.Id,
                        record.PartitionKey,
                        DocumentType.CustomerOrderAssociation.ToString());
                }
            }
            return true;

        }
    }
}
