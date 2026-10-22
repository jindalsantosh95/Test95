using System;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Microservices.Retail.Order.Domain.Order.CustomerEnrollment;
using VAA.EAPI.Microservices.Retail.Order.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Order.Domain.Order;
using VAA.EAPI.Microservices.Retail.Order.Domain.Order.Remarks;
using VAA.EAPI.Microservices.Retail.Order.Domain.Order.SSR;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Mapper.OrderCreate
{
    public class OrderDocumentMapper
    {
        public static OrderDocument MapOrderDocument(OrderResponse orderResponse, string docType, bool? isInsuranceAdded)
        {
            return MapOrderDocument(orderResponse, docType, new OrderCreateRequest { IsInsuranceAdded = isInsuranceAdded });
        }

        public static OrderDocument MapOrderDocument(OrderResponse orderResponse, string docType, OrderCreateRequest orderCreateRequest)
        {
            string docId = orderResponse.OrderNumber?.OrderId;

            return new OrderDocument
            {
                Id = docId,
                PartitionKey = docId,
                CreatedBy = Constants.Application.DCOMChannel,
                CreatedOn = DateTime.UtcNow.ToString(Constants.Application.ZuluDateTimeFormat),
                DocType = docType,
                PointOfSale = orderResponse.PointOfSale,
                PaymentDetails = orderResponse.PaymentDetails,
                OrderItems = orderResponse.OrderItems,
                OrderNumber = orderResponse.OrderNumber,
                DataList = orderResponse.DataList,
                OrderStatus = orderResponse.OrderStatus,
                BookingReference = orderResponse.BookingReference,
                CreationDateTime = orderResponse.CreationDateTime,
                TimeLimits = orderResponse.TimeLimits,
                Warnings = orderResponse.Warnings,
                BookingType = orderResponse.BookingType,
                OrderType = orderResponse.OrderType,
                IsInsuranceAdded = orderCreateRequest?.IsInsuranceAdded,
                VoucherApplied = orderCreateRequest?.VoucherApplied == true && orderCreateRequest.VoucherDetails is not null,
                VoucherDetails = orderCreateRequest?.VoucherApplied == true ? orderCreateRequest.VoucherDetails : null
            };
        }

        public static CloudEventData MapCustomerDetails(OrderCreateRequest orderCreateRequest)
        {
            var leadPassenger = orderCreateRequest.CustomerDetails.First(customer => customer.Ptc == Constants.Application.DeltaAdultPTC);
            return new CloudEventData
            {
                CustomerDetails = new CustomerDetails()
                {
                    Firstname = leadPassenger.FirstName,
                    Lastname = leadPassenger.LastName,
                    CustomerType = (int)CustomerTypeEnum.Basic,
                    Preferences = new Preferences[] { new Preferences { PreferenceType = Preferences.PreferenceTypes.VAAMarketingPermissions, PreferenceValue = "Y" } },
                    ContactDetails = new Contact { EmailDetails = new Email[] { new Email { EmailId = leadPassenger.Contacts.Email, PreferredFlag = "Y" } } },
                    LoyaltyMember = new LoyaltyMember { EnrollmentChannel = Constants.Application.EnrollmentChannel, MediaCode = Constants.Application.MediaCode }
                },
                ChannelUserId = string.Empty
            };
        }

        public static OrderChange OrderChildSsrMapping(OrderCreateRequest orderCreateRequest, OrderResponse orderResponse)
        {
            var specialServiceRequests = orderResponse.DataList.CustomerDetails.Where(customer => customer.Ptc == Constants.Application.ChildPtc).SelectMany(customer => orderResponse.DataList.SegmentDetails.Where(segment => segment.OperatingCarrier == "VS").Select(segment => new SpecialServiceRequestInfo
            {
                AirlineCode = Constants.Application.AirlineCode,
                AllPassengersFlag = "N",
                AllSegmentsFlag = "N",
                RequestCode = Constants.Application.ChildMeal,
                StatusCode = Constants.Application.SsrStatusCode,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                FirstNameNumber = customer.FirstNameNumber,
                LastNameNumber = customer.LastNameNumber,
                SegmentNumber = Convert.ToString(segment.SegmentSequence)
            }));

            if (!specialServiceRequests.Any())
            {
                return null;
            }

            return new OrderChange
            {
                Pos = new Pos { CityCode = orderCreateRequest.Pos.CityCode },
                Rloc = orderResponse.BookingReference.RLOC,
                ChannelId = Constants.Application.Channel,
                OrderAmendments = new List<OrderAmendment> { new OrderAmendment { ProductType = "SSR", SpecialServiceRequestInfo = specialServiceRequests.ToList() } }
            };
        }

        public static AddSpecialRemarkRequest MapVoucherRedemptionRemark(OrderCreateRequest orderCreateRequest, OrderResponse orderResponse)
        {
            var voucherDetails = orderCreateRequest?.VoucherDetails;

            if (voucherDetails?.AppliedVouchers is null || !voucherDetails.AppliedVouchers.Any())
            {
                return null;
            }

            return new AddSpecialRemarkRequest
            {
                Pnr = orderResponse?.BookingReference?.RLOC,
                Location = orderCreateRequest.Pos?.CityCode,
                RemarkType = Constants.Application.SpecialRemarkType,
                RemarkSubType = Constants.Application.VoucherRedemptionRemarkSubType,
                VoucherDetails = new VoucherRemarkDetails
                {
                    LoyaltyNumber = voucherDetails.VoucherOwnerFcAccount,
                    RedemptionType = voucherDetails.RedemptionType,
                    TotalPoints = voucherDetails.TotalPointsUsed,
                    Vouchers = voucherDetails.AppliedVouchers.Select(voucher => new VoucherRemark
                    {
                        VoucherNumber = voucher.VoucherId,
                        VoucherPointsApplied = voucher.PointsUsed
                    }).ToList()
                }
            };
        }

    }
}
