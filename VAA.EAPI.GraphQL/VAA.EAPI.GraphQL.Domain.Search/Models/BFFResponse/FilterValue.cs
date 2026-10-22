using System.Web;
using VAA.EAPI.GraphQL.Common.Extensions;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFResponse
{
    public class FilterValue
    {
        public FilterValueDetails Values { get; set; }
        public string Code { get; set; }
        public bool Enabled => Count > 0;
        public int Count { get; set; }
        public decimal MinPrice { get; set; }
        public string? Currency { get; set; }
        public string? DisplayName { get; set; }
        public string BaseUrl { get; set; }
        public string SelectedSort { get; set; }
        public FlightOffer.FlightOfferRequestFilters? SelectedFilters { get; set; }
        public string FilterType { get; set; }

        public bool Selected
        {
            get
            {
                if (FilterType == Constants.Application.FILTER_AIRLINE_NAME)
                    return SelectedFilters?.Airlines != null
                        && SelectedFilters.Airlines != Constants.Application.FILTER_PLACEHOLDER
                        && SelectedFilters.Airlines.Split(Constants.Application.FILTER_SEPARATOR).Contains(Code);

                if (FilterType == Constants.Application.FILTER_DEPARTURE_TIME)
                    return SelectedFilters?.DepartureTimes != null
                        && SelectedFilters.DepartureTimes != Constants.Application.FILTER_PLACEHOLDER
                        && SelectedFilters.DepartureTimes.Split(Constants.Application.FILTER_SEPARATOR).Contains(Code);

                if (FilterType == Constants.Application.FILTER_CONNECTIONS)
                    return SelectedFilters?.Connections != null
                        && SelectedFilters.Connections != Constants.Application.FILTER_PLACEHOLDER
                        && SelectedFilters.Connections.Split(Constants.Application.FILTER_SEPARATOR).Contains(Code);

                if (FilterType == Constants.Application.FILTER_CABIN)
                    return SelectedFilters?.Cabins != null
                        && SelectedFilters.Cabins != Constants.Application.FILTER_CABIN
                        && SelectedFilters.Cabins.Split(Constants.Application.FILTER_SEPARATOR).Contains(Code);

                return false;
            }
        }
        public IEnumerable<Link> Links
        {
            get
            {
                var filterPath = GetFilterPath(SelectedFilters?.Airlines, SelectedFilters?.DepartureTimes, SelectedFilters?.Connections, SelectedFilters?.Cabins);
                return new Link[]
                {
                    new() { Rel = $"{FilterType}/{filterPath}", Href = $"{BaseUrl}/{SelectedSort}/{filterPath}" }
                };
            }
        }
        public IEnumerable<Link> ClearFilterLink
        {
            get
            {
                string filterPath = Constants.Application.FILTER_PLACEHOLDER;

                if (FilterType == Constants.Application.FILTER_AIRLINE_NAME)
                    filterPath = $"{Constants.Application.FILTER_PLACEHOLDER}/{SelectedFilters?.DepartureTimes ?? Constants.Application.FILTER_PLACEHOLDER}/{SelectedFilters?.Connections ?? Constants.Application.FILTER_PLACEHOLDER}/{SelectedFilters?.Cabins ?? Constants.Application.FILTER_PLACEHOLDER}";

                if (FilterType == Constants.Application.FILTER_DEPARTURE_TIME)
                    filterPath = $"{SelectedFilters?.Airlines ?? Constants.Application.FILTER_PLACEHOLDER}/{Constants.Application.FILTER_PLACEHOLDER}/{SelectedFilters?.Connections ?? Constants.Application.FILTER_PLACEHOLDER}/{SelectedFilters?.Cabins ?? Constants.Application.FILTER_PLACEHOLDER}";

                if (FilterType == Constants.Application.FILTER_CONNECTIONS)
                    filterPath = $"{SelectedFilters?.Airlines ?? Constants.Application.FILTER_PLACEHOLDER}/{SelectedFilters?.DepartureTimes ?? Constants.Application.FILTER_PLACEHOLDER}/{Constants.Application.FILTER_PLACEHOLDER}/{SelectedFilters?.Cabins ?? Constants.Application.FILTER_PLACEHOLDER}";


                if (FilterType == Constants.Application.FILTER_CABIN)
                    filterPath = $"{SelectedFilters?.Airlines ?? Constants.Application.FILTER_PLACEHOLDER}/{SelectedFilters?.DepartureTimes ?? Constants.Application.FILTER_PLACEHOLDER}/{SelectedFilters?.Connections ?? Constants.Application.FILTER_PLACEHOLDER}/{Constants.Application.FILTER_PLACEHOLDER}";

                return new Link[]
                {
                    new() { Rel = $"clear_{FilterType}", Href = $"{BaseUrl}/{SelectedSort}/{filterPath}" }
                };
            }
        }

        private string GetFilterPath(string? airlinesFilter, string? departureTimesFilter, string? connectionsFilter, string? cabinsFilter)
        {
            if (FilterType == Constants.Application.FILTER_AIRLINE_NAME)
                airlinesFilter = GetFilterWithOrWithoutItem(airlinesFilter);

            if (airlinesFilter.IsNullOrEmpty())
                airlinesFilter = Constants.Application.FILTER_PLACEHOLDER;

            if (FilterType == Constants.Application.FILTER_DEPARTURE_TIME)
                departureTimesFilter = GetFilterWithOrWithoutItem(departureTimesFilter);

            if (departureTimesFilter.IsNullOrEmpty())
                departureTimesFilter = Constants.Application.FILTER_PLACEHOLDER;

            if (FilterType == Constants.Application.FILTER_CONNECTIONS)
                connectionsFilter = GetFilterWithOrWithoutItem(connectionsFilter);

            if (connectionsFilter.IsNullOrEmpty())
                connectionsFilter = Constants.Application.FILTER_PLACEHOLDER;

            if (FilterType == Constants.Application.FILTER_CABIN)
            {
                // Use single-select for cabinsFilter: choosing another value will clear the existing value
                cabinsFilter = cabinsFilter == Code ? Constants.Application.FILTER_PLACEHOLDER : Code;
            }

            if (cabinsFilter.IsNullOrEmpty())
                cabinsFilter = Constants.Application.FILTER_PLACEHOLDER;

            return $"{airlinesFilter}/{departureTimesFilter}/{connectionsFilter}/{HttpUtility.UrlEncode(cabinsFilter)}";
        }

        private string GetFilterWithOrWithoutItem(string? filterFromUrl)
        {
            if (filterFromUrl == Constants.Application.FILTER_PLACEHOLDER)
                return Code;

            var selectedValues = filterFromUrl?.Split(Constants.Application.FILTER_SEPARATOR) ?? Enumerable.Empty<string>();

            selectedValues = Selected ? selectedValues.Where(a => a != Code) : selectedValues.Append(Code).OrderBy(c => c);

            return string.Join(Constants.Application.FILTER_SEPARATOR, selectedValues);
        }
    }

    public class FilterValueDetails
    {
        public string? Airline { get; set; }
        public string? Connections { get; set; }
        public string? Cabin { get; set; }
        public string? DepartureTime { get; set; }
    }
}
