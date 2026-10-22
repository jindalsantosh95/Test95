/**
* The flight search slice fare content
*
* @author Gareth Williams
*/
namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFResponse
{
    public class SliceFareContent
    {

        public string cabinName { get; set; }

        public IEnumerable<SliceFareContentFeature> features { get; set; }

    }
}
