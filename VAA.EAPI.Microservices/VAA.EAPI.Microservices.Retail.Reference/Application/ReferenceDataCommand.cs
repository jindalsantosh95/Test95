using MediatR;
using VAA.EAPI.Microservices.Retail.Reference.Domain;

namespace VAA.EAPI.Microservices.Retail.Reference.Application
{
    public class ReferenceDataCommand : IRequest<string>
    {
        public ReferenceDataTypeEnum ReferenceDataType { get; set; }
    }
}
