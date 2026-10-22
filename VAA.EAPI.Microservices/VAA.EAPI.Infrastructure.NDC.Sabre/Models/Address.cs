namespace VAA.EAPI.Infrastructure.NDC.Sabre.Models
{
    public class Address
    {
        public string AddressLine { get; set; }
        public string CityName { get; set; }
        public string CountryCode { get; set; }
        public string PostalCode { get; set; }
        public StateCountyProv StateCountyProv { get; set; }
        public string StreetNmbr { get; set; }
        public VendorPrefs VendorPrefs { get; set; }
    }
}
