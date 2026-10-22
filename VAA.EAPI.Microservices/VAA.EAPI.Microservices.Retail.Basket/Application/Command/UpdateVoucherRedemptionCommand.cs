using CSharpFunctionalExtensions;
using MediatR;
using System;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Basket.Domain;

namespace VAA.EAPI.Microservices.Retail.Basket.Application.Command
{
    public sealed class UpdateVoucherRedemptionCommand : IRequest<Result<Response<Domain.Basket>>>
    {
        public Guid Id { get; set; }
        public bool? HasSufficientPoints { get; set; }
        public string? VoucherPoints { get; set; }
        public string? PointsToPay { get; set; }
        public bool VoucherApplied { get; set; }
        public VoucherDetails? VoucherDetails { get; set; }
        public VoucherRedemptionSelection? VoucherRedemptionSelection { get; set; }
    }
}
