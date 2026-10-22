using System;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Mapper.CreateToken;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public class CreateTokenCommandHandler(ICreateToken createToken) : ICreateTokenCommand
    {
        private readonly ICreateToken _createToken = createToken ?? throw new ArgumentNullException(nameof(createToken));

        public async Task<string> Handle(string pcc)
        {
            var response = await _createToken.CreateTokenAsync(CreateTokenMapper.RequestMapper(pcc));

            return response?.Security?.BinarySecurityToken?.Value;
        }
    }
}
