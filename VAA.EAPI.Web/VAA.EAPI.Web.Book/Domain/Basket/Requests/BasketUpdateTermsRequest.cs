using MediatR;
using VAA.EAPI.Web.Book.Models.YourDetails;
using VAA.EAPI.Web.Book.Models.YourDetails.Response;

namespace VAA.EAPI.Web.Book.Domain.Basket.Requests
{
    public class UpdateTnCRequest : BasketBase, IRequest<YourDetailsResponse>
    {
        public bool Accepted { get; set; }
        public TermsAndConditions TermsAndConditions { get; set; }
        public string Link { get; set; }
        public string Validation { get; set; }

        public UpdateTnCRequest()
        {

        }

        public UpdateTnCRequest(UpdateTermsRequest source)
        {
            BasketId = source.BasketId;
            TermsAndConditions = new TermsAndConditions()
            {
                Accepted = source.Accepted,
                Validation = source.Validation
            };
        }
    }
}
