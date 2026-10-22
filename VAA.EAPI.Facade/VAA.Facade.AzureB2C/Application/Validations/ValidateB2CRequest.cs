using MediatR;

namespace VAA.Facade.AzureB2C.Application.Validations
{
    class ValidateB2CRequest : IRequest<bool>
    {
        public string B2CRequest { get; set; }

        public ValidateB2CRequest(string b2cRequest)
        {
            B2CRequest = b2cRequest;
        }
    }
}
