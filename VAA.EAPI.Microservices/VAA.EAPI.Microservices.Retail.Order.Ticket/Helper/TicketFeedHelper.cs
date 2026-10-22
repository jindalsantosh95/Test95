using CSharpFunctionalExtensions;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.VoidTicket;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Infrastructure.Helper;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Helper
{
    public class TicketFeedHelper
    {
        public static Result<VoidTicketDetails> GetPnrTicketDetails(string voidTicketFeed)
        {
            var rLocrTable = Air4TicketFeedToDataTable<TicketBookingDocumentRecLocr>.Convert(voidTicketFeed, Air4TicketFeedTable.ET_TKDC_BKG_RLOCR.ToString(), Constants.Application.TicketFeedFileDelimiter);
            var couponTable = Air4TicketFeedToDataTable<Coupon>.Convert(voidTicketFeed, Air4TicketFeedTable.ET_CPN.ToString(), Constants.Application.TicketFeedFileDelimiter);

            var rLocrForVS = rLocrTable
                            .AsEnumerable()
                            .Where(x => x.Field<string>("ResSysPvdrCd") == Constants.Application.VAASystemProviderCode)
                            .Select(x => new
                            {
                                TktDocNumber = x.Field<string>("TktDocNumber"),
                                TktDocIssuseDate = x.Field<string>("TktDocIssuseDate"),
                                BkgRecLocrTxt = x.Field<string>("BkgRecLocrTxt")
                            }).FirstOrDefault();

            var coupon = couponTable
                            .AsEnumerable()
                            .Where(x => x.Field<string>("TktCpnETStatusCd") == TicketStatus.V.ToString()
                                    && x.Field<string>("TPFTrEvtTypeCd") == TicketStatus.EVUT.ToString())
                            .Select(x => new CouponDetail
                            {
                                TicketNo = x.Field<string>("TktDocNumber")?.Remove(0, Constants.Application.AirlineCodeLength),
                                LastTransactionEventGTs = x.Field<string>("LastTrnsEvtGTs"),
                                SegmentBookingDate = x.Field<string>("SegmentBkgGDt")
                            });

            if (!string.IsNullOrEmpty(rLocrForVS?.BkgRecLocrTxt) && (coupon?.Any() ?? false))
            {
                return Result.Success(new VoidTicketDetails
                {
                    PNR = rLocrForVS.BkgRecLocrTxt,
                    IssueDate = rLocrForVS.TktDocIssuseDate,
                    Tickets = new List<CouponDetail>(coupon)
                });

            }
            else
                return Result.Failure<VoidTicketDetails>("PNR Details Not Found");
        }
    }
}
