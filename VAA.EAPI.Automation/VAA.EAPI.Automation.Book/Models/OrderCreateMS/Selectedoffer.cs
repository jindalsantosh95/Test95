namespace VAA.EAPI.Automation.Book.Models.OrderCreateMS
{
    public class Selectedoffer
    {
        public string offerid { get; set; }
        public IEnumerable<Offeritem> offeritems { get; set; }
    }
}
