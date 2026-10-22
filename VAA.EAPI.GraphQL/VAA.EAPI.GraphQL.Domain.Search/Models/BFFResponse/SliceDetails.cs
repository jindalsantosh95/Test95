namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFResponse
{
    public class SliceDetails
    {
        public int sliceNumber { get; set; }
        public string selectedCabin { get; set; }
        public decimal selectedPrice { get; set; }
        public string? selectedAwardPoints { get; set; }
    }
}
