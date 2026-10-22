using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Infrastructure.Controllers.Extensions.OpenAPI;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Vouchers;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Vouchers;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Vouchers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Triggers.Http
{
    public class VoucherController
    {
        private readonly IVouchersQueryHandler _vouchersQueryHandler;
        private readonly IRedeemVoucherCommandHandler _redeemVoucherCommandHandler;
        private readonly IVoucherRelatedTransactionQueryHandler _voucherRelatedQueryHandler;
        private readonly IRollbackVoucherRdmCommandHandler _rollbackVoucherRdmCommandHandler;
        private readonly IPatchVoucherRelatedTxnsPnrCommandHandler _patchVoucherRelatedTxnsPnrCommandHandler;
        private readonly IVoucherTxnsPnrUpdateCommandStatusQuery _voucherTxnsPnrUpdateCommandStatusQuery;

        public VoucherController(IVouchersQueryHandler vouchersQueryHandler, IRedeemVoucherCommandHandler redeemVoucherCommandHandler, IVoucherRelatedTransactionQueryHandler voucherRelatedQueryHandler, IRollbackVoucherRdmCommandHandler rollbackVoucherRdmCommandHandler, IPatchVoucherRelatedTxnsPnrCommandHandler patchVoucherRelatedTxnsPnrCommandHandler, IVoucherTxnsPnrUpdateCommandStatusQuery voucherTxnsPnrUpdateCommandStatusQuery)
        {
            _vouchersQueryHandler = vouchersQueryHandler ?? throw new ArgumentNullException(nameof(vouchersQueryHandler));
            _redeemVoucherCommandHandler = redeemVoucherCommandHandler ?? throw new ArgumentNullException(nameof(redeemVoucherCommandHandler));
            _voucherRelatedQueryHandler = voucherRelatedQueryHandler ?? throw new ArgumentNullException(nameof(voucherRelatedQueryHandler));
            _rollbackVoucherRdmCommandHandler = rollbackVoucherRdmCommandHandler ?? throw new ArgumentNullException(nameof(rollbackVoucherRdmCommandHandler));
            _patchVoucherRelatedTxnsPnrCommandHandler = patchVoucherRelatedTxnsPnrCommandHandler ?? throw new ArgumentNullException(nameof(patchVoucherRelatedTxnsPnrCommandHandler));
            _voucherTxnsPnrUpdateCommandStatusQuery = voucherTxnsPnrUpdateCommandStatusQuery ?? throw new ArgumentNullException(nameof(voucherTxnsPnrUpdateCommandStatusQuery));
        }

        [OpenApiOperation(operationId: "GetVouchersByCustID", tags: new[] { "Customer Transactions Microservice -Vouchers" }, Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<VoucherResponse>), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "customerID", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GetVouchersByCustID")]
        public async Task<HttpResponseData> GetVouchersByCustID([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{customerID}/vouchers")] HttpRequestData req, string customerID)
        {
            var query = new VouchersQuery() { CustomerId = customerID };
            var commandOutput = await _vouchersQueryHandler.GetVouchersByCustomerIdAsync(query);
            return await ResultExtention<IEnumerable<VoucherResponse>>.ExecuteResultAsync(req, commandOutput);
        }


        [OpenApiOperation(operationId: "RedeemVoucher", tags: new[] { "Customer Transactions Microservice - Redeem Voucher" }, Summary = "Redeem Voucher", Description = "Http endpoint to redeem a voucher for Member", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(RedeemVoucherRequest), Description = "")]
        [OpenApiParameter(name: "customerID", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer id")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("RedeemVoucher")]
        public async Task<HttpResponseData> RedeemVoucher([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{customerID}/redeemVoucher")] HttpRequestData req, string customerID, CancellationToken cancellationToken)
        {
            var command = Json.Deserialize<RedeemVoucherRequest>(await new StreamReader(req.Body).ReadToEndAsync());

            if (command is null) throw new ArgumentNullException(nameof(command));

            command.CustomerId = customerID;

            return await ResultExtention<TransactionCommandResponse>.ExecuteResultAsync(req, await _redeemVoucherCommandHandler.Handle(new RedeemVoucherCommand(command), cancellationToken));
        }

        [OpenApiOperation(operationId: "GetVoucherRelatedTransactionsToCancel", tags: new[] { "Customer Transactions Microservice - Transaction Document" }, Summary = "Retrieve voucher related transactions to cancel", Description = "Http endpoint to retrieve voucher transactions to cancel", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(TransactionResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "customerID", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer ID")]
        [OpenApiParameter(name: "awardId", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "Award Id")]
        [OpenApiParameter(name: "voucherNumber", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "Voucher Number")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GetVoucherRelatedTransactionsToCancel")]
        public async Task<HttpResponseData> GetVoucherRelatedTransactionsToCancel([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{customerID}/VoucherRelatedTxns")] HttpRequestData req, string customerID)
        {

            var queryParams = System.Web.HttpUtility.ParseQueryString(req.Url.Query);

            var voucherNumber = queryParams["voucherNumber"];
            var awardId = queryParams["awardId"];

            var commandOutput = await _voucherRelatedQueryHandler.GetVoucherRelatedTransactions(new VoucherRelatedTransactionQuery { CustomerId = customerID, AwardId = awardId, VoucherNumber = voucherNumber });
            return await ResultExtention<TransactionResponse>.ExecuteResultAsync(req, commandOutput);

        }

        [OpenApiOperation(operationId: "RollbackVoucherRedemption", tags: new[] { "Customer Transactions Microservice - Voucher Redemption Rollback" }, Summary = "Voucher Redemption Rollback", Description = "Http endpoint to rollback a voucher redemption for Member", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(RollbackVoucherRdmRequest), Description = "")]
        [OpenApiParameter(name: "customerID", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer id")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("RollbackVoucherRedemption")]
        public async Task<HttpResponseData> RollbackVoucherRedemption([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "{customerID}/rollbackVoucherRedemption")] HttpRequestData req, string customerID, CancellationToken cancellationToken)
        {
            var command = Json.Deserialize<RollbackVoucherRdmRequest>(await new StreamReader(req.Body).ReadToEndAsync());

            if (command is null) throw new ArgumentNullException(nameof(command));

            command.CustomerId = customerID;

            return await ResultExtention<TransactionCommandResponse>.ExecuteResultAsync(req, await _rollbackVoucherRdmCommandHandler.Handle(new RollbackVoucherRdmCommand(command), cancellationToken));
        }

        [OpenApiOperation(operationId: "PatchVoucherRelatedTransactionsPnr", tags: new[] { "Customer Transactions Microservice - Patch Voucher Related Transactions Pnr" }, Summary = "Patch Voucher Related Transactions Pnr", Description = "Http endpoint to update Pnr on voucher related transactions for Member", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiRequestBody("application/json", typeof(PatchVoucherRelatedTxnsPnrRequest), Description = "PatchRequest")]
        [OpenApiParameter(name: "customerID", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Customer id")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Accepted, Summary = "Request accepted", Description = "Request accepted")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("PatchVoucherRelatedTransactionsPnr")]
        public async Task<HttpResponseData> PatchVoucherRelatedTransactionsPnr([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "{customerID}/voucherRelatedTxnsPnr")] HttpRequestData req, string customerID, CancellationToken cancellationToken)
        {
            var command = Json.Deserialize<PatchVoucherRelatedTxnsPnrRequest>(await new StreamReader(req.Body).ReadToEndAsync());

            if (command is null) throw new ArgumentNullException(nameof(command));

            command.CustomerId = customerID;

            return await ResultExtention<TransactionCommandResponse>.ExecuteResultAsync(req, await _patchVoucherRelatedTxnsPnrCommandHandler.Handle(new PatchVoucherRelatedTxnsPnrCommand(command), cancellationToken));
        }

        [OpenApiOperation(operationId: "GetStatusOfVoucherTxnsPnrUpdateCommand", tags: new[] { "Customer Transactions Microservice - Get Status Of Voucher Txns Pnr Update" }, Summary = "Retrieve status of Voucher Related Txns Pnr Update", Description = "Http enpoint to retrieve status of Voucher Related Txns Pnr Update", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiSecurity("OAuth2Client", SecuritySchemeType.OAuth2, Flows = typeof(OpenApiAuth))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(VoucherTxnsPnrUpdateCommandQueryResponse), Summary = "OK", Description = "OK")]
        [OpenApiParameter(name: "requestId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Command Correlation ID")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unauthorized", Description = "Unauthorized")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Forbidden", Description = "Forbidden")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnprocessableEntity, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Unprocessable Entity", Description = "Business Validation")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(List<ErrorDetails>), Summary = "Internal Server Error", Description = "Internal Server Error")]
        [Function("GetStatusOfVoucherTxnsPnrUpdateCommand")]
        public async Task<HttpResponseData> GetStatusOfVoucherTxnsPnrUpdateCommand([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "updateVoucherRelatedTxnsPnr/status/{requestId}")] HttpRequestData req, string requestId, CancellationToken cancellationToken)
        {
            var commandOutput = await _voucherTxnsPnrUpdateCommandStatusQuery.Handle(new VoucherTxnsPnrUpdateCommandStatusQuery { RequestId = requestId }, cancellationToken);

            return await ResultExtention<VoucherTxnsPnrUpdateCommandQueryResponse>.ExecuteResultAsync(req, commandOutput);
        }
    }
}
