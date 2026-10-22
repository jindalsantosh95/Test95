using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedAirBook;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public class EnhancedAirBookCommandHandler : IRequestHandler<EnhancedAirBookCommand, EnhancedAirBookRQResponse>
    {
        private readonly IEnhancedAirBook _enhancedAirBook;

        public EnhancedAirBookCommandHandler(IEnhancedAirBook enhancedAirBook)
        {
            _enhancedAirBook = enhancedAirBook ?? throw new ArgumentNullException(nameof(enhancedAirBook));
        }

        public async Task<EnhancedAirBookRQResponse> Handle(EnhancedAirBookCommand request, CancellationToken cancellationToken)
        {
            return await _enhancedAirBook.EnhancedAirBookAsync(request.EnhancedAirBookRequest);
        }
    }
}
