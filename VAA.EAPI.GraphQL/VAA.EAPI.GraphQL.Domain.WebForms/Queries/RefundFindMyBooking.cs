using HotChocolate;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.WebForms.Interfaces;
using VAA.EAPI.GraphQL.Domain.WebForms.Models.Refunds;

namespace VAA.EAPI.GraphQL.Domain.WebForms.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    [GraphQLDescription("Webform refund status queries")]
    public class RefundFindMyBooking
    {
        private readonly IRefundRequestService _refundService;

        public RefundFindMyBooking(IRefundRequestService refundService)
        {
            _refundService = refundService ?? throw new ArgumentNullException(nameof(refundService));
        }

        [GraphQLName("webformRefundStatus")]
        [GraphQLDescription("Returns refund status for one or more ticket and coupon entries")]
        public async Task<RefundForm> GetRefundStatus(RefundForm form)
        {
            return await _refundService.FindMyBooking(form);
        }

    }
}
