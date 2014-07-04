namespace Calais.Interfaces
{
    public interface ICalaisDocument
    {
        string RawOutput { get; set; }
        void ProcessResponse(string response);
    }
}
