using HotChocolate;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.WebForms.Interfaces;
using VAA.EAPI.GraphQL.Domain.WebForms.Models.Refunds;

namespace VAA.EAPI.GraphQL.Domain.WebForms.Mutations
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    [GraphQLDescription("Webform refund request mutations")]
    public class RefundRequestMutation
    {
        private readonly IRefundRequestService _refundService;

        public RefundRequestMutation(IRefundRequestService refundService)
        {
            _refundService = refundService ?? throw new ArgumentNullException(nameof(refundService));
        }

        [GraphQLName("webformSubmitRefundRequest")]
        [GraphQLDescription("Submits selected refund entries and requester details")]
        public async Task<string> SubmitRefundRequest(RefundForm request)
        {
            return await _refundService.SubmitRefundRequest(request);
        }
    }
}
