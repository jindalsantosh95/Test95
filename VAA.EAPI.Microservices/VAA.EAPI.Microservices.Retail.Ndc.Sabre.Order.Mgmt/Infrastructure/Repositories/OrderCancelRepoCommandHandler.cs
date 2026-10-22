using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Repositories;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Mapper;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Repositories
{
    public class OrderCancelRepoCommandHandler(IMediator mediator, ILogger<OrderCancelRepoCommandHandler> logger, SabreCommandLLSCommandHandler sabreCommandLLSCommandHandler) : IRequestHandler<OrderCancelRepoCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        private readonly ILogger<OrderCancelRepoCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly SabreCommandLLSCommandHandler _sabreCommandLLSCommandHandler = sabreCommandLLSCommandHandler ?? throw new ArgumentNullException(nameof(sabreCommandLLSCommandHandler));

        public async Task<Result<Response<string>>> Handle(OrderCancelRepoCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var token = await _mediator.Send(new CreateSessionCommand() { PCC = request.orderChangeRequestData.Parties.Participants.First().PccCode}, cancellationToken);

                if (token != null)
                {
                    //Call GetReservation
                    var getReservationReq = GetReservationRequestMapper.GetReservationRequest(token, request.orderChangeRequestData.Rloc);
                    var getReservationRes = await _mediator.Send(new GetReservationQuery(getReservationReq), cancellationToken);

                    if (getReservationRes.Errors == null)
                    {
                        if ((getReservationRes.Item as ReservationPNRB).PassengerReservation.TicketingInfo.ETicketNumber != null && (request.orderChangeRequestData.Operation == OrderChangeRequestData.Operations.OrderCreate || request.orderChangeRequestData.Operation == OrderChangeRequestData.Operations.OrderChange || request.orderChangeRequestData.IsRFC) && !request.orderChangeRequestData.IsCancelDisruption)
                        {
                            //Call VoidAirTicket
                            foreach (var ticket in (getReservationRes.Item as ReservationPNRB).PassengerReservation.TicketingInfo.ETicketNumber.Where(x => x.index != null))
                            {
                                var voidAirTicketReq = VoidAirTicketRequestMapper.GetVoidAirTicketRequest(token, ticket.index);
                                var voidAirTicketRes = await _mediator.Send(new VoidAirTicketCommand() { VoidTicketRequest = voidAirTicketReq }, cancellationToken);

                                if (voidAirTicketRes.ApplicationResults?.Error == null && voidAirTicketRes.ApplicationResults?.status.ToString() == "Complete")
                                {
                                    await _mediator.Send(new VoidAirTicketCommand() { VoidTicketRequest = voidAirTicketReq }, cancellationToken);
                                }
                                else
                                {
                                    _logger.LogError("Sabre VoidAirTicket returned an error response");

                                }
                            }
                        }

                        if ((!request.orderChangeRequestData.IsRFC || request.orderChangeRequestData.IsCancelDisruption) && request.orderChangeRequestData.Operation == OrderChangeRequestData.Operations.OrderCancel)
                        {
                            // To accept disrupted segments
                            if(request.orderChangeRequestData.IsCancelDisruption)
                            {
                                var enhancedEndTransactionRS = await _mediator.Send(new EnhancedEndTransactionCommand() { EnhancedEndTransactionRQ = EnhancedEndTransactionRequestMapper.GetEnhancedEndTransactionRequest(token, request.orderChangeRequestData.IsCancelDisruption) }, cancellationToken);

                                if (enhancedEndTransactionRS.ApplicationResults.status != EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedEndTransaction.CompletionCodes.Complete)
                                {
                                    throw new Exception("Error Occured in OrderCancelRepoCommandHandler while calling EnhancedEndTransaction");
                                }

                                var getReservationRS = await _mediator.Send(new GetReservationQuery(getReservationReq), cancellationToken);
                            }

                            await _mediator.Send(new DesignatePrinterCommand() { DesignatePrinterRequest = DesignatePrinterRequestMapper.Map(token, request.orderChangeRequestData.Parties.Participants.First().PccCode) }, cancellationToken);

                            bool isPartialFlown = request.orderChangeRequestData.AccountableDocumentRefs?.Any(x => x.Coupons?.Any(c => c.CouponStatus == "USED") == true) == true;

                            if (request.orderChangeRequestData.IsCancelHoldBooking != true && isPartialFlown)
                            {
                                for (int i = 1; i <= ((ReservationPNRB)getReservationRes.Item).PassengerReservation.Passengers.Passenger.Count(); i++)
                                {
                                    var pax = request.orderChangeRequestData.DataList.CustomerDetails.ElementAt(i - 1);

                                    var fareCaluculationText = request.orderChangeRequestData.SelectedOffers.First().OfferItems.Where(x => x.Services.Any(srv => srv.ServiceBeneficiaries.Contains(pax.CustId))).Select(x => x.OfferitemPricing.First().FareCalculationText).First();
                                    List<(string Amount, string FareBasis)> fareBasisandAmount = new List<(string, string)>();

                                    if (fareCaluculationText != null)
                                    {
                                        string[] fareSplits = fareCaluculationText.Split("VS");

                                        foreach (var split in fareSplits)
                                        {
                                            // Regex: capture number (with decimals) followed by letters/numbers
                                            var match = Regex.Match(split, @"(\d{2,}(?:\.\d+)?)([A-Z]{2,}[A-Z0-9]*)");
                                            if (match.Success)
                                            {
                                                string amount = match.Groups[1].Value;
                                                string code = match.Groups[2].Value;

                                                if (!Regex.IsMatch(code, @"^[A-Z][A-Z0-9]{3,}$"))
                                                    continue;

                                                fareBasisandAmount.Add((amount, code));
                                            }
                                        }
                                    }

                                    var passengerTicketDetails = ((ReservationPNRB)getReservationRes.Item).PassengerReservation.Passengers.Passenger.ElementAt(i - 1)?.TicketingInfo?.TicketDetails.Distinct();
                                    var ticketNumber = ((ReservationPNRB)getReservationRes.Item).PassengerReservation.TicketingInfo.TicketDetails.Where(x => passengerTicketDetails.All(y => y.TicketNumber == x.TicketNumber));

                                    var hostCommands = ticketNumber.SelectMany(x =>
                                    {
                                        return new string[]
                                        {
                                             $"WFR{x.TicketNumber}¥AVS¥N{i}.1",
                                             string.Empty,
                                             string.Empty,
                                             string.Empty,
                                             string.Empty,
                                             string.Empty,
                                             string.Empty
                                        };
                                    }).ToArray();

                                    int index = 0;
                                    foreach (var hostCommand in hostCommands)
                                    {
                                        var sabreCommandLLSRQ = SabreCommandLLSRequestMapper.GetSabreCommandRequest(token, hostCommand);
                                        var sabreCommandLLSRS = await _sabreCommandLLSCommandHandler.Handle(new SabreCommandLLSCommand() { SabreCommandLLSRequest = sabreCommandLLSRQ }, cancellationToken);
                                        if (sabreCommandLLSRS != null && sabreCommandLLSRS.ApplicationResults?.Error == null)
                                        {
                                            if (hostCommand.StartsWith("WFR"))
                                            {
                                                var sabreCommandResponse1 = sabreCommandLLSRS.Response.Split([',', '.', ';', ':']).ToArray();
                                                var matchCount = sabreCommandResponse1.Sum(x => new Regex(Regex.Escape("< >")).Matches(x).Count);
                                                var command1 = new StringBuilder();
                                                for (int j = 0; j < matchCount - 3; j++)
                                                {
                                                    command1.Append("<R>");
                                                }
                                                hostCommands[1] = $"<![CDATA[ WF{command1}<X><><>]]>";
                                            }
                                            else if (index == 1)
                                            {
                                                var sabreCommandResponse = Regex.Matches(sabreCommandLLSRS.Response, @"<([^>]*)>").Select(m => m.Groups[1].Value.Trim());
                                                var command1 = new StringBuilder();
                                                int sabreCommandResponseCount = sabreCommandResponse.Count();

                                                for (int j = 0; j < sabreCommandResponseCount; j++)
                                                {
                                                    string commandValue = sabreCommandResponse.ElementAtOrDefault(j);
                                                    if (string.IsNullOrEmpty(commandValue))
                                                    {
                                                        if (j == sabreCommandResponseCount - 2)
                                                            commandValue = "M";
                                                        else if (j == 9)
                                                            commandValue = "0";
                                                        else if (j == 7)
                                                            commandValue = fareBasisandAmount[0].FareBasis;
                                                        else if (j == 8)
                                                            commandValue = fareBasisandAmount[0].Amount;
                                                    }

                                                    command1.Append($"<{commandValue}>");
                                                }
                                                hostCommands[2] = $"<![CDATA[ WF{command1}]]>";
                                            }
                                            else if (index == 2)
                                            {
                                                var sabreCommandResponse = Regex.Matches(sabreCommandLLSRS.Response, @"<([^>]*)>").Select(m => m.Groups[1].Value.Trim());
                                                var command1 = new StringBuilder();
                                                for (int j = 0; j < sabreCommandResponse.Count(); j++)
                                                {
                                                    if (j == sabreCommandResponse.Count() - 3)
                                                        command1.Append("<X>");
                                                    else
                                                        command1.Append("< >");
                                                }
                                                hostCommands[3] = $"<![CDATA[ WF{command1}]]>";
                                            }
                                            else if (index == 3)
                                            {
                                                var sabreCommandResponse = Regex.Matches(sabreCommandLLSRS.Response, @"<([^>]*)>").Select(m => m.Groups[1].Value.Trim());
                                                var command1 = new StringBuilder();
                                                for (int j = 0; j < sabreCommandResponse.Count(); j++)
                                                {
                                                    if (j < 1)
                                                        command1.Append($"<{sabreCommandResponse.ElementAtOrDefault(j)}>");
                                                    else if (j == sabreCommandResponse.Count() - 3)
                                                        command1.Append("<X>");
                                                    else
                                                        command1.Append("< >");
                                                }
                                                hostCommands[4] = $"<![CDATA[ WF{command1}]]>";
                                            }
                                            else if (index == 4)
                                            {
                                                var sabreCommandResponse1 = sabreCommandLLSRS.Response.Split([',', '.', ';', ':']).ToArray();
                                                var matchCount = sabreCommandResponse1.Sum(x => new Regex(@"\<\s*\>").Matches(x).Count);
                                                var command1 = new StringBuilder();
                                                for (int j = 0; j < matchCount; j++)
                                                {
                                                    if (j == matchCount - 3)
                                                        command1.Append("<X>");
                                                    else
                                                        command1.Append("< >");
                                                }
                                                hostCommands[5] = $"<![CDATA[ WF{command1}]]>";
                                            }
                                            else if (index == 5)
                                            {
                                                var sabreCommandResponse3 = sabreCommandLLSRS.Response.Split([',', '.', ';', ':']).ToArray();
                                                var matchCount = sabreCommandResponse3.Sum(x => new Regex(@"\<\s*\>").Matches(x).Count);
                                                var command1 = new StringBuilder();
                                                for (int j = 0; j < matchCount; j++)
                                                {
                                                    if (j == matchCount - 2)
                                                        command1.Append("<X>");
                                                    else
                                                        command1.Append("<>");
                                                }
                                                hostCommands[6] = $"<![CDATA[ WF{command1}]]>";
                                            }
                                            index++;
                                        }
                                        if (sabreCommandLLSRS.ApplicationResults?.Error != null)
                                        {
                                            _logger.LogError("SabreCommandLLS returned an error response");
                                        }
                                    }

                                }
                            }
                            else if (request.orderChangeRequestData.IsCancelHoldBooking != true)
                            {
                                for (int i = 1; i <= ((ReservationPNRB)getReservationRes.Item).PassengerReservation.Passengers.Passenger.Count(); i++)
                                {
                                    var pax = request.orderChangeRequestData.DataList.CustomerDetails.ElementAt(i - 1);
                                var passengerTicketDetails = ((ReservationPNRB)getReservationRes.Item).PassengerReservation.Passengers.Passenger.ElementAt(i -1)?.TicketingInfo?.TicketDetails.Distinct();
                                    var ticketNumber = ((ReservationPNRB)getReservationRes.Item).PassengerReservation.TicketingInfo.TicketDetails.Where(x => passengerTicketDetails.All(y => y.TicketNumber == x.TicketNumber));
                                    var cancelFeeAmount = (from pen in (request.orderChangeRequestData.DataList.Penalty ?? Enumerable.Empty<Penalty>()).Where(x => x.Type == "Cancellation")
                                                           from offer in request.orderChangeRequestData.SelectedOffers
                                                           from offerItem in offer.OfferItems
                                                           from service in offerItem.Services
                                                           where service.PenaltyRef?.CancellationPenaltyRefIds != null && service.PenaltyRef.CancellationPenaltyRefIds.Any() && service.PenaltyRef.CancellationPenaltyRefIds.Contains(pen.Id) && service.ServiceBeneficiaries.Contains(pax.CustId)
                                                           select pen)?.FirstOrDefault()?.MaxPrice?.Amount is var amt && amt != null ? (!string.IsNullOrEmpty(amt.EquivalentMonetaryAmount) ? amt.EquivalentMonetaryAmount : amt.MonetaryAmount) : null;

                                    var hostCommands = ticketNumber.SelectMany(x =>
                                    {
                                        if (cancelFeeAmount != null)
                                        {
                                            return new string[]  {
                                        $"WFR{x.TicketNumber}¥AVS¥N{i}.1",
                                        string.Empty,
                                        string.Empty,
                                        string.Empty,
                                        string.Empty};
                                        }
                                        else
                                        {
                                            return new string[]  {
                                        $"WFR{x.TicketNumber}¥AVS¥N{i}.1",
                                        string.Empty,
                                        string.Empty,
                                        string.Empty };
                                        }
                                    }).ToArray();

                                    int index = 0;
                                    foreach (var hostCommand in hostCommands)
                                    {
                                        var sabreCommandLLSRQ = SabreCommandLLSRequestMapper.GetSabreCommandRequest(token, hostCommand);
                                        var sabreCommandLLSRS = await _mediator.Send(new SabreCommandLLSCommand() { SabreCommandLLSRequest = sabreCommandLLSRQ }, cancellationToken);
                                        if (sabreCommandLLSRS != null && sabreCommandLLSRS.ApplicationResults?.Error == null)
                                        {
                                            if (hostCommand.StartsWith("WFR"))
                                            {
                                                var sabreCommandResponse1 = sabreCommandLLSRS.Response.Split([',', '.', ';', ':']).ToArray();
                                                var matchCount = sabreCommandResponse1.Sum(x => new Regex(Regex.Escape("< >")).Matches(x).Count);
                                                var command1 = new StringBuilder();
                                                for (int j = 0; j < matchCount - 3; j++)
                                                {
                                                    command1.Append("<R>");
                                                }
                                                hostCommands[1] = $"<![CDATA[ WF{command1}<X><><>]]>";
                                            }
                                            else if (index == 1)
                                            {
                                                var sabreCommandResponse1 = sabreCommandLLSRS.Response.Split([',', '.', ';', ':']).ToArray();
                                                var matchCount = sabreCommandResponse1.Sum(x => new Regex(@"\<\s*\>").Matches(x).Count);
                                                var command1 = new StringBuilder();
                                                for (int j = 0; j < matchCount; j++)
                                                {
                                                    if (j == 0)
                                                        command1.Append($"<{cancelFeeAmount}>");
                                                    else if (j == matchCount - 2)
                                                        command1.Append("<X>");
                                                    else
                                                        command1.Append("<>");
                                                }
                                                hostCommands[2] = $"<![CDATA[ WF{command1} ]]>";
                                                if (cancelFeeAmount != null) hostCommands[3] = $"<![CDATA[ WF{command1} ]]>";
                                            }
                                        else if(cancelFeeAmount == null && index == 2 || cancelFeeAmount != null && index == 3)
                                            {
                                                var sabreCommandResponse3 = sabreCommandLLSRS.Response.Split([',', '.', ';', ':']).ToArray();
                                                var matchCount = sabreCommandResponse3.Sum(x => new Regex(@"\<\s*\>").Matches(x).Count);
                                                var command1 = new StringBuilder();
                                                for (int j = 0; j < matchCount; j++)
                                                {
                                                    if (j == matchCount - 2)
                                                        command1.Append("<X>");
                                                    else
                                                        command1.Append("<>");
                                                }
                                                hostCommands[3] = $"<![CDATA[ WF{command1} ]]>";
                                                if (cancelFeeAmount != null) hostCommands[4] = $"<![CDATA[ WF{command1} ]]>";
                                            }

                                            index++;
                                        }
                                        if (sabreCommandLLSRS.ApplicationResults?.Error != null)
                                        {
                                            _logger.LogError("SabreCommandLLS returned an error response");
                                        }
                                    }
                                }
                            }
                        }

                        if (!request.orderChangeRequestData.CancelPnr)
                        {
                            //Call enhanced end transaction
                            await CreateEnhancedEndTransaction(token, request.orderChangeRequestData, cancellationToken);
                        }
                        else
                        {
                            if(!request.orderChangeRequestData.IsCancelDisruption || !request.orderChangeRequestData.DataList.SegmentDetails.All(x => x.Status.Code == "UN"))
                            {
                                //Call OTACancel
                                var otaCancelReq = OTACancelLLSRequestMapper.GetOTACancelLLSRQForAllSeg(token);
                                var otaCancelRes = await _mediator.Send(new OTACancelLLSCommand() { OtaCancelRequest = otaCancelReq }, cancellationToken);

                                if (otaCancelRes.OTA_CancelRS.ApplicationResults.Error != null)
                                {
                                    throw new Exception("Error Occured in OrderCancelRepoCommandHandler while calling OTACancel");
                                }
                            }

                            //Call enhanced end transaction
                            await CreateEnhancedEndTransaction(token, request.orderChangeRequestData, cancellationToken);
                        }

                        return Result.Success(new Response<string>
                        {
                            Status = Status.OK                            
                        });
                    }
                    else
                    {
                        _logger.LogError("Sabre GetReservation returned an error response");

                        throw new Exception($"Error Occured in OrderCancelRepoCommandHandler while calling GetReservation");
                    }
                }
                else
                {
                    throw new Exception($"Error Occured in OrderCancelRepoCommandHandler while calling TokenService");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Order cancellation Sabre workflow failed");

                var errorResponse = ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath);

                return Result.Failure<Response<string>>(Json.Serialize(errorResponse));
            }
        }

        private async Task CreateEnhancedEndTransaction(string token, OrderChangeRequestData requestData, CancellationToken cancellationToken)
        {
            var enhancedEndTransactionRS = await _mediator.Send(new EnhancedEndTransactionCommand() { EnhancedEndTransactionRQ = EnhancedEndTransactionRequestMapper.GetEnhancedEndTransactionRequest(token) }, cancellationToken);

            // Call Update Passenger Name Record for adding OSI  element in disrupted pnrs
            if (requestData.IsCancelDisruption)
            {
                var updatePassengerNameRecordRQRequest = ManageCustomerRequestMapper.CustomerPnrReqMapperForDisruption(requestData.Rloc, token);
                var updatePassengerNameRecordRQResponse = await _mediator.Send(new UpdatePassengerNameRecordCommand() { UpdatePassengerNameRecordRQRequest = updatePassengerNameRecordRQRequest }, cancellationToken);

                if (updatePassengerNameRecordRQResponse.UpdatePassengerNameRecordRS.ApplicationResults?.Error != null)
                {
                    throw new Exception("Error Occured in OrderCancelRepoCommandHandler while calling UpdatePassengerNameRecord");
                }
            }

            await _mediator.Send(new SessionCloseCommand() { BinaryToken = token, PCC = requestData.Parties.Participants.First().PccCode }, cancellationToken);
            
            if (enhancedEndTransactionRS.ApplicationResults.status != EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedEndTransaction.CompletionCodes.Complete)
            {
                throw new Exception($"Error Occured in OrderCancelRepoCommandHandler while calling EnhancedEndTransaction");
            }
        }
    }
}
