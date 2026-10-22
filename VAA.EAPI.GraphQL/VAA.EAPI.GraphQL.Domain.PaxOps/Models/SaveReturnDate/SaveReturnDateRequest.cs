using HotChocolate;
using NodaTime;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.SaveReturnDate
{
    public class SaveReturnDateRequest
    {
        public string CheckInId { get; set; }
        public string PassengerId { get; set; }
        [GraphQLDeprecated("Use ReturnOn")]
        public DateTime? ReturnDate { get; set; }
        public LocalDate? ReturnOn { get; set; }
        public bool NoReturnDate { get; set; }
    }
}
