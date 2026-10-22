using VAA.EAPI.Web.Book.Models.Book.Confirmation;

namespace VAA.EAPI.Web.Book.Mapper
{
    public class CreateOrderMapper
    {
        public static ConfirmationResponse MapOrderCreateResponse(ConfirmationResponse confirmationResponse)
        {
            confirmationResponse.People = BasketMapper.PeopleMapper(confirmationResponse.People);

            return confirmationResponse;
        }
    }
}
