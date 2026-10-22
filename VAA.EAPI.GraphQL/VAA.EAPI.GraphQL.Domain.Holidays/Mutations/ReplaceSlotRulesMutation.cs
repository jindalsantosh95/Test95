using System.Globalization;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;
using VAA.EAPI.GraphQL.Domain.Holidays.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Mutations;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class ReplaceSlotRulesMutation
{
    private readonly IStoreManagerServiceProvider _storeServiceProvider;

    public ReplaceSlotRulesMutation(IStoreManagerServiceProvider storeServiceProvider)
    {
        _storeServiceProvider = storeServiceProvider ?? throw new ArgumentNullException(nameof(storeServiceProvider));
    }

    [GraphQLName(GraphQlOperationNames.Mutations.StoreManagerReplaceSlotRules)]
    [GraphQLDescription("Replace slot rules for a store.")]
    [Authorize(Roles = new[] { Roles.StoreManager, Roles.StoreAdmin })]
    public async Task<IEnumerable<SlotRuleResponse>> ReplaceSlotRules(string storeId, SlotRuleReplaceRequest request)
    {
        ValidateRequest(request);
        return await _storeServiceProvider.ReplaceSlotRulesAsync(storeId, request);
    }

    private static void ValidateRequest(SlotRuleReplaceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Rules);

        foreach (var rule in request.Rules)
        {
            if (rule is null)
            {
                throw new RequestValidationException("Invalid slot rule: rule must not be null.");
            }

            if (rule.DayOfWeek is < 0 or > 6)
            {
                throw BuildValidationException(rule.DayOfWeek, "dayOfWeek must be between 0 and 6.");
            }

            if (rule.SlotDurationMinutes <= 0)
            {
                throw BuildValidationException(rule.DayOfWeek, "slotDurationMinutes must be greater than zero.");
            }

            if (rule.SlotCapacity <= 0)
            {
                throw BuildValidationException(rule.DayOfWeek, "slotCapacity must be greater than zero.");
            }

            if (!TimeSpan.TryParseExact(rule.StartTime, @"hh\:mm", CultureInfo.InvariantCulture, out var startTime)
                || !TimeSpan.TryParseExact(rule.EndTime, @"hh\:mm", CultureInfo.InvariantCulture, out var endTime))
            {
                throw BuildValidationException(rule.DayOfWeek, "startTime and endTime must be valid time values.");
            }

            var windowMinutes = (endTime - startTime).TotalMinutes;

            if (windowMinutes <= 0)
            {
                throw BuildValidationException(rule.DayOfWeek, "endTime must be after startTime.");
            }

            if (windowMinutes < rule.SlotDurationMinutes)
            {
                throw BuildValidationException(rule.DayOfWeek, "slotDurationMinutes must be less than or equal to the time window (endTime - startTime).");
            }

            if (windowMinutes % rule.SlotDurationMinutes != 0)
            {
                throw BuildValidationException(rule.DayOfWeek, "time window (endTime - startTime) must be evenly divisible by slotDurationMinutes.");
            }
        }
    }

    private static RequestValidationException BuildValidationException(int dayOfWeek, string reason)
    {
        return new RequestValidationException($"Invalid slot rule for dayOfWeek {dayOfWeek}: {reason}");
    }
}
