using VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Domain;
using VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Mapper;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Application.Command
{
    public class TransactionCommandHandler : ITransactionCommand 
    {
        private readonly ITransactionService _transactionService;

        public TransactionCommandHandler(ITransactionService transactionService)
        {
            _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
        }

        public async Task<PaymentTransactionResponse> Handle(TransactionRequest request)
        {
            var arcTransactionRequest = TransactionMapper.MapArcTranctionRequest(request);

            var response = await _transactionService.Handle(arcTransactionRequest);

            return TransactionResponseMapper.Map(response);
        }
    }
}
