using System.Data;
using System.Text.RegularExpressions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Domain;

namespace VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Mapper
{
    public static class OrderResponseMapper
    {
        public static int CalculateTTL(IEnumerable<SegmentDetail> segmentDetails)
        {
            List<DateTime> departureDateDiffList = new List<DateTime>();

            foreach (SegmentDetail segmentDetail in segmentDetails)
            {
                var departDateTimeOutput = DateTimeHelper.Air4DepartDateFormatConversion(segmentDetail.DepartureDate, Constants.Application.TTLCalInitialYearDigits + segmentDetail.SegmentDepartureYear, segmentDetail.DepartureTime);

                if (departDateTimeOutput.HasValue && departDateTimeOutput >= DateTime.UtcNow)
                {
                    departureDateDiffList.Add(departDateTimeOutput.Value);
                }
            }

            DateTime? maxDepartureDate = departureDateDiffList.OrderByDescending(x => x).FirstOrDefault();
            var ttl = maxDepartureDate != null ? ((maxDepartureDate.Value.AddDays(Constants.Application.TTLCalAdditionalDays)).Subtract(DateTime.UtcNow)).TotalSeconds : 0;

            return ttl > 0 ? Convert.ToInt32(ttl) : 0;
        }

        public static string GetContactEmail(string Email)
        {
            var email = Email.Replace("//", "@").Replace("..", "_").Replace("./", "-").Replace("/", "");
            email = email.Contains(' ') ? email.Split(' ')[1] : email;

            if (!string.IsNullOrWhiteSpace(email) && RegexCompiled.IsMatch(email, Constants.Application.EmailRegex, RegexOptions.IgnoreCase))
            {
                return email;
            }

            return string.Empty;
        }

        public static CustomerOrderDetails GetCustomerOrderDetails(string CustomerID, string OrderID, int TimeToLive)
        {
            return new()
            {
                CustomerID = CustomerID,
                OrderID = OrderID,
                TimeToLive = TimeToLive
            };
        }

        public static string Get_contact_details(string phone)
        {
            phone = RegExHelper.RemoveWhiteSpace(phone.Replace("LON", " 44").Replace("-", "").Replace("*", ""));
            MatchCollection arr = RegexCompiled.Matches(phone, @"[\+\(]?[0-9][0-9 .\-\(\)]{8,}[0-9]");

            if (arr.Count > 0)
            {
                string[] textSplit = arr[0].Value.Split(" ");

                foreach (var a in textSplit)
                {
                    string phoneNumber = a;
                    if (phoneNumber.Length >= 10 && phoneNumber.Length <= 15)
                    {
                        return phoneNumber;
                    }
                    if (textSplit.Length > 1)
                    {
                        foreach (var b in textSplit.Skip(1))
                        {
                            phoneNumber = phoneNumber + b;

                            if (phoneNumber.Length >= 10 && phoneNumber.Length <= 15)
                            {
                                return phoneNumber;
                            }
                        }
                    }
                }
            }

            return string.Empty;
        }
    }
}
