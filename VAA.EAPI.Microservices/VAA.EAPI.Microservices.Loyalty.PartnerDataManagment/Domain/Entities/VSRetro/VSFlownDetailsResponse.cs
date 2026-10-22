namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.VSRetro
{
    public class VSFlownDetailsResponse
    {
        public IEnumerable<VSRetroFlownDetail> VSRetroFlownDetails { get; set; }

        public VSFlownDetailsResponse(IEnumerable<VSRetroFlownDetail> vSRetroFlownDetails)
        {
            VSRetroFlownDetails = vSRetroFlownDetails;
        }
    }
}
