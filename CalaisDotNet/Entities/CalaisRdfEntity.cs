using System.Collections.Generic;
using Calais.Interfaces;

namespace Calais.Entity
{
    public class CalaisRdfEntity : ICalaisRdfEntity
    {
        #region ICalaisRdfEntity Members

        public string Id { get; set; }
        public IEnumerable<CalaisRdfResourceInstance> Instances { get; set; }

        #endregion

        public CalaisRdfEntityType EntityType { get; set; }
        public CalaisRdfEntitySubType EntitySubType { get; set; }
        public string Value { get; set; }
        public string SubValue { get; set; }

        public CalaisRdfEntity()
        {
            EntitySubType = CalaisRdfEntitySubType.None;
            SubValue = string.Empty;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            if(string.IsNullOrEmpty(SubValue))
            {
                return EntityType + ": " + Value + "; ";    
            }

            return EntityType + ": " + Value + " ( " + StringEnum.GetString(EntitySubType) + " : " + SubValue.CapitalizeAll() + " )";
        }
    }
}