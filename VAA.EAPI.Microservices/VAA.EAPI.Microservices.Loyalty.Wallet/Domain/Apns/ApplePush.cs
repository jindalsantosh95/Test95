using System;
using System.Collections.Generic;
using System.Dynamic;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Domain.Apns
{
    public class ApplePush
    {
        public string Token { get; set; }
        
        public int Priority => CustomPriority ?? (Type == ApplePushType.Background ? 5 : 10);

        public ApplePushType Type { get; }

        public int? CustomPriority { get;  set; }

        public ApplePushAlert Alert { get; set; }

        public ApplePushLocalizedAlert LocalizedAlert { get; set; }

        public int? Badge { get; set; }        

        public string Sound { get;  set; }
        
        public string Category { get; set; }

        public bool IsContentAvailable { get; set; }

        public bool IsMutableContent { get; set; }     

        public Dictionary<string, object> CustomProperties { get; set; }

        public IDictionary<string, object> CustomApsProperties { get; set; }

        public bool _sendAlertAsText;

        public bool IsSendToDevelopmentServer { get;  set; }

        public ApplePush(ApplePushType pushType)
        {
            Type = pushType;
        }

        public ApplePush AddAlert( string title, string body)
        {
            Alert = new ApplePushAlert() { Title = title, Body = body };

            if (title == null)
                _sendAlertAsText = true;

            return this;
        }      

        public ApplePush AddToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentNullException(nameof(token)); 
            
            Token = token;

            return this;
        }

        public ApplePush SendToDevelopmentServer(bool isDevelopmentServer)
        {
            IsSendToDevelopmentServer = isDevelopmentServer;

            return this;
        }        

        public object GeneratePayload()
        {
            dynamic payload = new ExpandoObject();
            payload.aps = new ExpandoObject();
            IDictionary<string, object> apsAsDict = payload.aps;

            if (IsContentAvailable)
                apsAsDict["content-available"] = "1";

            if (IsMutableContent)
                apsAsDict["mutable-content"] = "1";

            if (Alert != null)
            {
                object alert;

                if (_sendAlertAsText)
                    alert = Alert.Body;
                else if (Alert.Subtitle == null)
                    alert = new { title = Alert.Title, body = Alert.Body };
                else
                    alert = new { title = Alert.Title, subtitle = Alert.Subtitle, body = Alert.Body };

                payload.aps.alert = alert;
            }
            else if (LocalizedAlert != null)
            {
                object localizedAlert = LocalizedAlert;
                payload.aps.alert = localizedAlert;
            }

            if (Badge != null)
                payload.aps.badge = Badge.Value;

            if (Sound != null)
                payload.aps.sound = Sound;

            if (Category != null)
                payload.aps.category = Category;

            if (CustomProperties != null)
            {
                IDictionary<string, object> payloadAsDict = payload;

                foreach (var customProperty in CustomProperties)
                    payloadAsDict[customProperty.Key] = customProperty.Value;
            }

            if (CustomApsProperties != null)
            {
                foreach (var customApsProperty in CustomApsProperties)
                    apsAsDict[customApsProperty.Key] = customApsProperty.Value;
            }

            return (IDictionary<string, object>)payload;
        }
    }
}
