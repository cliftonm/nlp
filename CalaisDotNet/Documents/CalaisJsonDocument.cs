using Calais.Interfaces;

namespace Calais
{
    public class CalaisJsonDocument : ICalaisDocument
    {
        #region ICalaisDocument Members

        public string RawOutput { get; set; }

        public void ProcessResponse(string response)
        {
            RawOutput = response;
        }

        #endregion
    }
}