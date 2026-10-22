using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.CRM.Notification.Domain.Entities.FlightOrderNotification;

namespace VAA.EAPI.Microservices.CRM.Notification.Infrastructure.Models.Airship
{    
    public class NotifRequest
    {
        public Audience Audience { get; set; }
        public Notification Notification { get; set; }
        public IEnumerable<string> Device_types { get; set; }
        public Campaigns Campaigns { get; set; }

        public static explicit operator NotifRequest(AirshipRequest request)
        {
            return new NotifRequest
            {
                Audience = new Audience
                {
                    Tag = request.PnrsWithDate,
                    Group = request.Group
                },
                Notification = new Notification
                {
                    Alert = request.Message,
                    Actions = new Actions
                    {
                        Open = new Open
                        {
                            Type = Constants.Constants.NotifReqType,
                            Content = request.DeepLink
                        },
                        AddCustomEvent = string.IsNullOrWhiteSpace(request.CustomEventName)
                            ? null
                            : new CustomEvent { Name = request.CustomEventName }
                    }
                },
                Device_types = [Constants.Constants.DeviceTypeAndroid, Constants.Constants.DeviceTypeIos],

                Campaigns = request.Campaigns?.Categories == null ? null : new Campaigns
                {
                    Categories = request.Campaigns.Categories
                }
            };
        }

    }
}
