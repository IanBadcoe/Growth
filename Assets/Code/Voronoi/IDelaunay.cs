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
        public float Tolerance { get; }
        IVPolyhedron OuterHull();
    }
}
