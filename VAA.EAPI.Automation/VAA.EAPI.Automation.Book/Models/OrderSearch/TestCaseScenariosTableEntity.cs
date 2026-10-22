using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Automation.Book.Models.OrderSearch
{
    public class TestCaseScenariosTableEntity : TableEntity
    {
        public string PNR { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
