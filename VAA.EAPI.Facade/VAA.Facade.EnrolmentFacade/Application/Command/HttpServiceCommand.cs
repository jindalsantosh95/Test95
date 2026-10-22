using System.Collections.Generic;
using System.Net.Http;
using VAA.Facade.EnrolmentFacade.Domain.Events;

namespace VAA.Facade.EnrolmentFacade.Application.Command
{
    public class HttpServiceCommand
    {
        public EnrolmentRequestCommand EnrolmentRequestCommand { get; set; }
        public EventGridTopicEvent EventGridTopicEvent { get; set; }
        public string url { get; set; }
        public List<KeyValuePair<string, string>> headersList { get; set; }

        public HttpMethod RequestType { get; set; }



        public HttpServiceCommand(EnrolmentRequestCommand enrolmentRequestCommand, EventGridTopicEvent eventGridTopicEvent, string URL, List<KeyValuePair<string, string>> list, HttpMethod requestType) {

            EnrolmentRequestCommand = enrolmentRequestCommand;
            EventGridTopicEvent = eventGridTopicEvent;
            url = URL;
            headersList = list;
            RequestType = requestType;
        }
    }
}
