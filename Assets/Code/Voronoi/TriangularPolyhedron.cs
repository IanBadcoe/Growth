using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Growth.Voronoi
{
    // a polyhedron made of triangles
    [DebuggerDisplay("Trianges: {Triangles.Count}")]
    public class TriangularPolyhedron
    {
        [DebuggerDisplay("({I1},{I2},{I3})")]
        public struct TriIndex : IEquatable<TriIndex>
        {
            public TriIndex(int i1, int i2, int i3)
            {
                I1 = i1;
                I2 = i2;
                I3 = i3;
            }

            public readonly int I1;
            public readonly int I2;
            public readonly int I3;

            public IEnumerable<int> Indices
            {
                get
                {
                    yield return I1;
                    yield return I2;
                    yield return I3;
                }
            }

            public bool Equals(TriIndex other)
            {
                return Indices.Contains(other.I1)
                    && Indices.Contains(other.I2)
                    && Indices.Contains(other.I3);
            }
        }

        public readonly List<TriIndex> Triangles = new List<TriIndex>();
        
        public IEnumerable<Triangle> TriFaces
        {
            get
            {
                return Triangles.Select(f => new Triangle(Verts[f.I1], Verts[f.I2], Verts[f.I3]));
            }
        }
        
        public readonly List<Vec3> Verts = new List<Vec3>();

        public TriangularPolyhedron(List<DTetrahedron> tets)
        {
            foreach(var tet in tets)
            {
                AddTetrahedron(tet);
            }
        }

        public void AddTetrahedron(DTetrahedron tet)
        {
            foreach (var f in tet.Triangles)
            {
                int[] vert_idxs = f.Verts.Select(vert_idxs => AddFindVert(vert_idxs)).ToArray();

                TriIndex tri = new TriIndex(vert_idxs[0], vert_idxs[1], vert_idxs[2]);

                // some significant CPU in this search, could store indexed by hash?
                // or indexed by one very then brute-force the others?
                bool find = Triangles.Contains(tri);

                // we build ourselves from tets, when a tet we already saw contains the same face
                // as one being added, it means that is becoming an internal face between two tets
                // and we do not want it...
                if (find)
                {
                    Triangles.Remove(tri);
                }
                else
                {
                    Triangles.Add(tri);
                }
            }
        }

        public int AddFindVert(Vec3 v)
        {
            int idx = Verts.IndexOf(v);

            if (idx != -1)
            {
                return idx;
            }

            Verts.Add(v);

            return Verts.Count - 1;
        }
    }
}
