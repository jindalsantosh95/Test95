using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.VSRetro;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Command.VSRetro
{
    public class UpdateVSRetroFlownDetailsCommand : IRequest<Result<Response<UpdateVSFlownDetailsResponse>>>
    {
        public UpdateVSFlownDetailsRequest Request { get; }

        public UpdateVSRetroFlownDetailsCommand(UpdateVSFlownDetailsRequest request)
        {
            Request = request ?? throw new ArgumentNullException(nameof(request));
        }
    }
}
