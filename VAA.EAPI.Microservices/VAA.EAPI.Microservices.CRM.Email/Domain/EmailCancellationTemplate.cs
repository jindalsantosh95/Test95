using System.Collections.Generic;
using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.CRM.Email.Domain
{
    public class EmailCancellationTemplate
    {
        [XmlRoot("ctx")]
        public class ctx
        {
            [XmlElement("SendMessage")]
            public SendMessage SendMessage { get; set; }
        }

        public class SendMessage
        {
            [XmlElement("ProcessingMetadata")]
            public ProcessingMetadata ProcessingMetadata { get; set; }

            [XmlElement("MessageData")]
            public MessageData MessageData { get; set; }
        }

        public class ProcessingMetadata
        {
            [XmlElement("EmailAddress")]
            public string EmailAddress { get; set; }

            [XmlElement("FromAddress")]
            public string FromAddress { get; set; }

            [XmlElement("Channel")]
            public string Channel { get; set; }

            [XmlElement("LanguageCode")]
            public string LanguageCode { get; set; }

            [XmlElement("Template")]
            public string Template { get; set; }
        }

        public class MessageData
        {
            [XmlElement("document")]
            public List<document> document { get; set; }
        }

        public class document
        {
            [XmlElement("request")]
            public request request { get; set; }
        }

        public class request
        {
            [XmlElement("template_id")]
            public string template_id { get; set; }

            [XmlElement("email_address")]
            public string email_address { get; set; }

            [XmlElement("MAILING_ID")]
            public MAILING_ID MAILING_ID { get; set; }

            [XmlElement("CONTENT_DATA")]
            public CONTENT_DATA CONTENT_DATA { get; set; }
        }
        public class MAILING_ID
        {
            [XmlElement("DATE_SENT")]
            public string DATE_SENT { get; set; }

            [XmlElement("CHANNEL")]
            public string CHANNEL { get; set; }

            [XmlElement("LANGUAGE")]
            public string LANGUAGE { get; set; }

            [XmlElement("DESCRIPTIVE")]
            public string DESCRIPTIVE { get; set; }
        }
        public class CONTENT_DATA
        {
            [XmlElement("FIRST_NM")]
            public string FIRST_NM { get; set; }

            [XmlElement("LAST_NM")]
            public string LAST_NM { get; set; }

            [XmlElement("REC_LOC")]
            public string REC_LOC { get; set; }

            [XmlElement("SKYMILES_COLUMN")]
            public string SKYMILES_COLUMN { get; set; }

            [XmlElement("PAX_INFO")]
            public List<PAX_INFO> PAX_INFO { get; set; }
        }

        public class PAX_INFO
        {
            [XmlElement("NAME")]
            public string NAME { get; set; }

            [XmlElement("SM_NO")]
            public string SM_NO { get; set; }

            [XmlElement("ETKT")]
            public string ETKT { get; set; }

            [XmlElement("CN_AUTH")]
            public string CN_AUTH { get; set; }
        }
    }
}
