/********************************************************************/
/*                                                                  */
/*  Classes auto-generated from Edit->Past Special->From JSon       */
/*  using JSon from                                                 */
/*                                                                  */
/*  https://devportal.atpco.net/docs/new-routehappy/consolidated/   */
/*                                                                  */
/********************************************************************/

using Newtonsoft.Json;
using System;

namespace VAA.EAPI.Microservices.Retail.Brand.Infrastructure.ATPCO
{

    public class ATPCOResponse
    {
        public Data data { get; set; }
        public Itinerary[] itineraries { get; set; }
        public string uuid { get; set; }
    }

    public class Data
    {
        public Advance_Change[] advance_change { get; set; }
        public Amenity amenity { get; set; }
        public Amenity_Summary[] amenity_summary { get; set; }
        public Boarding_Priority[] boarding_priority { get; set; }
        public Brand[] brand { get; set; }
        public Cancellation[] cancellation { get; set; }
        public Carry_On_Bag[] carry_on_bag { get; set; }
        public Check_In_Priority[] check_in_priority { get; set; }
        public Checked_Bag[] checked_bag { get; set; }
        public Lounge_Access[] lounge_access { get; set; }
        public object[] same_day_change { get; set; }
        public Seat_Selection[] seat_selection { get; set; }
        public Upa[] upa { get; set; }
        public Upgrade_Eligibility[] upgrade_eligibility { get; set; }
    }

    public class Amenity
    {
        public Aircraft[] aircrafts { get; set; }
        public Beverage[] beverages { get; set; }
        public Entertainment[] entertainments { get; set; }
        public Fresh_Foods[] fresh_foods { get; set; }
        public Layout[] layouts { get; set; }
        public Power[] powers { get; set; }
        public Seat[] seats { get; set; }
        public Wifi[] wifis { get; set; }
    }

    public class Aircraft
    {
        public string cabin_pressure { get; set; }
        public string display_text { get; set; }
        public string id { get; set; }
        public string model { get; set; }
        public string quality { get; set; }
        public string type { get; set; }
        public DateTime updated_at { get; set; }
        public string window_size { get; set; }
    }

    public class Beverage
    {
        public string alcoholic_cost { get; set; }
        public string display_text { get; set; }
        public string exists { get; set; }
        public string id { get; set; }
        public string nonalcoholic_cost { get; set; }
        public string quality { get; set; }
        public string type { get; set; }
        public DateTime updated_at { get; set; }
    }

    public class Entertainment
    {
        public string cost { get; set; }
        public string display_text { get; set; }
        public string exists { get; set; }
        public string id { get; set; }
        public Offerings offerings { get; set; }
        public string quality { get; set; }
        public string type { get; set; }
        public DateTime updated_at { get; set; }
    }

    public class Offerings
    {
        public Offering[] offerings { get; set; }
        [JsonProperty("operator")]
        public string _operator { get; set; }
    }

    public class Offering
    {
        public string content_type { get; set; }
        public string delivery_medium { get; set; }
        public string selection_type { get; set; }
    }

    public class Fresh_Foods
    {
        public string cost { get; set; }
        public string display_text { get; set; }
        public string exists { get; set; }
        public string id { get; set; }
        public string quality { get; set; }
        public string type { get; set; }
        public DateTime updated_at { get; set; }
    }

    public class Layout
    {
        public string direct_aisle_access { get; set; }
        public string display_text { get; set; }
        public string id { get; set; }
        public string quality { get; set; }
        public string row_layout { get; set; }
        public string type { get; set; }
        public DateTime updated_at { get; set; }
    }

    public class Power
    {
        public string chance { get; set; }
        public string cost { get; set; }
        public string display_text { get; set; }
        public string distribution { get; set; }
        public string exists { get; set; }
        public string id { get; set; }
        public string multiple_at_seat { get; set; }
        public string power_outlet { get; set; }
        public string quality { get; set; }
        public string type { get; set; }
        public DateTime updated_at { get; set; }
        public string usb_port { get; set; }
    }

    public class Seat
    {
        public string display_text { get; set; }
        public string flatness { get; set; }
        public string id { get; set; }
        public string legroom { get; set; }
        public string pitch { get; set; }
        public string quality { get; set; }
        public string type { get; set; }
        public DateTime updated_at { get; set; }
        public string width { get; set; }
    }

    public class Wifi
    {
        public string chance { get; set; }
        public string connectivity_type { get; set; }
        public string cost { get; set; }
        public string coverage { get; set; }
        public string display_text { get; set; }
        public string exists { get; set; }
        public string id { get; set; }
        public string performance { get; set; }
        public string quality { get; set; }
        public string type { get; set; }
        public DateTime updated_at { get; set; }
    }

    public class Advance_Change
    {
        public Anytime anytime { get; set; }
        public string id { get; set; }
    }

    public class Anytime
    {
        public string assessment { get; set; }
        public string description { get; set; }
        public Fee fee { get; set; }
        public string headline { get; set; }
    }

    public class Fee
    {
        public decimal amount { get; set; }
        public string currency { get; set; }
        public string higher_lower { get; set; }
        public decimal percentage { get; set; }
    }

    public class Amenity_Summary
    {
        public Aircraft1 aircraft { get; set; }
        public Beverage1 beverage { get; set; }
        public Entertainment1 entertainment { get; set; }
        public Fresh_Food fresh_food { get; set; }
        public string id { get; set; }
        public Layout1 layout { get; set; }
        public Power1 power { get; set; }
        public Seat1 seat { get; set; }
        public Wifi1 wifi { get; set; }
    }

    public class Aircraft1
    {
        public string display_text { get; set; }
    }

    public class Beverage1
    {
        public string display_text { get; set; }
        public string exists { get; set; }
    }

    public class Entertainment1
    {
        public string display_text { get; set; }
        public string exists { get; set; }
    }

    public class Fresh_Food
    {
        public string display_text { get; set; }
        public string exists { get; set; }
    }

    public class Layout1
    {
        public string display_text { get; set; }
    }

    public class Power1
    {
        public string display_text { get; set; }
        public string exists { get; set; }
    }

    public class Seat1
    {
        public string display_text { get; set; }
    }

    public class Wifi1
    {
        public string display_text { get; set; }
        public string exists { get; set; }
    }

    public class Boarding_Priority
    {
        public string application { get; set; }
        public string assessment { get; set; }
        public string code { get; set; }
        public string commercial_name { get; set; }
        public string description { get; set; }
        public string group_code { get; set; }
        public string headline { get; set; }
        public string id { get; set; }
        public string large_icon_url { get; set; }
        public string small_icon_url { get; set; }
        public string sub_group_code { get; set; }
    }

    public class Brand
    {
        public string[] brand_codes { get; set; }
        public string brand_name { get; set; }
        public string id { get; set; }
    }

    public class Cancellation
    {
        public Anytime1 anytime { get; set; }
        public string id { get; set; }
    }

    public class Anytime1
    {
        public string assessment { get; set; }
        public string description { get; set; }
        public Fee1 fee { get; set; }
        public string headline { get; set; }
    }

    public class Fee1
    {
        public decimal amount { get; set; }
        public string currency { get; set; }
        public string higher_lower { get; set; }
        public decimal percentage { get; set; }
    }

    public class Carry_On_Bag
    {
        public string assessment { get; set; }
        public string description { get; set; }
        public Free_Bags free_bags { get; set; }
        public string headline { get; set; }
        public string id { get; set; }
        public string large_icon_url { get; set; }
        public object[] paid_bags { get; set; }
        public string small_icon_url { get; set; }
    }

    public class Free_Bags
    {
        public Attributes attributes { get; set; }
        public string display_text { get; set; }
        public string optional_service_id { get; set; }
        public string pieces { get; set; }
        public Size size { get; set; }
        public Weight weight { get; set; }
    }

    public class Attributes
    {
        public string cxr { get; set; }
        public int seq_no { get; set; }
        public string service_type { get; set; }
        public string sub_code { get; set; }
    }

    public class Size
    {
        public string application { get; set; }
        public string cm { get; set; }
        [JsonProperty("in")]
        public string _in { get; set; }
    }

    public class Weight
    {
        public string application { get; set; }
        public string kg { get; set; }
        public string lb { get; set; }
    }

    public class Check_In_Priority
    {
        public string application { get; set; }
        public string assessment { get; set; }
        public string code { get; set; }
        public string commercial_name { get; set; }
        public string description { get; set; }
        public string group_code { get; set; }
        public string headline { get; set; }
        public string id { get; set; }
        public string large_icon_url { get; set; }
        public string small_icon_url { get; set; }
        public string sub_group_code { get; set; }
    }

    public class Checked_Bag
    {
        public string assessment { get; set; }
        public string description { get; set; }
        public Free_Bags1 free_bags { get; set; }
        public string headline { get; set; }
        public string id { get; set; }
        public string large_icon_url { get; set; }
        public Paid_Bags[] paid_bags { get; set; }
        public string small_icon_url { get; set; }
    }

    public class Free_Bags1
    {
        public Attributes1 attributes { get; set; }
        public string display_text { get; set; }
        public string optional_service_id { get; set; }
        public string pieces { get; set; }
    }

    public class Attributes1
    {
        public string cxr { get; set; }
        public int seq_no { get; set; }
        public string service_type { get; set; }
        public string sub_code { get; set; }
    }

    public class Paid_Bags
    {
        public Attributes2 attributes { get; set; }
        public string display_text { get; set; }
        public string excess_first { get; set; }
        public string excess_last { get; set; }
        public Fee2 fee { get; set; }
        public string free { get; set; }
        public string optional_service_id { get; set; }
        public string os_sequence { get; set; }
        public Size1 size { get; set; }
        public Weight1 weight { get; set; }
    }

    public class Attributes2
    {
        public string cxr { get; set; }
        public int seq_no { get; set; }
        public string service_type { get; set; }
        public string sub_code { get; set; }
    }

    public class Fee2
    {
        public float amount { get; set; }
        public string currency { get; set; }
    }

    public class Size1
    {
        public string application { get; set; }
        public string cm { get; set; }
        [JsonProperty("in")]
        public string _in { get; set; }
    }

    public class Weight1
    {
        public string application { get; set; }
        public string kg { get; set; }
        public string lb { get; set; }
    }

    public class Lounge_Access
    {
        public string application { get; set; }
        public string assessment { get; set; }
        public string code { get; set; }
        public string commercial_name { get; set; }
        public string description { get; set; }
        public string group_code { get; set; }
        public string headline { get; set; }
        public string id { get; set; }
        public string large_icon_url { get; set; }
        public string small_icon_url { get; set; }
        public string sub_group_code { get; set; }
    }

    public class Seat_Selection
    {
        public string application { get; set; }
        public string assessment { get; set; }
        public string code { get; set; }
        public string commercial_name { get; set; }
        public string description { get; set; }
        public string group_code { get; set; }
        public string headline { get; set; }
        public string id { get; set; }
        public string large_icon_url { get; set; }
        public string small_icon_url { get; set; }
        public string sub_group_code { get; set; }
    }

    public class Upa
    {
        public string att_descr { get; set; }
        public string[] categories { get; set; }
        public string cta_text { get; set; }
        public string cta_url { get; set; }
        public string description { get; set; }
        public object[] fees { get; set; }
        public string headline { get; set; }
        public string id { get; set; }
        public string large_icon_url { get; set; }
        public OSI[] optional_service_identifiers { get; set; }
        public Photo[] photos { get; set; }
        public string small_icon_url { get; set; }
        public string source { get; set; }
        public object[] tours { get; set; }
        public object[] videos { get; set; }
    }

    public class OSI
    {
        public string group_code { get; set; }
        public string sub_code { get; set; }
        public string sub_group_code { get; set; }

    }
    public class Photo
    {
        public string caption { get; set; }
        public string large_url { get; set; }
        public string media_type { get; set; }
        public string medium_square_url { get; set; }
        public string thumb_url { get; set; }
        public string url { get; set; }
    }

    public class Upgrade_Eligibility
    {
        public string application { get; set; }
        public string assessment { get; set; }
        public string code { get; set; }
        public string commercial_name { get; set; }
        public string description { get; set; }
        public string group_code { get; set; }
        public string headline { get; set; }
        public string id { get; set; }
        public string large_icon_url { get; set; }
        public string small_icon_url { get; set; }
        public string sub_group_code { get; set; }
    }

    public class Itinerary
    {
        public Data1 data { get; set; }
        public bool is_lcc { get; set; }
        public Rq rq { get; set; }
        public bool valid { get; set; }
    }

    public class Data1
    {
        public Leg[] legs { get; set; }
    }

    public class Leg
    {
        public string amenity_summary { get; set; }
        public Fare[] fares { get; set; }
        public string total_travel_time { get; set; }
    }

    public class Fare
    {
        public string[] advance_change { get; set; }
        public string brand { get; set; }
        public string[] cancellation { get; set; }
        public Segment[] segments { get; set; }
    }

    public class Segment
    {
        public Amenity1 amenity { get; set; }
        public string[][] boarding_priority { get; set; }
        public string[][] carry_on_bag { get; set; }
        public string[][] check_in_priority { get; set; }
        public string[][] checked_bag { get; set; }
        public string id { get; set; }
        public string[][] lounge_access { get; set; }
        public string overnight { get; set; }
        public object[][] same_day_change { get; set; }
        public string[][] seat_selection { get; set; }
        public string[] upas { get; set; }
        public string[][] upgrade_eligibility { get; set; }
    }

    public class Amenity1
    {
        public string aircraft { get; set; }
        public string beverage { get; set; }
        public string entertainment { get; set; }
        public string fresh_food { get; set; }
        public string layout { get; set; }
        public string power { get; set; }
        public string seat { get; set; }
        public string wifi { get; set; }
    }

    public class Rq
    {
        public string pos { get; set; }
        public Segment1[] segments { get; set; }
        public string src { get; set; }
    }

    public class Segment1
    {
        public string arr { get; set; }
        public int cabin { get; set; }
        public string cxr { get; set; }
        public string date { get; set; }
        public string dep { get; set; }
        public string fbc { get; set; }
        public int fltno { get; set; }
    }

}
