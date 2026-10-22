using System.Collections.Generic;

namespace VAA.EAPI.Web.Book.Models.Book.Confirmation
{
    public class Includes
    {
        public bool Hotel { get; set; }
        public bool Villa { get; set; }
        public bool Flight { get; set; }
        public bool Car { get; set; }
        public bool Transfer { get; set; }
        public bool Extras { get; set; }
        public bool DisneyExtras { get; set; }
        public bool VRoom { get; set; }
        public IEnumerable<object> Additional { get; set; }
    }
}
