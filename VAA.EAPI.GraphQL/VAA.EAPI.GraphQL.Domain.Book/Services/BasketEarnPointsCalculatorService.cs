using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Book.Interfaces;
using VAA.EAPI.GraphQL.Domain.Book.Mappers;
using VAA.EAPI.GraphQL.Domain.Book.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Offer;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Book.Services
{
    public class BasketEarnPointsCalculatorService(ICustomerProvider customerProvider, IInRuleServiceProvider inRuleServiceProvider, IBasketServiceProvider basketServiceProvider) : IBasketEarnPointsCalculatorService
    {
        private readonly ICustomerProvider _customerProvider = customerProvider;
        private readonly IInRuleServiceProvider _inRuleServiceProvider = inRuleServiceProvider;
        private readonly IBasketServiceProvider _basketServiceProvider = basketServiceProvider;

        public async Task<FCEarnPoints> Calculate(Basket basket, OfferResponse offer, CustomerDTO? leadCustomer)
        {
            if (basket.FlightsearchRequest.isAwardSearch)
                return basket.EarnPoints;

            int groupVirginPoints = 0;
            int virginPoints = 0;
            int tierPoints = 0;

            var tiers = basket.EarnPointPerTier?.Tiers;

            foreach (var person in basket.People.Where(x => !string.Equals(x.Type, Constants.Application.InfantPtc, StringComparison.OrdinalIgnoreCase)))
            {
                var fqtProgramme = person.MembershipDetails?.FQTProgramme;
                if (!string.IsNullOrWhiteSpace(fqtProgramme) && !fqtProgramme.Is(Constants.Application.VSFQTProgramme)) 
                    continue;

                var fcNumber = person.MembershipDetails?.FQTNumber;

                CustomerDTO customer;

                if (leadCustomer != null && string.Equals(leadCustomer.Customer.LoyaltyNumber, fcNumber, StringComparison.OrdinalIgnoreCase))
                {
                    customer = leadCustomer;
                }
                else
                {
                    customer = string.IsNullOrWhiteSpace(fcNumber) ? null : await _customerProvider.GetCustomer(null, fcNumber);
                }

                var earnVirginPoints = tiers?.FirstOrDefault(x => x.TierType.Equals(customer?.Customer?.MemberTier ?? "Red", StringComparison.OrdinalIgnoreCase));

                if (earnVirginPoints != null)
                {
                    if(person.Id == "0")
                    {
                        virginPoints = earnVirginPoints.VirginPoints;
                        tierPoints = earnVirginPoints.TierPoints;
                    }
                    groupVirginPoints += earnVirginPoints.VirginPoints;
                    continue;
                }

                var ruleResult = await _inRuleServiceProvider.AirlinePointsEarnCalculator(InRuleRequestMapper.MapPointsEarnCalculatorRequest(offer, customer));

                if (person.Id == "0")
                {
                    virginPoints = ruleResult?.SegmentResponses?.Sum(x => x.PointsAward) ?? 0;
                    tierPoints = ruleResult?.SegmentResponses?.Sum(x => x.TierPointsAward) ?? 0;
                }
                groupVirginPoints += ruleResult?.SegmentResponses?.Sum(x => x.PointsAward) ?? 0;
            }

            var basketResponse = await _basketServiceProvider.UpdateFCEarnPointsAsync(basket.BasketId.ToString(), new FCEarnPoints()
            {
                EarnVirginPoints = virginPoints,
                EarnTierPoints = tierPoints,
                EarnGroupVirginPoints = groupVirginPoints
            });

            return basketResponse?.EarnPoints;
        }
    }
}