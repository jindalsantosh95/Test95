using VAA.EAPI.GraphQL.Domain.Account.Helpers;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Account.Models.Transaction;

namespace VAA.EAPI.GraphQL.Domain.Account.Mappers
{
    public class TransactionMapper
    {
        public static CreateTransactionRequest MapToEarnTransactionResuest(TransactionMutationRequest transactionMutationRequest, string memberNumber, ProductDetails product, PartnerProfileDetails partner)
        {
            if (transactionMutationRequest is null) throw new ArgumentNullException(nameof(transactionMutationRequest));
            if (memberNumber is null) throw new ArgumentNullException(nameof(memberNumber));

            return new CreateTransactionRequest()
            {
                MemberNumber = memberNumber,
                PartnerCode = Common.Constants.Application.RetailTherapyPartnerCode,
                ProductName = Common.Constants.Application.RetailTherapyProductName,
                ActivityDate = transactionMutationRequest.ActivityDate?.ToString(Common.Constants.Application.FEAPIDateFormat),
                Comments = string.Concat(transactionMutationRequest.Comment, transactionMutationRequest.OrderId),
                Points = transactionMutationRequest.Points,
                TransactionId = $"AZ{Utility.GetVredTransactionId()}",
                PartnerName = partner?.Name,
                PartnerCategory = partner?.PartnerType,
                ProductDescription = product?.Description,
                TransactionSource = Common.Constants.Application.CreatedUser,
                TransactionSubType = Common.Constants.Application.ActivitySubtype,
                TransactionChannel = Common.Constants.Application.PAUser,
                ActivityType = Common.Constants.Application.AccrualTransactionType,
                Status = Common.Constants.Application.TxnProcessingStatus,
                CreatedBy = Common.Constants.Application.CreatedUser,
                UpdatedBy = Common.Constants.Application.CreatedUser,
                ProductCategory = product?.ProductCategory,
                StatementText = product?.Description,
            };
        }

        public static CreateTransactionRequest MapToBurnTransactionResuest(TransactionMutationRequest transactionMutationRequest, string memberNumber, ProductDetails product, PartnerProfileDetails partner)
        {
            if (transactionMutationRequest is null) throw new ArgumentNullException(nameof(transactionMutationRequest));
            if (memberNumber is null) throw new ArgumentNullException(nameof(memberNumber));

            return new CreateTransactionRequest()
            {
                MemberNumber = memberNumber,
                PartnerCode = Common.Constants.Application.RetailTherapyPartnerCode,
                ProductName = Common.Constants.Application.RetailTherapyProductName,
                ActivityDate = transactionMutationRequest.ActivityDate?.ToString(Common.Constants.Application.FEAPIDateFormat),
                Comments = string.Concat(transactionMutationRequest.Comment, transactionMutationRequest.OrderId),
                Points = transactionMutationRequest.Points,
                TransactionId = $"AZ{Utility.GetVredTransactionId()}",
                ActivityType = Common.Constants.Application.RedemptionTransactionType,
                TransactionSubType = Common.Constants.Application.ActivitySubtype,
                TransactionChannel = Common.Constants.Application.PAUser,
                CreatedBy = Common.Constants.Application.CreatedUser,
                UpdatedBy = Common.Constants.Application.CreatedUser,
                TransactionSource = Common.Constants.Application.CreatedUser,
                Status = Common.Constants.Application.TxnProcessingStatus,
                ProductDescription = product?.Description,
                PartnerName = partner?.Name,
                PartnerCategory = partner?.PartnerType,
                ProductCategory = product?.ProductCategory,
                StatementText = product?.Description,

            };
        }
    }
}
