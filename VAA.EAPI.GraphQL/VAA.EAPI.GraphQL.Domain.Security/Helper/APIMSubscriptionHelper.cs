using FluentValidation.Results;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Security.Models.NDC;

namespace VAA.EAPI.GraphQL.Domain.Security.Helper
{
    public static class APIMSubscriptionHelper
    {
        public static string GetSubscriptionId(NDCAgencyDetails? agencyDetails, NDCAggregatorDetails? aggregatorDetails)
        {
            if (agencyDetails is not null && aggregatorDetails is null)
                return agencyDetails.AgencyId!;
            if (agencyDetails is null && aggregatorDetails is not null)
                return aggregatorDetails.AggregatorId!;
            if (agencyDetails is not null && aggregatorDetails is not null)
                return $"{agencyDetails.AgencyId}-{aggregatorDetails.AggregatorId}";
            throw new NotImplementedException();
        }

        public static string GetSubscriptionName(NDCAgencyDetails? agencyDetails, NDCAggregatorDetails? aggregatorDetails)
        {
            if (agencyDetails is not null && aggregatorDetails is null)
                return agencyDetails.AgencyName!;
            if (agencyDetails is null && aggregatorDetails is not null)
                return aggregatorDetails.AggregatorName!;
            if (agencyDetails is not null && aggregatorDetails is not null)
                return $"{agencyDetails.AgencyName}-{aggregatorDetails.AggregatorName}";
            throw new NotImplementedException();

        }

        public static List<IError> BuildGraphQLRequestValidationErrors(List<ValidationFailure> validationErrors)
        {
            List<IError> errors = new();
            validationErrors.ForEach(err =>
            {
                var builder = ErrorBuilder.New()
                                      .SetMessage(err.ErrorMessage);
                errors.Add(builder.Build());
            });
            return errors;
        }

        public static IError BuildGraphQLRequestError(string message, ErrorDetails error)
        {
            var builder = ErrorBuilder.New()
                                      .SetMessage(message);
            builder.SetExtension("code", error.Code);
            builder.SetExtension("message", error.Message);
            return builder.Build();
        }
    }
}
