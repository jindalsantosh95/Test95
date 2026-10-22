using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;

namespace VAA.EAPI.Automation.Core.TestDataConfig
{
    public class MultiCity
    {
        public LocationType Location { get; set; }

        public string ReturnDate { get; set; }

        public MultiCity(Search search)
        {
            Location = LocationType.ATL;
            ReturnDate = DataGenerator.ReturnDate(search.ReturnDate);
        }
    }
}