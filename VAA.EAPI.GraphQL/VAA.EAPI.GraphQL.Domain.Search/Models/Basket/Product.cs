namespace VAA.EAPI.GraphQL.Domain.Search.Models.Basket
{
    public class Product
    {
        public bool Hotel { get; set; }
        public bool Villa { get; set; }
        public bool Flight { get; set; }
        public bool Car { get; set; }
        public bool Transfer { get; set; }
        public bool Extras { get; set; }
        public bool DisneyExtras { get; set; }
        public bool VRoom { get; set; }
        public IEnumerable<string> Additional { get; set; }
    }
}