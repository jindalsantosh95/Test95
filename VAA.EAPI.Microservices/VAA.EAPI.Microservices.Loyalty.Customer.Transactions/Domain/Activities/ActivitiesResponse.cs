using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Base;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Activities
{
    public class ActivitiesResponse : ICustomerTransactionQueryResponse
    {
        public IEnumerable<Activity> Activities { get; set; }
        public string CustomerId { get; set; } = string.Empty;

        public ActivitiesResponse()
        {
            // Required for Serialisation
        }

        public ActivitiesResponse(string customerId, IEnumerable<object> transactions)
        {
            CustomerId = customerId;

            IEnumerable<ActivityDTO> dtoList = Json.Deserialize<IEnumerable<ActivityDTO>>(Json.Serialize(transactions));

            // Convert from DTO to Domain
            Activities = dtoList.Select(dto => (Activity)dto);
        }
    }
}
