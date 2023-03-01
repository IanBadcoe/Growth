using System.Collections.Generic;

namespace Growth.Voronoi
{
    public interface IPolyhedronSet
    {
        IEnumerable<IPolyhedron> Polyhedrons { get; }
    }
}
