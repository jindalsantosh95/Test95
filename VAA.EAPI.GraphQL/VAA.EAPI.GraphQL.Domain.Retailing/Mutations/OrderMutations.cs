using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Interface;
using VAA.EAPI.GraphQL.Domain.Retailing.Models.OrderMutation;

namespace VAA.EAPI.GraphQL.Domain.Retailing.Mutations
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public class OrderMutations
    {
        private readonly IOrderDataProvider _ordersProvider;

        public OrderMutations(IOrderDataProvider ordersProvider)
        {
            _ordersProvider = ordersProvider ?? throw new ArgumentNullException(nameof(ordersProvider));
        }

        //*** TODO: Add authentication and authorization to ensure only authorized users can access booking details ***//
        [Authorize(Roles = new[] { "Retailing.Bookings.Write" })]
        [GraphQLName("updateSSRs")]
        [GraphQLDescription("Submits the order request")]
        public async Task<OrderMutationResponse> UpdateSSRs(OrderMutationRequest request)
        {
            // ADVISORY: This mutation should verify the list of SSRs that are allowed to be mutated to ensure that random SSRs are not added / removed to the booking.
            try
            {
                return new OrderMutationResponse()
                {
                    ConfirmationNumber = await _ordersProvider.UpdateSSRs(request),
                    Status = "Confirmed"
                };
            }
            catch (Exception ex)
            {
                if (ex is GraphQLException)
                {
                    throw;
                }
                throw new GraphQLException(ErrorBuilder.New()
                    .SetMessage("Unhandled exception")
                    .SetExtension("code", 500)
                    .Build());
            }

        }
    }
}
