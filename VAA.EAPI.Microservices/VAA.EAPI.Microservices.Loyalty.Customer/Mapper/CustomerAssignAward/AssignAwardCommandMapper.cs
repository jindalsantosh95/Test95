using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.CustomerAssignAward;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Mapper.CustomerAssignAward
{
    public class AssignAwardCommandMapper
    {
        public static CustomerAssignAwardEventProcessCommand Mapper(CloudEvent request)
        {
            return new CustomerAssignAwardEventProcessCommand
            {
                Id = request.Id,
                Data = request.Data,
                Type = request.Type,
                Subject = request.Subject,
                Specversion = request.Specversion
            };
        }
    }
}
