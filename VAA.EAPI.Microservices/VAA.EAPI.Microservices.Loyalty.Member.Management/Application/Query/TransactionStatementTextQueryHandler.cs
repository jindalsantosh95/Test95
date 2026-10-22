using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Constants;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.TransactionStatement;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Validations;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query
{
    public class TransactionStatementTextQueryHandler : IRequestHandler<TransactionStatementTextQuery, Result<Response<StatementTextResponse>>>
    {
        private readonly ILogger<TransactionStatementTextQueryHandler> _logger;
        private readonly ITxnStatementTextService _txnStmtTextService;

        public TransactionStatementTextQueryHandler(ILogger<TransactionStatementTextQueryHandler> logger, ITxnStatementTextService txnStmtTextService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _txnStmtTextService = txnStmtTextService ?? throw new ArgumentNullException(nameof(txnStmtTextService));
        }

        public async Task<Result<Response<StatementTextResponse>>> Handle(TransactionStatementTextQuery request, CancellationToken cancellationToken)
        {
            try
            {
                if (new TransactionStatementTextRequestValidator().Validate(request).IsValid)
                {
                    return Result.Success(new Response<StatementTextResponse>
                    {
                        Status = Status.OK,
                        Value = await _txnStmtTextService.GetTransactionStatementText(request)
                    });
                }
                else
                {
                    return Result.Success(new Response<StatementTextResponse>
                    {
                        Status = Status.BusinessException,
                        Value = null,
                        Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.MandatoryInputFieldsMissing), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error has occured in Generate Transaction Statement Query Handler");
                return Result.Failure<Response<StatementTextResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }

    }

}

