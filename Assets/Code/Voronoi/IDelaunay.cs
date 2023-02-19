using System;
using System.Collections.Generic;

namespace Growth.Voronoi
{
    public interface IDelaunay : IPolyhedronSet
    {
        public IDelaunay Clone();
        public IEnumerable<DTetrahedron> Tets { get; }
        public IEnumerable<DTetrahedron> TetsForVert(Vec3 vert);
        public IEnumerable<Vec3> Verts { get; }
        public List<String> GetVertTags(Vec3 v);
        public float Tolerance { get; }
        public IReadOnlyList<Vec3> BoundingPoints { get; }
    }
}
