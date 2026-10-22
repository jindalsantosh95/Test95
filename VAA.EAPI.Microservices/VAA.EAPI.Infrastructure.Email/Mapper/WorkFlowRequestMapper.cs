using VAA.EAPI.Domain.Entities.Email;
using VAA.EAPI.Infrastructure.Email.Entities;

namespace VAA.EAPI.Infrastructure.Email.Mapper
{
    public class WorkFlowRequestMapper
    {
        public static WorkFlowRequest MapRequest(SendEmailRequest request)
        {
            return new WorkFlowRequest()
            {
                FromAddress = request.From,
                ToAddress = request.To,
                CcAddress = request.Cc,
                Subject = request.Subject,
                Template = request.Template,
                Data = request.DataFields,
            };
        }
    }
}
