using System.Collections.Concurrent;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.OrderDataMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Handlers
{
    public class StampingPNRsHandler(IPnrQueryProvider PnrQueryProvider, IPnrDetailsProvider pnrDetailsProvider, IPnrUpdateMutationsProvider pnrUpdateMutationsProvider) : IStampingPNRsHandler
    {
        private readonly IPnrQueryProvider _pnrQueryProvider = PnrQueryProvider ?? throw new ArgumentNullException(nameof(PnrQueryProvider));
        private readonly IPnrDetailsProvider _pnrDetailsProvider = pnrDetailsProvider ?? throw new ArgumentNullException(nameof(pnrDetailsProvider));
        private readonly IPnrUpdateMutationsProvider _pnrUpdateMutationsProvider = pnrUpdateMutationsProvider ?? throw new ArgumentNullException(nameof(pnrUpdateMutationsProvider));

        public async Task StampPNRs(CustomerDTO customerResponse)
        {
            var customerPnrs = await _pnrQueryProvider.RetrievePNRsbyEmail(customerResponse.Customer.ContactDetails.EmailDetails.First(x => x.PreferredFlag == "Y").emailId);

            if (customerPnrs != null)
            {
                var eligiblePnrDetails = await GetPnrDetails(customerPnrs, customerResponse);

                if (eligiblePnrDetails.Any())
                    await _pnrUpdateMutationsProvider.StampingFCNumberToPNR(new UpdatePNRRequest { FCStampingDetails = eligiblePnrDetails });
            }
        }

        private async Task<IEnumerable<PNRDetailsforFCNumber>> GetPnrDetails(IEnumerable<string> customerPnrs, CustomerDTO customerResponse)
        {
            ConcurrentBag<PNRDetailsforFCNumber> eligiblePNRDetails = [];

            await Parallel.ForEachAsync(customerPnrs, async (pnr, CancellationToken) =>
            {
                PnrDetailsResponse pnrServiceResonse = await _pnrDetailsProvider.RetrievePNR(pnr);

                if (pnrServiceResonse?.PnrType?.Passengers != null)
                {
                    foreach (PassengerDetails psg in pnrServiceResonse.PnrType.Passengers)
                    {
                        if (psg.LastName.Replace(" ", "").Is(customerResponse.Customer.LastName.Replace(" ", "")) && (psg.FirstName.Replace(" ", "").Is(customerResponse.Customer.FirstName.Replace(" ", "") + customerResponse.Customer.MiddleName?.Replace(" ", "")) || psg.FirstName.Replace(" ", "").Is(customerResponse.Customer.FirstName.Replace(" ", "")) || psg.FirstName[..3].Is(customerResponse.Customer.FirstName[..3])))
                        {
                            if (pnrServiceResonse.PnrType.FrequentFlyer == null || !pnrServiceResonse.PnrType.FrequentFlyer.Any(x => x.FirstNameNumber == psg.FirstNameNumber && x.LastNameNumber == psg.LastNameNumber && x.AccountNumber != null))
                            {
                                eligiblePNRDetails.Add(new PNRDetailsforFCNumber
                                {
                                    FirstName = psg.FirstName,
                                    LastName = psg.LastName,
                                    MemberNumber = customerResponse.Customer.LoyaltyNumber,
                                    PNR = pnr
                                });
                            }
                        }
                    }
                }
            });

            return eligiblePNRDetails;
        }

    }
}
