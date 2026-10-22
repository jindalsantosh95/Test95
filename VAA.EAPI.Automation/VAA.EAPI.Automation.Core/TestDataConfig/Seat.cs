using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using System.ComponentModel;
using System.Reflection;

public class Seat
{
    public string  SeatProductName { get; set; }
    public CabinName CabinName { get; set; }
    public string SectorId { get; set; }
    public string SeatOfferId { get; set; }
    public string[] SectorInbound {  get; set; }
    public string[] SectorOutbound { get; set; }
    public string SeatNumber {  get; set; }
    public double Amount { get; set; }
    public string Currency { get; set; }
}