using System;
using System.Collections.Generic;

namespace Growth.Voronoi
{
    public interface IDelaunay : IPolyhedronSet
    {
        public IDelaunay Clone();
        public IReadOnlyList<DTetrahedron> Tets { get; }
        public IEnumerable<Vec3> Verts { get; }
        public List<String> GetVertTags(Vec3 v);
        public float Tolerance { get; }
    }
}
