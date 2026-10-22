using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Application.Commands.UpsertTicket;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.TicketData;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Infrastructure.Helper;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Helper
{
    public class TicketFeedParsingHelper
    {
        public static Result<TicketDto> GetTicketDocDetails(string ticketFeed, ILogger<UpsertTicketCommandHandler> logger)
        {
            try
            {
                TicketDto ticketObj = new TicketDto();
                var isPrimaryExcTkt = false;
                var isSecTkt = false;

                #region Exchange Ticket Indicator                
                int eTktDocExcIndx = ticketFeed.IndexOf(Air4TicketFeedTable.ET_TKDC_EXCG_RLN.ToString());
                int eTktDocSecIndx = ticketFeed.IndexOf(Air4TicketFeedTable.ET_TKT_DOC_SEC.ToString());

                if (eTktDocExcIndx > -1)
                {
                    isPrimaryExcTkt = true;

                    if (eTktDocSecIndx > -1 && eTktDocExcIndx > eTktDocSecIndx)
                    {
                        isPrimaryExcTkt = false;
                    }
                }
                var primaryTktStr = ticketFeed;
                var exchangeTktStr = string.Empty;
                var secondaryTktStr = string.Empty;
                if (isPrimaryExcTkt)
                {
                    primaryTktStr = ticketFeed.Substring(0, eTktDocExcIndx);
                    exchangeTktStr = ticketFeed.Substring(eTktDocExcIndx, ticketFeed.Length - primaryTktStr.Length);
                }
                #endregion

                #region Secondary or Conjunctive Tickets
                if (eTktDocSecIndx > -1)
                {
                    isSecTkt = true;
                    if (isPrimaryExcTkt)
                        exchangeTktStr = ticketFeed.Substring(eTktDocExcIndx, ticketFeed.Length - eTktDocSecIndx);
                }
                if (!isPrimaryExcTkt && isSecTkt)
                {
                    primaryTktStr = ticketFeed.Substring(0, eTktDocSecIndx);
                }
                if (isSecTkt)
                {
                    secondaryTktStr = ticketFeed.Substring(eTktDocSecIndx);
                }

                #endregion

                ticketObj = PopulatePrimaryTicket(primaryTktStr, logger);

                ticketObj.ExchangeTickets = PopulateExchangeTickets(exchangeTktStr, logger);

                ticketObj.SecondaryTickets = PopulateConjunctiveTickets(secondaryTktStr, logger);

                ticketObj.DocType = Constants.Application.TicketFeedDocType;

                return Result.Success(ticketObj);
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, "Ticket Feed parsing failed while GetTicketDocDetails");
                throw;
            }
        }

        private static TicketDto PopulatePrimaryTicket(string primaryTktStr, ILogger<UpsertTicketCommandHandler> logger)
        {
            TicketDto objTicketDto;
            try
            {
                var optionalRecord = Air4TicketFeedToEntities<OptionalRecord>.Convert(primaryTktStr, Air4TicketFeedTable.Optional_Record.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var eventHeader = Air4TicketFeedToEntities<EventHeader>.Convert(primaryTktStr, Air4TicketFeedTable.EVENT_HDR.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var primaryTicketDocument = Air4TicketFeedToEntities<TicketDocument>.Convert(primaryTktStr, Air4TicketFeedTable.ET_TKT_DOC.ToString(), Constants.Application.TicketFeedFileDelimiter).FirstOrDefault();
                var ticketDocSource = Air4TicketFeedToEntities<TicketDocumentSource>.Convert(primaryTktStr, Air4TicketFeedTable.ET_TKT_DOC_SRC.ToString(), Constants.Application.TicketFeedFileDelimiter);

                var tktDocElectrAddress = Air4TicketFeedToEntities<ElectronicAddress>.Convert(primaryTktStr, Air4TicketFeedTable.ET_TKDC_ELCR_ADR.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktDocElectrMsg = Air4TicketFeedToEntities<ElectronicMessage>.Convert(primaryTktStr, Air4TicketFeedTable.ET_ELCR_MSG.ToString(), Constants.Application.TicketFeedFileDelimiter);

                var tktDocGphcAddress = Air4TicketFeedToEntities<TicketDocGeographicAddress>.Convert(primaryTktStr, Air4TicketFeedTable.ET_TKDC_GPHC_ADR.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktDocPhysAddress = Air4TicketFeedToEntities<TicketDocPhoneAddress>.Convert(primaryTktStr, Air4TicketFeedTable.ET_TKT_DOC_PH_ADR.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktDocRelations = Air4TicketFeedToEntities<TicketDocRelationship>.Convert(primaryTktStr, Air4TicketFeedTable.ET_TKT_DOC_RLN.ToString(), Constants.Application.TicketFeedFileDelimiter);

                var tktDocWavr = Air4TicketFeedToEntities<TicketDocWavr>.Convert(primaryTktStr, Air4TicketFeedTable.ET_TKT_DOC_WAVR.ToString(), Constants.Application.TicketFeedFileDelimiter);

                var tktDocSupplemental = Air4TicketFeedToEntities<TicketDocSupplemental>.Convert(primaryTktStr, Air4TicketFeedTable.ET_TKT_DOC_SPTML.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktMiscDoc = Air4TicketFeedToEntities<TicketMiscellaneousDoc>.Convert(primaryTktStr, Air4TicketFeedTable.ET_MISC_DOC.ToString(), Constants.Application.TicketFeedFileDelimiter);

                var tktDocPrtLn = Air4TicketFeedToEntities<TicketMiscellaneousDocPrintLine>.Convert(primaryTktStr, Air4TicketFeedTable.ET_MISC_DOC_PRT_LN.ToString(), Constants.Application.TicketFeedFileDelimiter);

                var tktMiscDocAtvy = Air4TicketFeedToEntities<TicketMiscellaneousDocActivity>.Convert(primaryTktStr, Air4TicketFeedTable.ET_MISC_DOC_ATVY.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktMiscDocAtvyDtl = Air4TicketFeedToEntities<TicketMiscDocActivityDetail>.Convert(primaryTktStr, Air4TicketFeedTable.ET_MDOC_ATVY_DTL.ToString(), Constants.Application.TicketFeedFileDelimiter);

                var tktDocBkgRlocr = Air4TicketFeedToEntities<TicketDocBookingRecordLocator>.Convert(primaryTktStr, Air4TicketFeedTable.ET_TKDC_BKG_RLOCR.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktDocPosSrc = Air4TicketFeedToEntities<TicketDocPointOfSaleSource>.Convert(primaryTktStr, Air4TicketFeedTable.ET_TKT_DOC_POS_SRC.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktDocChrg = Air4TicketFeedToEntities<TicketDocumentCharge>.Convert(primaryTktStr, Air4TicketFeedTable.ET_TKT_DOC_CHRG.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktDocActbChrg = Air4TicketFeedToEntities<TicketAccountableCharge>.Convert(primaryTktStr, Air4TicketFeedTable.ET_TKDC_ACTB_CHRG.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktDocCmsn = Air4TicketFeedToEntities<TicketDocCommission>.Convert(primaryTktStr, Air4TicketFeedTable.ET_TKT_DOC_CMSN.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktDocTxFax = Air4TicketFeedToEntities<TicketDocTaxFacsimile>.Convert(primaryTktStr, Air4TicketFeedTable.ET_TKT_DOC_TX_FAX.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktDocFareCaltn = Air4TicketFeedToEntities<TicketDocFareCalculation>.Convert(primaryTktStr, Air4TicketFeedTable.ET_TKDC_FARE_CALTN.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktDocPmtColtn = Air4TicketFeedToEntities<TicketDocPaymentCollection>.Convert(primaryTktStr, Air4TicketFeedTable.ET_TKDC_PMT_COLTN.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktDocPmt = Air4TicketFeedToEntities<TicketDocPayment>.Convert(primaryTktStr, Air4TicketFeedTable.ET_TKT_DOC_PMT.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktDocPmtRmrk = Air4TicketFeedToEntities<TicketDocPaymentRemark>.Convert(primaryTktStr, Air4TicketFeedTable.ET_TKDC_PMT_RMRK.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktDocRtrnsRln = Air4TicketFeedToEntities<TicketDocRefundTransactionRelation>.Convert(primaryTktStr, Air4TicketFeedTable.ET_TKDC_RTRNS_RLN.ToString(), Constants.Application.TicketFeedFileDelimiter);

                var tktDocRspn = Air4TicketFeedToEntities<TicketDocResponse>.Convert(primaryTktStr, Air4TicketFeedTable.ET_TKT_DOC_RSPN.ToString(), Constants.Application.TicketFeedFileDelimiter);

                var tktDocSdsttRmrk = Air4TicketFeedToEntities<TicketDocSuspendStatusRemark>.Convert(primaryTktStr, Air4TicketFeedTable.ET_TKDC_SDSTT_RMRK.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktDocRfndTrns = Air4TicketFeedToEntities<TicketDocRefundTransaction>.Convert(primaryTktStr, Air4TicketFeedTable.ET_TKDC_RFND_TRNS.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktDocRtrnsChrg = Air4TicketFeedToEntities<TicketDocRefundTransactionCharge>.Convert(primaryTktStr, Air4TicketFeedTable.ET_TKDC_RTRNS_CHRG.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktDocRtrnsCmsn = Air4TicketFeedToEntities<TicketDocRefundTransactionCommission>.Convert(primaryTktStr, Air4TicketFeedTable.ET_TKDC_RTRNS_CMSN.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktDocRtrnsPmt = Air4TicketFeedToEntities<TicketDocRefundTransactionPayment>.Convert(primaryTktStr, Air4TicketFeedTable.ET_TKDC_RTRNS_PMT.ToString(), Constants.Application.TicketFeedFileDelimiter);

                var tktDocAgyRsn = Air4TicketFeedToEntities<TicketDocTranasactionAgencyReason>.Convert(primaryTktStr, Air4TicketFeedTable.ET_TDTN_AGY_RSN.ToString(), Constants.Application.TicketFeedFileDelimiter);

                var tktCpns = Air4TicketFeedToEntities<Coupon>.Convert(primaryTktStr, Air4TicketFeedTable.ET_CPN.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktCpnSrcs = Air4TicketFeedToEntities<CouponSource>.Convert(primaryTktStr, Air4TicketFeedTable.ET_CPN_SRC.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktCpnSts = Air4TicketFeedToEntities<CouponState>.Convert(primaryTktStr, Air4TicketFeedTable.ET_CPN_ST.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktCpnStSrcs = Air4TicketFeedToEntities<CouponStSrc>.Convert(primaryTktStr, Air4TicketFeedTable.ET_CPN_ST_SRC.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktCpnCrrRoles = Air4TicketFeedToEntities<CouponCarrierRole>.Convert(primaryTktStr, Air4TicketFeedTable.ET_CPN_CRR_ROLE.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktCpnRlns = Air4TicketFeedToEntities<CouponRelation>.Convert(primaryTktStr, Air4TicketFeedTable.ET_CPN_RLN.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktCpnRlnHsts = Air4TicketFeedToEntities<CouponRelationHistory>.Convert(primaryTktStr, Air4TicketFeedTable.ET_CPN_RLN_HST.ToString(), Constants.Application.TicketFeedFileDelimiter);

                objTicketDto = new TicketDto
                {
                    OptionalRecord = optionalRecord.ToArray(),
                    EventHeader = eventHeader.FirstOrDefault(),
                    Document = primaryTicketDocument,
                    DocumentSource = ticketDocSource.FirstOrDefault(),
                    DocumentElectronic = GetElectronicAddresses(tktDocElectrAddress, tktDocElectrMsg),
                    GeographicAddress = tktDocGphcAddress.ToArray(),
                    PhoneAddress = tktDocPhysAddress.ToArray(),
                    Relationship = tktDocRelations.Where(t => t.CurTktDocNb == primaryTicketDocument.TktDocNb).ToArray(),
                    DocWavr = tktDocWavr.FirstOrDefault(),
                    Supplemental = tktDocSupplemental.ToArray(),
                    MiscellaneousDoc = tktMiscDoc.ToArray(),
                    MiscellaneousDocPrintLine = tktDocPrtLn.FirstOrDefault(),
                    MiscellaneousDocActivity = GetMiscDocActivity(tktMiscDocAtvy, tktMiscDocAtvyDtl),
                    BookingRecordLocator = tktDocBkgRlocr.ToArray(),
                    PointOfSaleSource = tktDocPosSrc.ToArray(),
                    Charge = tktDocChrg.ToArray(),
                    AccountableCharge = tktDocActbChrg.ToArray(),
                    Commission = tktDocCmsn.ToArray(),
                    TaxFacsimile = tktDocTxFax.ToArray(),
                    FareCalculation = tktDocFareCaltn.ToArray(),
                    PaymentCollection = tktDocPmtColtn.ToArray(),
                    Payment = tktDocPmt.ToArray(),
                    PaymentRemark = tktDocPmtRmrk.ToArray(),
                    RefundTransactionRelation = tktDocRtrnsRln.ToArray(),
                    DocResponse = tktDocRspn.ToArray(),
                    SuspendStatusRemark = tktDocSdsttRmrk.ToArray(),
                    RefundTransaction = tktDocRfndTrns.ToArray(),
                    RefundTransactionCharge = tktDocRtrnsChrg.ToArray(),
                    RefundTransactionCommission = tktDocRtrnsCmsn.ToArray(),
                    RefundTransactionPayment = tktDocRtrnsPmt.ToArray(),
                    TranasactionAgencyReason = tktDocAgyRsn.ToArray(),
                    Coupons = GetCouponDetails(tktCpns, tktCpnSrcs, tktCpnSts, tktCpnStSrcs, tktCpnCrrRoles, tktCpnRlns, tktCpnRlnHsts, null, null)
                };
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, "Ticket Feed parsing failed while PopulatePrimaryTicket");
                throw;
            }

            return objTicketDto;
        }

        protected static ExchangeTicket[] PopulateExchangeTickets(string completeExchangeTktStr, ILogger<UpsertTicketCommandHandler> logger)
        {
            if (!string.IsNullOrWhiteSpace(completeExchangeTktStr))
            {
                List<ExchangeTicket> exchangeTickets = new List<ExchangeTicket>();
                int index = 0;
                List<int> indexes = new List<int>();
                string excKeyWord = Air4TicketFeedTable.ET_TKDC_EXCG_RLN.ToString();
                while ((index = completeExchangeTktStr.IndexOf(excKeyWord, index, StringComparison.OrdinalIgnoreCase)) != -1)
                {
                    indexes.Add(index);
                    index += excKeyWord.Length;
                }
                for (int i = 0; i < indexes.Count; i++)
                {
                    var insStr = i + 1 < indexes.Count ? completeExchangeTktStr.Substring(indexes[i], indexes[i + 1] - indexes[i]) : completeExchangeTktStr.Substring(indexes[i]);
                    var excTkt = ExtractExchangeAttributes(insStr, logger);
                    if (excTkt != null)
                    {
                        exchangeTickets.Add(excTkt);
                    }
                }
                return exchangeTickets != null && exchangeTickets.Count > 0 ? exchangeTickets.ToArray() : null;
            }
            else
            {
                return null;
            }
        }

        protected static SecondaryTicket[] PopulateConjunctiveTickets(string completeConjuctiveTktStr, ILogger<UpsertTicketCommandHandler> logger)
        {
            if (!string.IsNullOrWhiteSpace(completeConjuctiveTktStr))
            {
                List<SecondaryTicket> conjunctiveTickets = new List<SecondaryTicket>();
                int index = 0;
                List<int> indexes = new List<int>();
                string conjunctiveKeyWord = Air4TicketFeedTable.ET_TKT_DOC_SEC.ToString();
                while ((index = completeConjuctiveTktStr.IndexOf(conjunctiveKeyWord, index, StringComparison.OrdinalIgnoreCase)) != -1)
                {
                    indexes.Add(index);
                    index += conjunctiveKeyWord.Length;
                }
                for (int i = 0; i < indexes.Count; i++)
                {
                    var insStr = i + 1 < indexes.Count ? completeConjuctiveTktStr.Substring(indexes[i], indexes[i + 1]) : completeConjuctiveTktStr.Substring(indexes[i]);
                    var conjTkt = ExtractSecondaryTicketAttribute(insStr, logger);
                    if (conjTkt != null)
                    {
                        conjunctiveTickets.Add(conjTkt);
                    }
                }
                return conjunctiveTickets != null && conjunctiveTickets.Count > 0 ? conjunctiveTickets.ToArray() : null;
            }
            else
            {
                return null;
            }
        }

        private static ExchangeTicket ExtractExchangeAttributes(string exchangeTktStr, ILogger<UpsertTicketCommandHandler> logger)
        {
            try
            {
                var tktDocExcgRlns = Air4TicketFeedToEntities<TicketDocumentExchangeRelation>.Convert(exchangeTktStr, Air4TicketFeedTable.ET_TKDC_EXCG_RLN.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktDocExrsdlTxs = Air4TicketFeedToEntities<TicketDocumentExchangeResidualTax>.Convert(exchangeTktStr, Air4TicketFeedTable.ET_TKDC_EXRSDL_TX.ToString(), Constants.Application.TicketFeedFileDelimiter);

                var tktCpns = Air4TicketFeedToEntities<Coupon>.Convert(exchangeTktStr, Air4TicketFeedTable.ET_CPN.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktCpnSrcs = Air4TicketFeedToEntities<CouponSource>.Convert(exchangeTktStr, Air4TicketFeedTable.ET_CPN_SRC.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktCpnSts = Air4TicketFeedToEntities<CouponState>.Convert(exchangeTktStr, Air4TicketFeedTable.ET_CPN_ST.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktCpnStSrcs = Air4TicketFeedToEntities<CouponStSrc>.Convert(exchangeTktStr, Air4TicketFeedTable.ET_CPN_ST_SRC.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktCpnExrlns = Air4TicketFeedToEntities<CouponExchangeRelation>.Convert(exchangeTktStr, Air4TicketFeedTable.ET_TKDC_CPN_EXRLN.ToString(), Constants.Application.TicketFeedFileDelimiter);

                ExchangeTicket excTicketObj = new ExchangeTicket
                {
                    EtTkDcExcgRln = tktDocExcgRlns != null ? tktDocExcgRlns.ToArray() : null,
                    EtTkDcExRsdlTx = tktDocExrsdlTxs != null ? tktDocExrsdlTxs.ToArray() : null,
                    CouponOp = GetCouponDetails(tktCpns, tktCpnSrcs, tktCpnSts, tktCpnStSrcs, null, null, null, tktCpnExrlns, null)
                };
                return excTicketObj;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ticket Feed parsing failed while PopulatePrimaryExchangeTickets");
                throw;
            }
        }

        protected static SecondaryTicket ExtractSecondaryTicketAttribute(string secondaryTktStr, ILogger<UpsertTicketCommandHandler> logger)
        {
            try
            {
                var isSecondaryExcTkt = false;
                var exchangeTktStr = string.Empty;
                int eTktDocExcIndx = secondaryTktStr.IndexOf(Air4TicketFeedTable.ET_TKDC_EXCG_RLN.ToString());

                if (eTktDocExcIndx > -1)
                {
                    isSecondaryExcTkt = true;
                }
                if (isSecondaryExcTkt)
                {
                    exchangeTktStr = secondaryTktStr.Substring(eTktDocExcIndx);
                    secondaryTktStr = secondaryTktStr.Substring(0, eTktDocExcIndx);
                }
                var tktDocSec = Air4TicketFeedToEntities<TicketDocument>.Convert(secondaryTktStr,Air4TicketFeedTable.ET_TKT_DOC_SEC.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktDocSrcs = Air4TicketFeedToEntities<TicketDocumentSource>.Convert(secondaryTktStr,Air4TicketFeedTable.ET_TKT_DOC_SRC.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktDocRlns = Air4TicketFeedToEntities<TicketDocRelationship>.Convert(secondaryTktStr,Air4TicketFeedTable.ET_TKT_DOC_RLN.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktDocRtrnsRln = Air4TicketFeedToEntities<TicketDocRefundTransactionRelation>.Convert(secondaryTktStr,Air4TicketFeedTable.ET_TKDC_RTRNS_RLN.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktDocRfndTrns = Air4TicketFeedToEntities<TicketDocRefundTransaction>.Convert(secondaryTktStr,Air4TicketFeedTable.ET_TKDC_RFND_TRNS.ToString(), Constants.Application.TicketFeedFileDelimiter);

                var tktCpns = Air4TicketFeedToEntities<Coupon>.Convert(secondaryTktStr,Air4TicketFeedTable.ET_CPN.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktCpnSrcs = Air4TicketFeedToEntities<CouponSource>.Convert(secondaryTktStr,Air4TicketFeedTable.ET_CPN_SRC.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktCpnSts = Air4TicketFeedToEntities<CouponState>.Convert(secondaryTktStr,Air4TicketFeedTable.ET_CPN_ST.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktCpnStSrcs = Air4TicketFeedToEntities<CouponStSrc>.Convert(secondaryTktStr,Air4TicketFeedTable.ET_CPN_ST_SRC.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktCpnCrrRoles = Air4TicketFeedToEntities<CouponCarrierRole>.Convert(secondaryTktStr,Air4TicketFeedTable.ET_CPN_CRR_ROLE.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktCpnRlns = Air4TicketFeedToEntities<CouponRelation>.Convert(secondaryTktStr,Air4TicketFeedTable.ET_CPN_RLN.ToString(), Constants.Application.TicketFeedFileDelimiter);
                var tktCpnRlnHsts = Air4TicketFeedToEntities<CouponRelationHistory>.Convert(secondaryTktStr,Air4TicketFeedTable.ET_CPN_RLN_HST.ToString(), Constants.Application.TicketFeedFileDelimiter);

                var secondaryTkt = new SecondaryTicket
                {
                    EtTktDocSec = tktDocSec.FirstOrDefault(),
                    EtTktDocSrc = tktDocSrcs.FirstOrDefault(),
                    EtTktDocRln = tktDocRlns.ToArray(),
                    EtTkDcRTrnsRln = tktDocRtrnsRln.ToArray(),
                    EtTkDcRfndTrns = tktDocRfndTrns.ToArray(),
                    Coupon = GetCouponDetails(tktCpns, tktCpnSrcs, tktCpnSts, tktCpnStSrcs, tktCpnCrrRoles, tktCpnRlns, tktCpnRlnHsts, null, null)
                };
                if (isSecondaryExcTkt)
                {
                    secondaryTkt.SecondaryExchange = PopulateExchangeTickets(exchangeTktStr, logger);
                }
                return secondaryTkt;
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, "Ticket Feed parsing failed while PopulateSecondaryTickets");
                throw;
            }
        }

        protected static TicketDocumentElectronic[] GetElectronicAddresses(List<ElectronicAddress> tktDocElectrAddress, List<ElectronicMessage> tktDocElectrMsg)
        {
            if (tktDocElectrAddress != null && tktDocElectrAddress.Count > 0)
            {
                var lstAddress = tktDocElectrAddress.Select(t => t.TktDocEAdrSqNb).Distinct();
                var docElctrns = new List<TicketDocumentElectronic>();
                foreach (var item in lstAddress)
                {
                    docElctrns.Add(new TicketDocumentElectronic
                    {
                        ElectronicAddresses = tktDocElectrAddress.Where(a => a.TktDocEAdrSqNb == item).ToArray(),
                        ElectronicMessages = tktDocElectrMsg.Where(a => a.TktDocEAdrSqNb == item).ToArray()
                    });
                }
                return docElctrns.ToArray();
            }
            return null;
        }

        protected static TicketMiscellaneousDocActivityOp[] GetMiscDocActivity(List<TicketMiscellaneousDocActivity> tktMiscDocAtvy, List<TicketMiscDocActivityDetail> tktMiscDocAtvyDtl)
        {
            if (tktMiscDocAtvy != null && tktMiscDocAtvy.Count > 0)
            {
                var lstActivites = tktMiscDocAtvy.Select(t => t.MDocAcyTySqNb).Distinct();
                var docElctrns = new List<TicketMiscellaneousDocActivityOp>();
                foreach (var item in lstActivites)
                {
                    docElctrns.Add(new TicketMiscellaneousDocActivityOp
                    {
                        MiscDocActivity = tktMiscDocAtvy.Where(a => a.MDocAcyTySqNb == item).ToArray(),
                        MiscDocActivityDetail = tktMiscDocAtvyDtl.Where(a => a.MDocAcyTySqNb == item).ToArray()
                    });
                }
                return docElctrns.ToArray();
            }
            return null;
        }

        protected static TicketCoupon[] GetCouponDetails(List<Coupon> cpns, List<CouponSource> cpnSrcs, List<CouponState> cpnSts, List<CouponStSrc> cpnStSrcs, List<CouponCarrierRole> cpnCrrs, List<CouponRelation> cpnRlns, List<CouponRelationHistory> cpnRlnHsts, List<CouponExchangeRelation> cpnExRlns, List<ReissueCouponFlownItinerary> reissCpnFlwnItins)
        {
            if (cpns != null && cpns.Count > 0)
            {
                var tktCpns = new List<TicketCoupon>();
                var distCpnNbrs = cpns.Select(c => c.CpnNb).Distinct();
                foreach (var item in distCpnNbrs)
                {
                    var cpnDetail = new TicketCoupon
                    {
                        EtCpn = cpns != null && cpns.Any(d => d.CpnNb == item) ? cpns.Where(d => d.CpnNb == item).ToArray() : null,
                        EtCpnSrc = cpnSrcs != null && cpnSrcs.Any(d => d.CpnNb == item) ? cpnSrcs?.Where(d => d.CpnNb == item).FirstOrDefault() : null,
                        EtCpnSt = cpnSts != null && cpnSts.Any(d => d.CpnNb == item) ? cpnSts.Where(d => d.CpnNb == item).ToArray() : null,
                        EtCpnStSrc = cpnStSrcs != null && cpnStSrcs.Any(d => d.CpnNb == item) ? cpnStSrcs.Where(d => d.CpnNb == item).ToArray() : null,
                        EtCpnCrrRole = cpnCrrs != null && cpnCrrs.Any(d => d.CpnNb == item) ? cpnCrrs.Where(d => d.CpnNb == item).ToArray() : null,
                        EtCpnRln = cpnRlns != null && cpnRlns.Any(d => d.FltCpnNb == item) ? cpnRlns.Where(d => d.FltCpnNb == item).ToArray() : null,
                        EtCpnRlnHst = cpnRlnHsts != null && cpnRlnHsts.Any(d => d.FltCpnNb == item) ? cpnRlnHsts.Where(d => d.FltCpnNb == item).ToArray() : null,
                        EtTkDcCpnEXRln = cpnExRlns != null && cpnExRlns.Any(d => d.ExcgdCpnNb == item) ? cpnExRlns.Where(d => d.ExcgdCpnNb == item).ToArray() : null,
                        EtReissCpnFlwnItin = reissCpnFlwnItins != null && reissCpnFlwnItins.Any(d => d.FlwnCpnNb == item) ? reissCpnFlwnItins.Where(d => d.FlwnCpnNb == item).ToArray() : null
                    };
                    tktCpns.Add(cpnDetail);
                }
                return tktCpns.ToArray();
            }
            return null;
        }

    }
}