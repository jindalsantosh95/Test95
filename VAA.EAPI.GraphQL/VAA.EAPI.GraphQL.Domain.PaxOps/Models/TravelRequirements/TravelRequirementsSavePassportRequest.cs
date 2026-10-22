using System.ComponentModel.DataAnnotations;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models;
namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.TravelRequirements
{
    public class TravelRequirementsSavePassportRequest
    {
        [Required]
        public string TravelRequirementsId { get; set; }
        [Required]
        public string PassengerId { get; set; }
        [Required]
        public Document Document { get; set; }
        public string MRZLine1 { get; set; }
        public string MRZLine2 { get; set; }
        public bool IsScanned { get; set; }
    }
}
