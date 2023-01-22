using System.Collections.Generic;

namespace Growth.Voronoi
{
    public interface IVoronoi : IPolyhedronSet
    {
        IEnumerable<Face> Faces { get; }
        IEnumerable<Vec3> Verts { get; }
        IDelaunay Delaunay { get; }
        float Tolerance { get; }
    }
}
