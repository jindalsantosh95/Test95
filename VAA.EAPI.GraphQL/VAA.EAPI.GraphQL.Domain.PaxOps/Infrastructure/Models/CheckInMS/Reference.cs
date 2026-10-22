using NodaTime;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.CheckInMS
{
    public class Reference
    {
        public string PnrLocator { get; set; }
        public LocalDate? CreatedDate { get; set; }
    }
}
