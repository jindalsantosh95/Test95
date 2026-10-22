namespace VAA.EAPI.Microservices.Data.Egress.Models
{
    public class Airport
    {
        // PROVISIONAL MODEL - AWAITING CONCRETE DEFINITION FROM DATA & INSIGHTS TEAM

        public string APTIATACODE { get; set; }
        public string APTICAOCODE { get; set; }
        public string APTNAME { get; set; }
        public float? LATITUDE { get; set; }
        public float? LONGITUDE { get; set; }
        public string CITYIATACODE { get; set; }
        public string CITYNAME { get; set; }
        public string CTRYCODE { get; set; }
        public string CTRYNAME { get; set; }

        // Override ToString method
        public override string ToString()
        {
            return $"APTIATACODE: {APTIATACODE}, APTICAOCODE: {APTICAOCODE}, APTNAME: {APTNAME}, " +
                   $"LATITUDE: {LATITUDE}, LONGITUDE: {LONGITUDE}, CITYIATACODE: {CITYIATACODE}, " +
                   $"CITYNAME: {CITYNAME}, CTRYCODE: {CTRYCODE}, CTRYNAME: {CTRYNAME}";
        }

    }
}
