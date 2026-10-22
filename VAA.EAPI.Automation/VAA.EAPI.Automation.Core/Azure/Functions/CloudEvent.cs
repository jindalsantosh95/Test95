namespace VAA.EAPI.Automation.Core.Azure.Functions
{
    /// <summary>
    /// CloudEvents is a specification for describing event data in common formats to provide interoperability across services, platforms 
    /// and systems. Based on https://github.com/cloudevents/spec
    /// </summary>
    public class CloudEvent : INotification
    {
        /// <summary>
        /// The version of the CloudEvents specification which the event uses. This enables the interpretation of the context. Compliant 
        /// event producers MUST use a value of 1.0 when referring to this version of the specification.
        /// </summary>
        public string Specversion { get; set; } = "1.0";

        /// <summary>
        /// This attribute contains a value describing the type of event related to the originating occurrence. Often this attribute is
        /// used for routing, observability, policy enforcement, etc. The format of this is producer defined and might include information 
        /// such as the version of the type.
        /// 
        /// Must follow the following convention: {Organisation}.{Consumer}.{Domain}.{Action}, E.g.: 'VAA.NDC.Order.InitiateOrderChange'
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Identifies the context in which an event happened. Often this will include information such as the type of the event source, 
        /// the organization publishing the event or the process that produced the event. The exact syntax and semantics behind the data
        /// encoded in the URI is defined by the event producer.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Identifies the event. Producers MUST ensure that source + id is unique for each distinct event. If a duplicate event 
        /// is re-sent (e.g. due to a network error) it MAY have the same id. Consumers MAY assume that Events with identical source and id 
        /// are duplicates.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Originating user [UPN] or service that created the event.
        /// </summary>
        public string Originator { get; set; }

        /// <summary>
        /// Timestamp of when the occurrence happened. If the time of the occurrence cannot be determined then this attribute MAY be set
        /// to some other time (such as the current time) by the CloudEvents producer, however all producers for the same source MUST be
        /// consistent in this respect.
        /// </summary>
        public string Time { get; set; }

        /// <summary>
        /// This describes the subject of the event in the context of the event producer (identified by source). In publish-subscribe 
        /// scenarios, a subscriber will typically subscribe to events emitted by a source, but the source identifier alone might not be 
        /// sufficient as a qualifier for any specific event if the source context has internal sub-structure.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Identifies the schema that data adheres to. Incompatible changes to the schema SHOULD be reflected by a different URI. 
        /// </summary>
        public string Dataschema { get; set; }

        /// <summary>
        /// Content type of data value. This attribute enables data to carry any type of content, whereby format and encoding might differ 
        /// from that of the chosen event format.
        /// </summary>
        public string DataContentType { get; set; } = "application/json";

        /// <summary>
        /// The event payload. This specification does not place any restriction on the type of this information. It is encoded into a media 
        /// format which is specified by the datacontenttype attribute (e.g. application/json), and adheres to the dataschema format when 
        /// those respective attributes are present.
        /// </summary>
        public object Data { get; set; }

        public bool DiagnosticsEnabled { get; set; }

        public Guid SessionId { get; set; }

        public Guid OperationId { get; set; }

        public Guid CorrelationId { get; set; }

        public string[] Channel { get; set; }
    }
}
