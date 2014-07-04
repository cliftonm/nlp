using System.Collections.Generic;
using System.Text;
using Calais.Interfaces;

namespace Calais.Entity
{
    public class CalaisRdfRelationship : ICalaisRdfEntity
    {
        #region ICalaisRdfEntity Members

        public string Id { get; set; }
        public IEnumerable<CalaisRdfResourceInstance> Instances { get; set; }

        #endregion

        public CalaisRdfRelationshipType RelationshipType { get; set; }
        public IDictionary<string, string> RelationshipDetails { get; set; }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            
            foreach (var detail in RelationshipDetails)
            {
                sb.Append(detail.Key.CapitalizeAll() + ": " + detail.Value + "; ");    
            }

            return sb.ToString();
        }
    }
}