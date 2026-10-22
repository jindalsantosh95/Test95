namespace VAA.EAPI.Automation.Core.TestDataConfig;

public class Book
{
    public Payment Payment { get; set; }
    public Order Order { get; set; }
    public Seat Seat { get; set; }
    public Cost Cost { get; set; }
    public Contact Contact { get; set; }
    public Passenger Passenger { get; set; }
    public CoBrand CoBrand { get; set; }

    public Book()
    {
        Payment = new Payment();
        Order = new Order();
        Seat = new Seat();
        Cost = new Cost();
        Contact = new Contact();
        Passenger = new Passenger();
        CoBrand = new CoBrand();
    }
}