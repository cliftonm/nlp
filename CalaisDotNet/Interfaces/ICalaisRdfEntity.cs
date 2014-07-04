using System.Collections.Generic;
using Calais.Entity;

namespace Calais.Interfaces
{
    public interface ICalaisRdfEntity
    {
        string Id { get; set; }
        IEnumerable<CalaisRdfResourceInstance> Instances { get; set; }
    }
}