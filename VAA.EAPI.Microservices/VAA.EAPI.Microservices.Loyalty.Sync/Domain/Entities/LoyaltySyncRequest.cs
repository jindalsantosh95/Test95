using MediatR;
using System;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Domain.Entities
{
    public class LoyaltySyncRequest : IRequest
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Subject { get; set; }
        public LoyaltySyncRequestData Data { get; set; }
        public string Source { get; set; }
        public string Key { get; set; }
        public object Specversion { get; set; }
    }
}