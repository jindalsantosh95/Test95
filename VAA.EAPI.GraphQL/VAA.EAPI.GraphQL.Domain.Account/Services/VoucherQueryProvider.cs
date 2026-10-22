using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Mappers;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Domain.Account.Models.Transaction;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;


namespace VAA.EAPI.GraphQL.Domain.Account.Services
{
    public class VoucherQueryProvider : IVoucherQueryProvider
    {

        private readonly IRefDataService<ProductDetails> _productRefService;
        private readonly IRefDataService<PartnerProfileDetails> _partnerProfileService;
        private readonly ICustomerTransactionQueryProvider<TransactionResponse<TransactionDocument>> _customerTransTransaction;
        private readonly ICustomerTransactionQueryProvider<AwardResponse<Award>> _customerTransAwards;
        private readonly ILogger<VoucherQueryProvider> _logger;
        private readonly ICustomerTransactionQueryProvider<IEnumerable<VoucherDetail>> _customerTransVoucherDetails;

        public VoucherQueryProvider(ILogger<VoucherQueryProvider> logger, IRefDataService<ProductDetails> productRefService, IRefDataService<PartnerProfileDetails> partnerProfileService, ICustomerTransactionQueryProvider<TransactionResponse<TransactionDocument>> customerTransTransaction, ICustomerTransactionQueryProvider<AwardResponse<Award>> customerTransAwards, ICustomerTransactionQueryProvider<IEnumerable<VoucherDetail>> customerTransVoucherDetails)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _productRefService = productRefService ?? throw new ArgumentNullException(nameof(productRefService));
            _partnerProfileService = partnerProfileService ?? throw new ArgumentNullException(nameof(partnerProfileService));
            _customerTransTransaction = customerTransTransaction ?? throw new ArgumentNullException(nameof(customerTransTransaction));
            _customerTransAwards = customerTransAwards ?? throw new ArgumentNullException(nameof(customerTransAwards));
            _customerTransVoucherDetails = customerTransVoucherDetails ?? throw new ArgumentNullException(nameof(customerTransVoucherDetails));
        }

        public async Task<VoucherDetails> GetAccountMemberVouchers(string customerId, int page, int pageSize)
        {
            var voucherResponse = await _customerTransVoucherDetails.CustomerTransactionQuery(
                $"{ConfigReader.Get(Environment.CustTransactionMsUrl)}{Common.Constants.Application.GetVoucherDetails}",
                customerId);

            return voucherResponse is null
                ? new VoucherDetails()
                : VoucherMapper.VoucherResponseMapper(voucherResponse, page, pageSize);
        }

        public async Task<IEnumerable<Voucher>> GetVoucherQuery(string customerId, int page, int pageSize)
        {
            var awardResponse = await _customerTransAwards.CustomerTransactionQuery($"{ConfigReader.Get(Environment.CustTransactionMsUrl)}{Common.Constants.Application.GetAwardsDetails}", customerId);

            var awards = awardResponse?.awards;
            if (awards?.Any() != true)
            {
                return Enumerable.Empty<Voucher>();
            }

            var customerTransResponse = await _customerTransTransaction.CustomerTransactionQuery($"{ConfigReader.Get(Environment.CustTransactionMsUrl)}{Common.Constants.Application.GetTransactionDetails}", customerId);

            var productDetails = await _productRefService.Query() ?? Enumerable.Empty<ProductDetails>();
            var partnerProfile = await _partnerProfileService.Query() ?? Enumerable.Empty<PartnerProfileDetails>();

            var vouchers = VoucherMapper.VoucherResponseMapper(_logger, customerId, awards, customerTransResponse?.transactionDocuments, productDetails, partnerProfile, page, pageSize);

            return vouchers ?? Enumerable.Empty<Voucher>();
        }
    }
}