using CSharpFunctionalExtensions;
using System;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;
using VAA.EAPI.Infrastructure.PSS.Mapper;
using VAA.EAPI.Infrastructure.PSS.Repository.Command;
using VAA.EAPI.Microservices.Retail.Order.Management.Constants;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.Remarks;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Application.Services
{
    public class SpecialRemarksService : ISpecialRemarksService
    {
        private readonly ICachedChannel _cachedChannel;
        private readonly IAir4PnrCommonV6 _pnrCommonV6;
        private readonly IAir4RemarksV3 _remarksV3;

        public SpecialRemarksService(ICachedChannel cachedChannel, IAir4PnrCommonV6 pnrCommonV6, IAir4RemarksV3 remarksV3)
        {
            _cachedChannel = cachedChannel ?? throw new ArgumentNullException(nameof(cachedChannel));
            _pnrCommonV6 = pnrCommonV6 ?? throw new ArgumentNullException(nameof(pnrCommonV6));
            _remarksV3 = remarksV3 ?? throw new ArgumentNullException(nameof(remarksV3));
        }

        public async Task<Result<Response<string>>> AddAsync(AddSpecialRemarkRequest request, string channelId, CancellationToken cancellationToken)
        {
            if (request is not null && !IsVoucherRedemptionRemark(request))
            {
                return Result.Success(new Response<string> { Status = Status.OK, Value = "Remark subtype ignored." });
            }

            if (request is null || string.IsNullOrWhiteSpace(request.Pnr) || string.IsNullOrWhiteSpace(request.Location) || string.IsNullOrWhiteSpace(channelId) || string.IsNullOrWhiteSpace(request.VoucherDetails?.LoyaltyNumber) || request.VoucherDetails.TotalPoints is null || !IsSupportedRedemptionType(request.VoucherDetails.RedemptionType) || request.VoucherDetails.Vouchers is null || !request.VoucherDetails.Vouchers.Any() || request.VoucherDetails.Vouchers.Any(voucher => string.IsNullOrWhiteSpace(voucher?.VoucherNumber) || voucher.VoucherPointsApplied is null))
            {
                return MandatoryInputFieldsMissing();
            }

            var channel = await _cachedChannel.GetChannelEntity(channelId);
            if (channel is null)
            {
                return InvalidChannel();
            }

            string sessionId = null;
            var transactionId = AsyncContext.Current.CorrelationId.ToString();
            var setupContextRequest = PNRCommonV6ReqResMapper.PNRCommonV6Req(transactionId, request.Location, request.Pnr, channel);
            sessionId = Convert.ToString(SetupContext(setupContextRequest).SessionId);

            foreach (var voucher in request.VoucherDetails.Vouchers)
            {
                var voucherType = string.Equals(request.VoucherDetails.RedemptionType, "COMPANION", StringComparison.OrdinalIgnoreCase) ? "Companion" : "Upgrade";
                var remark = $"{Constants.Application.VoucherRedemptionSource} {request.VoucherDetails.LoyaltyNumber}. {voucherType} Vchr: {voucher.VoucherPointsApplied.Value} pts. Total pts: {request.VoucherDetails.TotalPoints.Value}";
                await _remarksV3.AddSpecialRemarkRequest(RemarksV3Mapper.MapSpecialRemarkRequest(new AddRemarksCommand { SessionId = sessionId, Remarks = remark }));
            }

            TearDownContext(PNRCommonV6ReqResMapper.TeardownContextRequest(sessionId, AsyncContext.Current.CorrelationId.ToString(), request.Location, channel));
            return Result.Success(new Response<string> { Status = Status.OK, Value = "Voucher remark added successfully." });
        }

        private static bool IsSupportedRedemptionType(string redemptionType) => string.Equals(redemptionType, "COMPANION", StringComparison.OrdinalIgnoreCase) || string.Equals(redemptionType, "UPGRADE", StringComparison.OrdinalIgnoreCase);

        private static bool IsVoucherRedemptionRemark(AddSpecialRemarkRequest request) => string.Equals(request.RemarkSubType, "VOUCHER_REDEMPTION", StringComparison.OrdinalIgnoreCase);

        private SetupContextResponseType SetupContext(SetupContextRequestType request)
        {
            try
            {
                return _pnrCommonV6.Air4SetupContext(request);
            }
            catch (FaultException exception)
            {
                throw Air4FaultExceptionChecker.PNRCommonV6Exception(exception, ErrorCodes.DeltaSetupContextErrorCode);
            }
        }

        private void TearDownContext(TeardownContextRequestType request)
        {
            try
            {
                _pnrCommonV6.Air4TearDownContext(request);
            }
            catch (FaultException exception)
            {
                throw Air4FaultExceptionChecker.PNRCommonV6Exception(exception, ErrorCodes.DeltaTearDownErrorCode);
            }
        }

        private static Result<Response<string>> MandatoryInputFieldsMissing() => Result.Success(new Response<string> { Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.MandatoryInputFieldsMissing), AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath), Status = Status.BusinessException });

        private static Result<Response<string>> InvalidChannel() => Result.Success(new Response<string> { Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidChannelErrorCode), AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath), Status = Status.BusinessException });
    }
}
