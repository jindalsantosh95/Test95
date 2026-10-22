using System.ComponentModel.DataAnnotations;
using NodaTime;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.TravelRequirements
{
    public class TravelRequirementsSaveReturnDateRequest
    {
        [Required]
        public string TravelRequirementsId { get; set; }

        [Required]
        public string PassengerId { get; set; }

        public LocalDate? ReturnOn { get; set; }

        public bool NoReturnDate { get; set; }
    }
}
