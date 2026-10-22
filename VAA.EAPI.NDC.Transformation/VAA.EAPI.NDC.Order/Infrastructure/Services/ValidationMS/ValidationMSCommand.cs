using MediatR;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ValidationMS;


namespace VAA.EAPI.NDC.Order.Infrastructure.Services.ValidationMS
{
    public class ValidationMSCommand : IRequest<ValidationRS>
    {
        public AgencyValidateRequest AgencyValidateRequest { get; set; }
        public string AgentSubscriptionKey { get; set; }
    }
}
