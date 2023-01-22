using System.Collections.Generic;

namespace Growth.Voronoi
{
    public interface IVPolyhedron
    {
        IReadOnlyList<Face> Faces { get; }
        IReadOnlyList<Vec3> Verts { get; }
        Vec3 Centre { get; }
    }
}
