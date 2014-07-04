using Calais.Interfaces;

namespace Calais
{
    public class CalaisMicroFormatsDocument : ICalaisDocument
    {
        #region ICalaisDocument Members

        public string RawOutput { get; set;}

        public void ProcessResponse(string response)
        {
            RawOutput = response;
        }

        #endregion
    }
}