using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Awards;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Reference;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Vouchers;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Helper.Caching.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Vouchers
{
    public class VouchersQueryHandler : IVouchersQueryHandler
    {
        private readonly AwardsQueryHandler _awardsQueryHandler;
        private readonly FlyingClubMemberQueryHandler _flyingClubMemberQueryHandler;
        private readonly TransactionDocumentQueryHandler _transactionDocumentQueryHandler;
        private readonly IReferenceService _referenceService;
        private readonly ILogger<VouchersQueryHandler> _logger;

        public VouchersQueryHandler(AwardsQueryHandler awardsQueryHandler, FlyingClubMemberQueryHandler flyingClubMemberQueryHandler, TransactionDocumentQueryHandler transactionDocumentQueryHandler, IReferenceService referenceService, ILogger<VouchersQueryHandler> logger)
        {
            _awardsQueryHandler = awardsQueryHandler;
            _flyingClubMemberQueryHandler = flyingClubMemberQueryHandler;
            _transactionDocumentQueryHandler = transactionDocumentQueryHandler;
            _referenceService = referenceService;
            _logger = logger;
        }

        public async Task<Result<Response<IEnumerable<VoucherResponse>>>> GetVouchersByCustomerIdAsync(VouchersQuery query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query.CustomerId))
                    return Result.Failure<Response<IEnumerable<VoucherResponse>>>("CustomerId is required");

                var awardsResult = await _awardsQueryHandler.Handle(new AwardsQuery { CustomerId = query.CustomerId, DocType = ConfigReader.Get(EnvironmentSettings.AwardDocType) }, CancellationToken.None);

                if (awardsResult.IsFailure)
                {
                    return Result.Failure<Response<IEnumerable<VoucherResponse>>>("Failed to retrieve awards");
                }

                var awards = awardsResult.Value.Value.Awards;

                var memberResult = await _flyingClubMemberQueryHandler.Handle(new FlyingClubMemberQuery { CustomerId = query.CustomerId, DocType = ConfigReader.Get(EnvironmentSettings.FlyingClubMemberDocType) }, CancellationToken.None);

                if (memberResult.IsFailure)
                {
                    return Result.Failure<Response<IEnumerable<VoucherResponse>>>("Failed to retrieve member details");
                }

                var member = memberResult.Value.Value.FlyingClubMemberDocuments.FirstOrDefault();
                var tier = member?.TierCode;

                var transResult = await _transactionDocumentQueryHandler.Handle(new TransactionQuery { CustomerId = query.CustomerId, DocType = ConfigReader.Get(EnvironmentSettings.TransactionDocType) }, CancellationToken.None);

                if (transResult.IsFailure)
                {
                    return Result.Failure<Response<IEnumerable<VoucherResponse>>>("Failed to retrieve transaction details");
                }

                var transactions = transResult.Value.Value.TransactionDocuments;

                var products = await _referenceService.GetAllProductsAsync();

                var partners = await _referenceService.GetAllPartnersAsync();

                var partnerProfileRefData = partners.ToList();

                bool isWingsMember = awards.Any(a =>
                {
                    if (string.IsNullOrWhiteSpace(a.ProductId))
                        return false;

                    var product = products.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.ProductId) && p.ProductId.Trim().Is(a.ProductId.Trim()));
                    
                    var productName = product?.ProductName;
                    if (string.IsNullOrWhiteSpace(productName) || !productName.Trim().Is(Constants.Application.WingsMemProductName))
                        return false;

                    return !a.ExpiryDate.HasValue || a.ExpiryDate.Value.Date >= DateTime.Now.Date;

                });


                var voucherAwards = awards
                    .Where(a => !string.IsNullOrWhiteSpace(a.ProductId))
                    .Where(a =>
                    {
                        var product = products.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.ProductId) && p.ProductId.Trim().Equals(a.ProductId.Trim(), StringComparison.OrdinalIgnoreCase));
                        return product != null &&
                              string.Equals(product.ProductType, Constants.Application.ProductTypeElectronicVoucher, StringComparison.OrdinalIgnoreCase);
                    })
                    .ToList();

                var vouchers = voucherAwards.Select(a =>
                {
                    var product = products.First(p => p.ProductId != null && a.ProductId != null && p.ProductId.Trim().Equals(a.ProductId.Trim(), StringComparison.OrdinalIgnoreCase));

                    return new VoucherResponse
                    {
                        VoucherName = product.ProductName,
                        VoucherNumber = a.AwardNumber,
                        Description = product.ProductDescription,
                        Status = GetVoucherStatus(a.ExpiryDate, a.Status),
                        StartDate = a.StartDate?.Date.ToString(Constants.Application.ISODateFormat),
                        ExpiryDate = a.ExpiryDate?.Date.ToString(Constants.Application.ISODateFormat),
                        UsedDate = a.UsedDate?.Date.ToString(Constants.Application.ISODateFormat),
                        DateGifted = a.GiftedAwardDate?.Date.ToString(Constants.Application.ISODateFormat),
                        Source = string.IsNullOrEmpty(a.TransactionId) ? Constants.Application.VirginMoney : GetSourceFromPartnerProfile(a.TransactionId, partnerProfileRefData, transactions),
                        MaxValue = VoucherValueHelper.GetTierBasedValue(tier, a.Status, isWingsMember),
                        Category = product.CategoryCode,
                        IsGifted = a.GiftedAwardDate.HasValue && !string.IsNullOrWhiteSpace(a.GiftedFromCustomerId)
                    };
                }).ToList();

                return Result.Success(new Response<IEnumerable<VoucherResponse>>
                {
                    Status = Status.OK,
                    Value = vouchers
                });

            }

            catch (System.Exception ex)
            {
                _logger.LogError($"Error in GetVouchersByCustomerIdAsync");
                return Result.Failure<Response<IEnumerable<VoucherResponse>>>("An error occurred while retrieving vouchers.");

            }
        }

        private string GetVoucherStatus(DateTime? expiryDate, string oldStatus)
        {
            if (string.IsNullOrWhiteSpace(oldStatus))
                return oldStatus;

            var normalized = oldStatus.Trim().ToUpperInvariant();

            if (normalized == Constants.Application.Available.ToUpperInvariant() || normalized == Constants.Application.PartiallyUsed.ToUpperInvariant())
            {
                if (expiryDate.HasValue && DateTime.Now.Date > expiryDate.Value.Date)
                    return Constants.Application.Expired;

                return oldStatus;
            }

            return oldStatus;
        }

        private static string GetSourceFromPartnerProfile(string transactionId, IEnumerable<Partner> partnerProfileDetails, IEnumerable<TransactionDocument> transactions)
        {
            var txn = transactions.FirstOrDefault(t => t.TransactionId == transactionId);

            if (txn == null)
                return Constants.Application.VirginMoney;

            var partnerId = txn.PartnerId;

            if (string.IsNullOrWhiteSpace(partnerId))
                return Constants.Application.VirginMoney;

            var partner = partnerProfileDetails.FirstOrDefault(p => p.PartnerId == partnerId);

            return partner?.PartnerName ?? Constants.Application.VirginMoney;
        }
    }
}
