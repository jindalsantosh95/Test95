using System.Globalization;
using HotChocolate;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Services;

public static class StandardEnrolmentAgePolicy
{
    private const int Under18AgeLimit = 18;
    private static readonly string[] SupportedDateOfBirthFormats = ["yyyy-MM-dd", "MM/dd/yyyy"];

    public static void EnsureEligible(CustomerAccountDetails customerDetails)
    {
        if (customerDetails is null)
        {
            throw CreateError(
                VAA.EAPI.GraphQL.Common.Constants.Application.MandatoryInputFieldsMissingCode,
                "Customer details are required.");
        }

        if (!DateOnly.TryParseExact(customerDetails.DateOfBirth, SupportedDateOfBirthFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOfBirth))
        {
            throw CreateError(
                VAA.EAPI.GraphQL.Common.Constants.Application.MandatoryInputFieldsMissingCode,
                "Date of birth must be a valid date in yyyy-MM-dd or MM/dd/yyyy format.");
        }

        var under18EnrolmentBlockEnabled = ConfigReader.Try<bool>(Environment.EnableUnder18StandardEnrolmentBlock, false);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (dateOfBirth > today || (under18EnrolmentBlockEnabled && dateOfBirth.AddYears(Under18AgeLimit) > today))
        {
            throw CreateError(
                Constants.Application.Under18StandardEnrolmentErrorCode,
                Constants.Application.Under18StandardEnrolmentErrorMessage,
                "loginToCreateYoungFlyer");
        }

        // Customer MS and Identity require the same unambiguous ISO date.
        customerDetails.DateOfBirth = dateOfBirth.ToString(Common.Constants.Application.ISODateFormat, CultureInfo.InvariantCulture);
    }

    private static GraphQLException CreateError(string code, string message, string nextAction = null) =>
        new(ErrorBuilder.New()
            .SetMessage(message)
            .SetCode(code)
            .SetExtension("code", code)
            .SetExtension("message", message)
            .SetExtension("source", VAA.EAPI.GraphQL.Common.Constants.Application.ErrorSource)
            .SetExtension("nextAction", nextAction)
            .Build());
}
