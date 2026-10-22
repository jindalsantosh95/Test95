using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Order.Domain.OrderHistoryRQ;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ValidationMS;

namespace VAA.EAPI.NDC.Order.Validations.OrderHistory
{
    public static class OrderHistoryRequestValidation
    {
        public static IEnumerable<ErrorDetail> ValidateDistributionChain(IATA_OrderHistoryRQ OrderHistoryRQ)
        {
            string totalDC = OrderHistoryRQ.DistributionChain.Length.ToString();
            var errors = new List<ErrorDetail>();

            if (!OrderHistoryRQ.DistributionChain.Any(x => x.OrgRole == DistributionChainRoleCodeType.Carrier))
            {
                errors.Add(new ErrorDetail
                {
                    ErrorCode = "914_17",
                    IsDynamicMessage = false
                });
            }

            if (errors.Count == 0)
            {
                foreach (var dc in OrderHistoryRQ.DistributionChain)
                {
                    var emailId = dc.ContactInfo?.EmailAddress?.FirstOrDefault()?.EmailAddressText;
                    var contactInfo = dc.ContactInfo?.ContactInfoID;

                    if (dc.ParticipatingOrg is null || string.IsNullOrEmpty(dc.ParticipatingOrg.OrgID))
                    {
                        errors.Add(new ErrorDetail
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "912_21"
                        });
                    }

                    else if (dc.OrgRole == DistributionChainRoleCodeType.Carrier && dc.Ordinal != totalDC)
                    {
                        errors.Add(new ErrorDetail
                        {
                            ErrorCode = "912_24",
                            IsDynamicMessage = false
                        });
                    }

                    else if (dc.OrgRole == DistributionChainRoleCodeType.Carrier && dc.ParticipatingOrg.OrgID != Constants.Application.OwnerAirline)
                    {
                        errors.Add(new ErrorDetail
                        {
                            ErrorCode = "914_17",
                            IsDynamicMessage = false
                        });
                    }

                    else if ((dc.SalesAgent != null && string.IsNullOrEmpty(dc.SalesAgent.SalesAgentID)) || (dc.SalesBranch != null && string.IsNullOrEmpty(dc.SalesBranch.SalesBranchID)))
                    {
                        errors.Add(new ErrorDetail
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "913_4"
                        });
                    }

                    else if (string.IsNullOrEmpty(dc.Ordinal) || (dc.Ordinal.Equals("0")) || (dc.Ordinal.Any(Char.IsLetter)) || (dc.OrgRole == DistributionChainRoleCodeType.Seller && !dc.Ordinal.StartsWith("1")))
                    {
                        errors.Add(new ErrorDetail
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "913_4"
                        });
                    }

                    else if (contactInfo.IsNullOrEmpty() && dc.OrgRole.ToString().Equals("Seller"))
                    {
                        errors.Add(new ErrorDetail
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "912_34"
                        });
                    }

                    else if (!string.IsNullOrEmpty(emailId) && (emailId.Length > Convert.ToInt32(Constants.Application.EmailMaxLength) || !RegexCompiled.Match(emailId, Constants.Application.RegexToValidateEmail).Success))
                    {
                        errors.Add(new ErrorDetail
                        {
                            IsDynamicMessage = false,
                            ErrorCode = "317_1"
                        });
                    }

                }
            }
            return errors;
        }



        public static IEnumerable<ErrorDetail> ValidateRLOCAndOrderID(IATA_OrderHistoryRQ OrderHistoryRQ)
        {
            var orderID = OrderHistoryRQ?.Request?.Order?.OrderID;

            if (string.IsNullOrEmpty(orderID))
            {
                return new List<ErrorDetail>()
                {
                    new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "912_1",
                    }
                };
            }

            return Enumerable.Empty<ErrorDetail>();
        }

        public static IEnumerable<ErrorDetail> ValidateOrderIDFormat(IATA_OrderHistoryRQ OrderHistoryRQ)
        {
            var orderID = OrderHistoryRQ?.Request?.Order.OrderID;

            if (!string.IsNullOrEmpty(orderID) && !RegexCompiled.Match(orderID, "^[A-Z0-9]*$").Success)
            {
                return new List<ErrorDetail>()
                {
                    new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "914_1",
                    }
                };
            }

            return Enumerable.Empty<ErrorDetail>();
        }

        public static IEnumerable<ErrorDetail> ValidateRLOCFormat(IATA_OrderHistoryRQ OrderHistoryRQ)
        {
            var orderId = OrderHistoryRQ?.Request?.Order?.OrderID; 
            string rLOC = string.Empty;

            if (!string.IsNullOrEmpty(orderId))
            {
                if (orderId.Length > 7)
                {
                    rLOC = orderId.Substring(5, orderId.Length - 7); // middle part only
                }
            }

            if (!string.IsNullOrEmpty(rLOC) && (!RegexCompiled.Match(rLOC, "^[A-Z0-9]*$").Success || rLOC.Length != Constants.Application.PNRLength))
            {
                return new List<ErrorDetail>()
                {
                    new ErrorDetail
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "914_2",
                    }
                };
            }

            return Enumerable.Empty<ErrorDetail>();
        }

        public static IEnumerable<ErrorDetail> ValidateAgent(IATA_OrderHistoryRQ OrderHistoryRQ, ValidationRS validationRS)
        {
            var distributionChain = OrderHistoryRQ.DistributionChain.Where(x => x.OrgRole != DistributionChainRoleCodeType.Carrier).OrderBy(x => int.Parse(x.Ordinal));

            if (validationRS.ValidateResponse.KeyType == "DIRECT" && (distributionChain.Count() != 2 && distributionChain.Any(x => x.OrgRole == DistributionChainRoleCodeType.Distributor) || distributionChain.FirstOrDefault()?.ParticipatingOrg?.OrgID != validationRS.ValidateResponse.AgencyDetails.RequestAgencyId))
            {
                return new List<ErrorDetail>()
                {
                    new ErrorDetail()
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "368_2"
                    }
                };
            }

            if (validationRS.ValidateResponse.KeyType == "INDIRECT" && (distributionChain.Count() != 2 || !distributionChain.Any(x => x.OrgRole == DistributionChainRoleCodeType.Seller && x.ParticipatingOrg?.OrgID == validationRS.ValidateResponse.AgencyDetails.RequestAgencyId
            || !distributionChain.Any(x => x.OrgRole != DistributionChainRoleCodeType.Seller && x.ParticipatingOrg?.OrgID == validationRS.ValidateResponse.AggregatorDetails.AgencyId))))
            {
                return new List<ErrorDetail>()
                {
                    new ErrorDetail()
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "368_2"
                    }
                };
            }

            var email = distributionChain.Count() > 1 ? distributionChain.OrderBy(x => x.Ordinal).FirstOrDefault()?.ContactInfo?.EmailAddress?.FirstOrDefault()?.EmailAddressText : distributionChain.FirstOrDefault()?.ContactInfo?.EmailAddress?.FirstOrDefault()?.EmailAddressText;

            if (validationRS.ValidateResponse.AgencyDetails.RequestAgencyIdType == Constants.Application.AgentTypeIATA && string.IsNullOrEmpty(email))
            {
                return new List<ErrorDetail>()
                {
                    new ErrorDetail()
                    {
                        IsDynamicMessage = false,
                        ErrorCode = "368_7"
                    }
                };
            }

            return Enumerable.Empty<ErrorDetail>();
        }
    }
}
