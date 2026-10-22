using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Wallet.Domain.BoardingPass;
using VAA.EAPI.Microservices.Loyalty.Wallet.Helper;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Infrastructure.Airship
{
    public class PassengerRequest
    {
        public string AdaptiveLinkExternalId { get; set; }
        public PassengerFields Fields { get; set; }
        public PassengerSemantics Semantics { get; set; }

        public static explicit operator PassengerRequest(BoardingPass source)
        {
            if (source == null || source.QrCodeData.IsNullOrEmpty())
            {
                return null;
            }

            var tierName = AirshipWalletHelper.GetTierName(source?.TierCode);

            Match? seatInfo = null;
            if (!string.IsNullOrWhiteSpace(source.SeatNumber) && !source.SeatNumber.Equals("INF", StringComparison.OrdinalIgnoreCase))
            {
                seatInfo = Regex.Match(source.SeatNumber.Trim(), @"^(\d+)([A-Za-z]+)$");
            }

            return new PassengerRequest
            {
                AdaptiveLinkExternalId = $"{source.OperatingFlightNumber}-{source.DepartureLocal}-{source.PnrLocator}-{source.PassengerNIN}",
                Fields = new PassengerFields
                {
                    SeatNumber = new FieldValue
                    {
                        Value = source?.SeatNumber ?? string.Empty
                    },
                    ConfirmationCode = new FieldValue
                    {
                        Value = source?.PnrLocator
                    },
                    BoardingTime = new FieldValue
                    {
                        Value = source?.BoardingTimeLocalTime?.ToString() ?? string.Empty
                    },
                    BoardingGroup = new FieldValue
                    {
                        Value = source?.Zone ?? string.Empty
                    },
                    Remarks = new FieldValue
                    {
                        Value = source?.VerificationStatusMessage
                    },
                    SeatClass = new FieldValue
                    {
                        Value = source?.CabinName
                    },
                    PassengerName = new FieldValue
                    {
                        Value = $"{source?.Surname}/{source?.GivenNames}"
                    },
                    Barcode_value = new FieldValue
                    {
                        Value = source?.QrCodeData
                    },
                    SecurityProgramLogo = new FieldValue
                    {
                        Value = source.ShowTsaPre ? ConfigReader.Get(Constants.Environment.TSAPreImgUrl) : string.Empty
                    },
                    BoardingPrivilegeImage = new FieldValue
                    {
                        Value = source.ShowSkyPriority ? ConfigReader.Get(Constants.Environment.SkyPriorityImgUrl) : string.Empty
                    },
                    Footer_image = new FieldValue
                    {
                        Value = AirshipWalletHelper.GetSecureProgramImageUrl(source.ShowSkyPriority, source.ShowTsaPre)
                    },
                    TierCode = new FieldValue
                    {
                        Value = source?.TierCode
                    },
                    TierName = new FieldValue
                    {
                        Value = tierName
                    }
                },
                Semantics = new PassengerSemantics
                {
                    PassengerName = new Infrastructure.Airship.PassengerName
                    {
                        GivenName = source?.GivenNames,
                        FamilyName = source?.Surname
                    },
                    BoardingGroup = source?.Zone?.Replace("GROUP", "", StringComparison.OrdinalIgnoreCase)?.Replace("ZONE", "", StringComparison.OrdinalIgnoreCase)?.Trim() ?? string.Empty,
                    PassengerEligibleSecurityPrograms = source.ShowTsaPre ? ["tsaPreCheck"] : [],
                    PassengerCapabilities = new List<string> { "carryOn" }, //default value need to send
                    PriorityStatus = source.ShowSkyPriority ? "SkyPriority" : string.Empty,
                    MembershipProgramStatus = !tierName.IsNullOrEmpty() ? tierName : null,
                    MembershipProgramName = !tierName.IsNullOrEmpty() ? "Flying Club" : null,
                    InternationalDocumentsAreVerified = source.VerificationStatusMessage.Is("DOCS-OK"),
                    InternationalDocumentsVerifiedDeclarationName = source.VerificationStatusMessage,
                    TicketFareClass = source.CabinName,
                    Seats = [ new Seat
                    {
                        SeatType = source.CabinName,
                        SeatNumber = seatInfo?.Success == true ? seatInfo.Groups[2].Value : null,
                        SeatRow = seatInfo?.Success == true ? seatInfo.Groups[1].Value :null
                    }]
                }
            };
        }
    }
}
