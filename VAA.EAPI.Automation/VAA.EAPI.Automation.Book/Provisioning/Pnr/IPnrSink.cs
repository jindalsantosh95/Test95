namespace VAA.EAPI.Automation.Book.Provisioning.Pnr
{
    public interface IPnrSink
    {
        Task WriteAsync(CreatedPnr pnr);
    }
}
