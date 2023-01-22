using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Growth.Voronoi
{
    class Delaunay : IDelaunay
    {
        public Delaunay(float tolerance)
        {
            TetsRW = new List<DTetrahedron>();
            Tolerance = tolerance;
            Tags = new Dictionary<Vec3, List<String>>();
        }

        public Delaunay(Delaunay old)
        {
            // this works because Vec3, CircumSphere and DTetrahedron are all immutable, so by cloning the current state
            // of old we are good to be valid in that state forever...
            TetsRW = TetsRW;
            Tolerance = old.Tolerance;
            Tags = old.Tags;
        }

        List<DTetrahedron> TetsRW { get; }

        Dictionary<Vec3, List<String>> Tags;

        #region IDelaunay
        public IReadOnlyList<DTetrahedron> Tets { get { return TetsRW; } }
        public IEnumerable<Vec3> Verts { get { return Tets.SelectMany(x => x.Verts).Distinct(); } }
        public IDelaunay Clone()
        {
            return new Delaunay(this);
        }
        public List<String> GetVertTags(Vec3 v)
        {
            List<String> tags;

            if (!Tags.TryGetValue(v, out tags))
            {
                return new List<String>();
            }

            return tags;
        }
        public float Tolerance { get; }
        #endregion

        #region IPolyhedronSet
        public IReadOnlyList<IVPolyhedron> Polyhedrons => Tets.Select(tet => tet.ToPolyhedron()).ToList();
        #endregion

        public void AddTet(DTetrahedron tet)
        {
            TetsRW.Add(tet);
        }

        public bool Validate()
        {
            foreach (var tet in Tets)
            {
                if (!tet.Valid)
                {
                    return false;
                }

                foreach (var p in Verts)
                {
                    if (tet.Sphere.Contains(p, Tolerance))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void AddVert(Vec3 v)
        {
            // SPATIAL SEARCH
            List<DTetrahedron> bad_tets = Tets.Where(tet => tet.Sphere.Contains(v, Tolerance)).ToList();

            TriangularPolyhedron pt = new TriangularPolyhedron(bad_tets);

            foreach (var tet in bad_tets)
            {
                TetsRW.Remove(tet);
            }

            foreach (var tri in pt.TriFaces)
            {
                var tet = new DTetrahedron(v, tri.V1, tri.V2, tri.V3);

                AddTet(tet);
            }
        }

        public void InitialiseWithVerts(Vec3[] verts)
        {
            VBounds b = new VBounds();

            foreach (var p in verts)
            {
                b.Encapsulate(p);
            }

            // build an encapsulating tetrahedron, using whichever axis is longest, and padding by 1 on each dimension
            Vec3 c0 = new Vec3(b.Min.X - 1, b.Min.Y - 1, b.Min.Y - 1);
            float size = Mathf.Max(b.Size.X, b.Size.Y, b.Size.Z) + 2;

            // a right-angled prism which contains that box has its corners on the axes at 3x the
            // box dimensions
            var c1 = c0 + new Vec3(size * 3, 0, 0);
            var c2 = c0 + new Vec3(0, size * 3, 0);
            var c3 = c0 + new Vec3(0, 0, size * 3);

            var bounding_tet = new DTetrahedron(c0, c1, c2, c3);
            //System.Diagnostics.Debug.Assert(bounding_tet.Valid);

            AddTet(bounding_tet);

            //System.Diagnostics.Debug.Assert(Validate());

            foreach (var v in verts)
            {
                AddVert(v);

                //UnityEngine.Debug.Assert(Validate());
            }

            // remove the encapsulating verts we started with, and any associated tets
            var encapsulating_vert_tets = Tets.Where(x => x.UsesVert(c0) || x.UsesVert(c1) || x.UsesVert(c2) || x.UsesVert(c3)).ToList();

            TetsRW.RemoveAll(tet => encapsulating_vert_tets.Contains(tet));

            UnityEngine.Debug.Assert(!Verts.Contains(c0));
            UnityEngine.Debug.Assert(!Verts.Contains(c1));
            UnityEngine.Debug.Assert(!Verts.Contains(c2));
            UnityEngine.Debug.Assert(!Verts.Contains(c3));
        }

        public void TagVert(Vec3 v, string tag)
        {
            List<String> tags;

            if (!Tags.TryGetValue(v, out tags))
            {
                tags = new List<String>();
                Tags[v] = tags;
            }

            tags.Add(tag);
        }
    }
}
