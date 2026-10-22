namespace VAA.EAPI.GraphQL.Domain.VHols.Models.BookingResponse
{
    public class ProductContent
    {
        public string EcmsId { get; set; }
        public string TbxId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Errata { get; set; }
        public string Summary { get; set; }
        public string AdditionalInformation { get; set; }
        public List<Image> Images { get; set; }
        public List<string> ThemeTags { get; set; }
    }
}

