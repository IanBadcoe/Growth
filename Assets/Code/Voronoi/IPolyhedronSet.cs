using System.Collections.Generic;

namespace Growth.Voronoi
{
    public interface IPolyhedronSet
    {
        IReadOnlyList<IVPolyhedron> Polyhedrons { get; }
    }
}
