using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Virgin.Models.Products.Baggage;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita.Common
{
    public class BaggageAllowanceMapper
    {
        public static async Task<List<DimensionAllowance>> GetPieceDimensionAllowance(ICachedPieceAllowance pieceAllowance)
        {
            var maxDimensionAllowanceCM = new DimensionAllowance();
            var maxDimensionAllowanceIN = new DimensionAllowance();

            maxDimensionAllowanceCM.LengthUnitOfMeasurement = LengthUnitCodeType.CMT;
            maxDimensionAllowanceIN.LengthUnitOfMeasurement = LengthUnitCodeType.INH;

            var checkInAllowance = await pieceAllowance.GetCachedPieceAllowanceInformation("PieceAllowance");

            if (checkInAllowance != null)
            {
                var combinedDimensionList = new List<DimensionAllowance>
                {
                   GetCheckedInDimensionAllowance(maxDimensionAllowanceCM,checkInAllowance.Where(x=>x.UnitCode==Constants.Application.Centimeter)),
                   GetCheckedInDimensionAllowance(maxDimensionAllowanceIN,checkInAllowance.Where(x=>x.UnitCode==Constants.Application.Inch)),
                };

                return combinedDimensionList.First() != null && Convert.ToDecimal(combinedDimensionList.First().LengthMeasure) != 0
                    ? combinedDimensionList
                    : null;
            }
            return null;
        }

        public static List<DimensionAllowance> GetPieceDimensionAllowanceCarryOn(BaggageAllowances bagRulesAndAllowances, IEnumerable<CabinBaggageEntity> cabinBagAllowance, List<WeightAllowance> weightAllowance)
        {
            var maxDimensionAllowanceIN = new DimensionAllowance();
            var maxDimensionAllowanceCM = new DimensionAllowance();

            var cabinBaggages = cabinBagAllowance.Where(x => x.TotalQuantity == bagRulesAndAllowances?.FreeBaggageAllowance?.AllowedBagCnt.ToString());

            if (cabinBaggages != null)
            {
                maxDimensionAllowanceCM.LengthUnitOfMeasurement = LengthUnitCodeType.CMT;
                maxDimensionAllowanceIN.LengthUnitOfMeasurement = LengthUnitCodeType.INH;

                foreach (var cb in weightAllowance)
                {
                    if (cb.WeightUnitOfMeasurement == WeightUnitCodeType.KGM)
                    {
                        maxDimensionAllowanceCM.LinearMeasure = cb.MaximumWeightMeasure;
                    }
                    if (cb.WeightUnitOfMeasurement == WeightUnitCodeType.LBR)
                    {
                        maxDimensionAllowanceIN.LinearMeasure = cb.MaximumWeightMeasure;
                    }
                }
                var combinedDimensionList = new List<DimensionAllowance>
                {
                    GetCarryOnDimensionAllowance(maxDimensionAllowanceCM,cabinBagAllowance.Where(x=>x.UnitCode==Constants.Application.Centimeter)),
                    GetCarryOnDimensionAllowance(maxDimensionAllowanceIN,cabinBagAllowance.Where(x=>x.UnitCode==Constants.Application.Inch))
                };
                if (combinedDimensionList.FirstOrDefault() != null && Convert.ToDecimal(combinedDimensionList.FirstOrDefault().LengthMeasure) != 0)
                {
                    return combinedDimensionList;
                }
            }
            return null;
        }

        public static List<WeightAllowance> GetWeightAllowance(BaggageAllowances bagRulesAndAllowances)
        {
            var weightAllowanceListKG = new List<WeightAllowance>();
            var weightAllowanceListLB = new List<WeightAllowance>();
            var combinedWeightList = new List<WeightAllowance>();

            if (bagRulesAndAllowances?.FreeBaggageAllowance?.BagDescriptors != null && bagRulesAndAllowances?.FreeBaggageAllowance?.BagDescriptors.Count > 0)
            {
                foreach (var bagDesc in bagRulesAndAllowances?.FreeBaggageAllowance?.BagDescriptors)
                {
                    
                    if (bagDesc?.BagDescriptorDesc?.Any() != true && bagRulesAndAllowances?.FreeBaggageAllowance?.WeightInKilos > 0)
                    {
                        int weightAllowanceInKG = bagRulesAndAllowances.FreeBaggageAllowance.WeightInKilos;

                        weightAllowanceListKG.Add(new WeightAllowance
                        {
                            WeightUnitOfMeasurement = WeightUnitCodeType.KGM,
                            MaximumWeightMeasure = weightAllowanceInKG.ToString()
                        });

                        weightAllowanceListLB.Add(new WeightAllowance
                        {
                            WeightUnitOfMeasurement = WeightUnitCodeType.LBR,
                            MaximumWeightMeasure = Math.Ceiling(weightAllowanceInKG * 2.204).ToString()
                        });

                    }

                    foreach (var desc in bagDesc?.BagDescriptorDesc ?? Enumerable.Empty<string>())
                    {
                        string weightDesc = desc.Substring(desc.IndexOf("Up to") + 5);
                        var strSplit = weightDesc.Split('/');
                        if (strSplit != null)
                        {
                            foreach (var s in strSplit)
                            {
                                var wUnit = s?.Trim()?.Split(' ');
                                if (wUnit != null && wUnit.Length == 2)
                                {
                                    if (wUnit[1] != null && wUnit[1] != "")
                                    {
                                        if (wUnit[1].ToUpper() == "KG")
                                        {
                                            weightAllowanceListKG.Add(new WeightAllowance
                                            {
                                                WeightUnitOfMeasurement = WeightUnitCodeType.KGM,
                                                MaximumWeightMeasure = wUnit[0]
                                            });
                                        }

                                        if (wUnit[1].ToUpper() == "LB")
                                        {
                                            weightAllowanceListLB.Add(new WeightAllowance
                                            {
                                                WeightUnitOfMeasurement = WeightUnitCodeType.LBR,
                                                MaximumWeightMeasure = wUnit[0]
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }

                }

            }

            combinedWeightList.AddRange(weightAllowanceListKG);
            combinedWeightList.AddRange(weightAllowanceListLB);

            return combinedWeightList;
        }

        private static DimensionAllowance GetCarryOnDimensionAllowance(DimensionAllowance dimensionAllowance, IEnumerable<CabinBaggageEntity> cabinBaggages)
        {
            foreach (var dimension in cabinBaggages)
            {
                if (dimension.DimensionCategory.Is(Constants.Application.DimensionCategoryLength))
                {
                    dimensionAllowance.LengthMeasure = dimension.Value;
                }
                else if (dimension.DimensionCategory.Is(Constants.Application.DimensionCategoryHeight))
                {
                    dimensionAllowance.HeightMeasure = dimension.Value;
                }
                else if (dimension.DimensionCategory.Is(Constants.Application.DimensionCategoryWidth))
                {
                    dimensionAllowance.WidthMeasure = dimension.Value;
                }
                else if (dimension.DimensionCategory.Is(Constants.Application.DimensionCategoryAllLinear))
                {
                    dimensionAllowance.LinearMeasure = dimension.Value;
                }
            }
            return dimensionAllowance;
        }

        private static DimensionAllowance GetCheckedInDimensionAllowance(DimensionAllowance dimensionAllowance, IEnumerable<PieceAllowanceTableEntity> pieceAllowance)
        {
            foreach (var dimension in pieceAllowance)
            {
                if (dimension.DimensionCategory.Is(Constants.Application.DimensionCategoryLength))
                {
                    dimensionAllowance.LengthMeasure = dimension.MaxMeasure;
                }
                else if (dimension.DimensionCategory.Is(Constants.Application.DimensionCategoryHeight))
                {
                    dimensionAllowance.HeightMeasure = dimension.MaxMeasure;
                }
                else if (dimension.DimensionCategory.Is(Constants.Application.DimensionCategoryWidth))
                {
                    dimensionAllowance.WidthMeasure = dimension.MaxMeasure;
                }
                else if (dimension.DimensionCategory.Is(Constants.Application.DimensionCategoryAllLinear))
                {
                    dimensionAllowance.LinearMeasure = dimension.MaxMeasure;
                }
            }
            return dimensionAllowance;
        }
    }
}

