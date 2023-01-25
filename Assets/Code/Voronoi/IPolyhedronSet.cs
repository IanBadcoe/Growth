using System.Collections.Generic;

namespace Growth.Voronoi
{
    public interface IPolyhedronSet
    {
        IEnumerable<IVPolyhedron> Polyhedrons { get; }
    }
}
