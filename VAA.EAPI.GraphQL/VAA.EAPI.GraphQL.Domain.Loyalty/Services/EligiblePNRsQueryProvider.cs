using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.FutureFlights;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PNR;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.StampFCNumToPNR;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class EligiblePNRsQueryProvider : IEligiblePNRsQueryProvider
    {
        private readonly IFutureFlightsQueryProvider _futureFlightsrService;
        private readonly IPNRQueryProvider _pnrQueryProvider;

        public EligiblePNRsQueryProvider(IFutureFlightsQueryProvider futureFlightsrService, IPNRQueryProvider pnrQueryProvider)
        {
            _futureFlightsrService = futureFlightsrService ?? throw new ArgumentNullException(nameof(futureFlightsrService));
            _pnrQueryProvider = pnrQueryProvider ?? throw new ArgumentNullException(nameof(pnrQueryProvider));
        }

        public async Task<IEnumerable<FCNumToPNRDetails>> GetEligiblePNRs(GetEligiblePNRsRequest request)
        {
            if (string.IsNullOrEmpty(request.CustomerDetails.ContactDetails.EmailDetails.FirstOrDefault(x => x.PreferredFlag == "Y").EmailId) || string.IsNullOrEmpty(request.CustomerDetails.FirstName) || string.IsNullOrEmpty(request.CustomerDetails.LastName))
                throw new RequestValidationException("Either 'Email' or 'FirstName'  or 'LastName' is missing");

            List<string> customerPnrs = (await _futureFlightsrService.Query(new FlightRequest() { ContactEmail = request.CustomerDetails.ContactDetails.EmailDetails.FirstOrDefault(x => x.PreferredFlag == "Y").EmailId }))?.FlightSegmentDetails?.Select(x => x.BookingReference)?.Distinct().ToList();

            if (customerPnrs is null || !customerPnrs.Any()) return [];

            var tasks = customerPnrs.AsParallel()
                                    .Select(async pnr =>
                                    {
                                        RetrievePNRResponse pnrServiceResonse = await _pnrQueryProvider.RetrievePNR(pnr);

                                        return pnrServiceResonse?.PnrType.Passengers.Select(psg =>
                                        {
                                            if (psg.LastName.Replace(" ", "").Is(request.CustomerDetails.LastName.Replace(" ", "")) && (psg.FirstName.Replace(" ", "").Is(request.CustomerDetails.FirstName.Replace(" ", "") + request.CustomerDetails.MiddleName?.Replace(" ", ""))
                                                    || psg.FirstName.Replace(" ", "").Is(request.CustomerDetails.FirstName.Replace(" ", "")) || psg.FirstName.Substring(0, 3).Is(request.CustomerDetails.FirstName.Substring(0, 3))))
                                            {
                                                if (pnrServiceResonse.PnrType.FrequentFlyer == null || !pnrServiceResonse.PnrType.FrequentFlyer.Any(x => x.FirstNameNumber == psg.FirstNameNumber && x.LastNameNumber == psg.LastNameNumber && x.AccountNumber != null))
                                                    return new FCNumToPNRDetails()
                                                    {
                                                        FirstName = psg.FirstName,
                                                        LastName = psg.LastName,
                                                        MemberNumber = request.MemberNumber,
                                                        PNR = pnr
                                                    };
                                            }

                                            return null;
                                        });
                                    });

            await Task.WhenAll(tasks);

            return tasks.Where(x => x.Result is not null && x.Result.Any(s => s is not null)).SelectMany(x => x.Result.Where(s => s is not null).AsEnumerable());
        }
    }
}
