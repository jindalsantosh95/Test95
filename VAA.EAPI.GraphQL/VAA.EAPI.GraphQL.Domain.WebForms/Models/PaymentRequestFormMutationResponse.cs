using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.WebForms.Models
{
    [GraphQLDescription("Webform Mutation response")]
    public class PaymentRequestFormMutationResponse
    {
        [GraphQLDescription("Status of CRM payment update (EnumValues : Success, Failure)")]
        public StatusEnum Status { get; set; }

    }
}
