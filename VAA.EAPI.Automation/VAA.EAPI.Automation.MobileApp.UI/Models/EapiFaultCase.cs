using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Automation.MobileApp.UI.Models;

public sealed record EapiFaultCase(string Code, string Message, bool Basket = false, bool Timeout = false,
    PostHppOperation Completion = PostHppOperation.None, SeatFaultOperation Seat = SeatFaultOperation.None)
{
    public bool PostHpp => Completion != PostHppOperation.None;
    public string Operation => Completion switch
    {
        PostHppOperation.UpdatePaymentStatus => "SearchAndBookUpdatePaymentStatus",
        PostHppOperation.CreateOrder => "SearchAndBookCreateOrder",
        _ => Seat switch
        {
            SeatFaultOperation.RetrieveMap => "SearchAndBookRetrieveSeatMap",
            SeatFaultOperation.SelectSeat => "SearchAndBookSelectSeat",
            _ => Basket ? "SearchAndBookRetrieveBasket" : "SearchAndBookOffers"
        }
    };
    public string RootField => Completion switch
    {
        PostHppOperation.UpdatePaymentStatus => "bookUpdatePaymentStatus",
        PostHppOperation.CreateOrder => "bookCreateOrder",
        _ => Seat switch
        {
            SeatFaultOperation.RetrieveMap => "retrieveSeatMap",
            SeatFaultOperation.SelectSeat => "selectSeat",
            _ => Basket ? "retrieveBasket" : "searchOffers"
        }
    };
    public string ExpectedUiMessage => Seat == SeatFaultOperation.RetrieveMap
        ? "We could not load the seat map. Please try again."
        : Seat == SeatFaultOperation.SelectSeat
        ? "We could not save your seat selection. Please choose again."
        : PostHpp
        ? "Sorry, we’re unable to confirm the status of your payment or booking.\nFor more information you can call our Contact Centre on 0344 472 1422"
        : Basket
        ? "We could not retrieve your booking details. Please try again."
        : "We could not start this search just now. Please try again.";

    // Sources and intentional limitations are recorded in MOBILE_FAULT_INJECTION.md.
    public static EapiFaultCase PastDate => new("3001", "Doesn't time fly? The date you've search for has now passed. Please try a different date.");
    public static EapiFaultCase SearchUnavailable => new("3002", "We're sorry, there was a problem processing your request. Please go back and try the entry again. If you continue to experience problems, please <a href='https://help.virginatlantic.com/gb/en.html#contactus' target='_blank'>contact us</a>. #3002");
    public static EapiFaultCase NoAvailability => new("101682", "We couldn’t find any results for your search. But we’ll try not to let that stop us, why not try a different airport or date. Some destinations we only fly to seasonally, and not every day. #101682");
    public static EapiFaultCase SupplierSessionMissing => new("100600", "Sorry, nothing matches those options. Please change your selections and try again. #100600");
    public static EapiFaultCase BasketMissing => new("2004", "We are facing technical difficulties. Please re-start your journey", Basket: true);
    public static EapiFaultCase RequestTimeout => new("transport-timeout", "No EAPI response delivered within the client timeout.", Timeout: true);
    public static EapiFaultCase PaymentStatusFailure => new("500", "Unhandled Exception in create PRI from payment mutation.", Completion: PostHppOperation.UpdatePaymentStatus);
    public static EapiFaultCase BookingCompletionFailure => new("2004", "We are facing technical difficulties. Please re-start your journey", Completion: PostHppOperation.CreateOrder);
    public static EapiFaultCase BookingCompletionTimeout => new("transport-timeout", "No booking response delivered within the client timeout.", Timeout: true, Completion: PostHppOperation.CreateOrder);
    public static EapiFaultCase SeatMapFailure => new("500", "Unhandled Exception in retrieve seat map query.", Seat: SeatFaultOperation.RetrieveMap);
    public static EapiFaultCase SeatSaveFailure => new("500", "Unhandled Exception in select seat mutation.", Seat: SeatFaultOperation.SelectSeat);

    public JObject BuildRule()
    {
        string details = new JArray(new JObject
        {
            ["code"] = Code,
            ["developerMessage"] = "Synthetic automation fixture; no live identifiers.",
            ["message"] = Message
        }).ToString(Formatting.None);
        JObject extensions = Basket || Completion == PostHppOperation.CreateOrder
            ? new JObject { ["code"] = "500", ["reason"] = "Internal Server Error", ["response"] = details }
            : new JObject { ["code"] = "422", ["Reason"] = details };
        if (Completion == PostHppOperation.UpdatePaymentStatus || Seat != SeatFaultOperation.None)
            extensions = new JObject { ["code"] = "500", ["Exception"] = "Synthetic automation failure; no live identifiers." };
        return new JObject
        {
            ["endpoint"] = "https://digital-test.virginatlantic.com/sit/graphql/mobile/public/v1",
            ["operationName"] = Operation,
            ["rootField"] = RootField,
            ["mode"] = Timeout ? "timeout" : "response",
            ["response"] = new JObject
            {
                ["data"] = new JObject { [RootField] = null },
                ["errors"] = new JArray(new JObject
                {
                    ["message"] = Completion == PostHppOperation.UpdatePaymentStatus || Seat != SeatFaultOperation.None ? Message
                        : Basket || PostHpp ? "Request failed." : "Search Error",
                    ["extensions"] = extensions
                })
            }
        };
    }
}
