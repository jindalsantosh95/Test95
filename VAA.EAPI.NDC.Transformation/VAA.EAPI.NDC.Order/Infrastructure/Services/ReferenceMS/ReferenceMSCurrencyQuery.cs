using MediatR;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.ReferenceMS
{
    public class ReferenceMSCurrencyQuery : IRequest<string>
    {
        public string ReferenceDataType { get; set; }
    }
}
