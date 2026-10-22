using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Application.Queries.RetrieveTicket;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Domain;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.Flights;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.TicketFeedData;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Infrastructure.Repository.ChangeFeed;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Application.Queries.Flights
{
    public class FlightsQueryHandler : IRequestHandler<FlightsQuery, Result<Response<FlightResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FlightsQueryHandler> _logger;

        public FlightsQueryHandler(IMediator mediator, ILogger<FlightsQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<FlightResponse>>> Handle(FlightsQuery request, CancellationToken cancellationToken)
        {
            if (request.FlightRequest.ContactEmail is null && request.FlightRequest.FFPNumber is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            IEnumerable<Domain.Flights.Ticket> ffpNumberTickets = [];
            IEnumerable<Domain.Flights.Ticket> emailTickets = [];
            List<Domain.Flights.Ticket> tickets = [];

            //FFPNumber Search
            if (!string.IsNullOrEmpty(request.FlightRequest.FFPNumber))
            {                
                var ffpSearch = await _mediator.Send(new TicketFeedRepoQuery(value: request.FlightRequest.FFPNumber, docType: DocumentType.TicketFFPAndEmail.ToString(), searchFilter: "FFPNumber"));

                var ffpSearchItems = ffpSearch?.Where(e => ((JObject)e)["FFPCarrierCode"].ToString().Is("VS")).Select(e => ((JToken)e).ToObject<TicketFFPAndEmailDto>());

                ffpNumberTickets = await FlightDetails(ffpSearchItems, "FFPNumber");
            }

            //Email Search
            if (!string.IsNullOrEmpty(request.FlightRequest.ContactEmail))
            {
                var emailSearch = await _mediator.Send(new TicketFeedRepoQuery(value: request.FlightRequest.ContactEmail, docType: DocumentType.TicketFFPAndEmail.ToString(), searchFilter: "ContactEmail"));
                
                var emailSearchItems = emailSearch?.Select(e=> Json.DeserializeObject<TicketFFPAndEmailDto>((JToken)e));

                emailTickets = await FlightDetails(emailSearchItems, "ContactEmail");                
            }
            
            var commonTickets = ffpNumberTickets?.Where(x => emailTickets.Any(y => x.TicketNumber == y.TicketNumber));
            
            tickets.AddRange(ffpNumberTickets);
            tickets.AddRange(emailTickets);
            
            foreach (var item in commonTickets)
            {
                var commonTicket = tickets.FirstOrDefault(f=> f.TicketNumber == item.TicketNumber);
                commonTicket.SearchCriteria = "FFPNumberAndEmail";
            }

            return new Response<FlightResponse>
            {
                Status = Status.OK,
                Value = new FlightResponse { ContactEmail = request.FlightRequest.ContactEmail, FFPNumber = request.FlightRequest.FFPNumber, Tickets = tickets.DistinctBy(t => t.TicketNumber) }
            };            
        }

        private async Task<IEnumerable<Domain.Flights.Ticket>> FlightDetails(IEnumerable<TicketFFPAndEmailDto> ticketFFPAndEmailDtos, string searchIdentifier)
        {
            List<Domain.Flights.Ticket> tickets = [];

            foreach (var item in ticketFFPAndEmailDtos)
            {
                //Retrieve Tickets
                var ticket = await _mediator.Send(new RetriveTicketQuery(new RetriveTicketRequest { ETicketNumber = item.TicketNumber, IncludeAssociatedEmds = false }));

                var flights = ticket.Value?.Value?.Coupons?.Where(c => DateTime.ParseExact(c.EtCpnCrrRole?.First().SchdDprtLDt, Constants.Application.TicketDocDateFormat, CultureInfo.InvariantCulture) >= DateTime.UtcNow)?.Select(r => new Flight
                {
                    Origin = r.EtCpnCrrRole?.First().ROrigTrpnLocTxt,
                    Destination = r.EtCpnCrrRole?.First().RDestTrpnLocTxt,
                    DepartureDate = !string.IsNullOrEmpty(r.EtCpnCrrRole?.First().SchdDprtLDt) ? DateOnly.ParseExact(r.EtCpnCrrRole?.First().SchdDprtLDt, Constants.Application.TicketDocDateFormat, CultureInfo.InvariantCulture) : null,
                    DepartureTime = r.EtCpnCrrRole?.First().SchdDprtLTmTxt,
                    ArrivalDate = !string.IsNullOrEmpty(r.EtCpnCrrRole?.First().SchdArrLDt) ? DateOnly.ParseExact(r.EtCpnCrrRole?.First().SchdArrLDt, Constants.Application.TicketDocDateFormat, CultureInfo.InvariantCulture) : null,
                    ArrivalTime = r.EtCpnCrrRole?.First().SchdArrLTmTxt,
                    OperatingBookingClass = string.IsNullOrEmpty(r.EtCpnCrrRole?.First().BkgCOSCd) ? r.EtCpnCrrRole?.First().EqOwrTktgCOSCd : r.EtCpnCrrRole?.First().BkgCOSCd,
                    MarketingBookingClass = r.EtCpnCrrRole?.First().MkdTktgCOSCd,
                    OperatingCarrierCode = r.EtCpnCrrRole?.First().EqpOwnrCrrCd ?? r.EtCpnCrrRole?.First().OpAsCrrCd,
                    MarketingCarrierCode = r.EtCpnCrrRole?.First().MkdCrrCd,
                    OperatingFlightNumber = r.EtCpnCrrRole?.First().EqpOwnrFltNb ?? r.EtCpnCrrRole?.First().OpAsFltNb,
                    MarketingFlightNumber = r.EtCpnCrrRole?.First().MkdFltNb
                });

                tickets.Add(new Domain.Flights.Ticket()
                {
                    TicketNumber = item.TicketNumber,
                    TicketIssueDate = DateTime.ParseExact(ticket.Value.Value.Document.TktDocIssLDt, Constants.Application.TicketDocDateFormat, CultureInfo.InvariantCulture),
                    SearchCriteria = searchIdentifier,
                    Flights = flights
                });
            }

            return tickets;
        }
    }
}
