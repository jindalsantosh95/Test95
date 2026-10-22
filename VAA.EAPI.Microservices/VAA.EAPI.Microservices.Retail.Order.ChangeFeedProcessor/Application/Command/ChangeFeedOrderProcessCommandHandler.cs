using MediatR;
using VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Domain;
using VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Infrastructure.Repository;
using VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Mapper;

namespace VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Application.Command
{
    public class ChangeFeedOrderProcessCommandHandler : IRequestHandler<ChangeFeedOrderProcessCommand, bool>
    {
        private readonly IMediator _mediator;

        public ChangeFeedOrderProcessCommandHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<bool> Handle(ChangeFeedOrderProcessCommand request, CancellationToken cancellationToken)
        {
            OrderDTO objOrder = (OrderDTO)request.Orders;
            var ttl = OrderResponseMapper.CalculateTTL(objOrder.SegmentDetails);

            if (ttl > 0)
            {
                // Step 1: Move list outside loop
                List<CustomerOrderDetails> objCustomerOrderDetailsList = new List<CustomerOrderDetails>();

                foreach (CustomerDetail item in objOrder.CustomerDetails)
                {
                    string email = string.Empty;
                    string contactNumber = string.Empty;

                    foreach (var contact in item.CustomerContacts.Contacts)
                    {
                        var customerEmail = OrderResponseMapper.GetContactEmail(contact.Email);

                        if (!string.IsNullOrEmpty(customerEmail) && string.IsNullOrEmpty(email))
                        {
                            email = customerEmail;
                        }

                        var customerPhone = OrderResponseMapper.Get_contact_details(contact.Phone);

                        if (!string.IsNullOrEmpty(customerPhone) && string.IsNullOrEmpty(contactNumber))
                        {
                            contactNumber = customerPhone;
                        }
                    }

                    // create object
                    CustomerOrderDetails objCustomerOrderDetails =
                        OrderResponseMapper.GetCustomerOrderDetails(
                            item.CustId.TrimStart(new Char[] { '0' }),
                            objOrder.OrderId,
                            ttl);

                    objCustomerOrderDetails.CustomerEmail = email;
                    objCustomerOrderDetails.CustomerPhone = contactNumber;

                    // add to master 
                    objCustomerOrderDetailsList.Add(objCustomerOrderDetails);
                }

                // Step 2: Remove Duplicates (Outside Loop)
                var distinctItems = objCustomerOrderDetailsList
                    .DistinctBy(i => new { i.CustomerPhone, i.CustomerID, i.OrderID })
                    .Where(x =>
                        !string.IsNullOrEmpty(x.CustomerID) &&
                        !string.IsNullOrEmpty(x.OrderID))
                    .ToList();

                // Step 3: Send single call 
                if (distinctItems.Any())
                {
                    await _mediator.Send(
                        new OrderRepoCommand(distinctItems)
                    );
                }
            }

            return true;
        }
    }
}
