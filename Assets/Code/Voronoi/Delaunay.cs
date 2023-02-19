//#define PROFILE_ON

using Growth.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Growth.Voronoi
{
    public class Delaunay : IDelaunay
    {
        public Delaunay(float tolerance)
        {
            TetsRW = new HashSet<DTetrahedron>();
            VecToTet = new Dictionary<Vec3, List<DTetrahedron>>();

            Tolerance = tolerance;
            Tags = new Dictionary<Vec3, List<String>>();
        }

        public Delaunay(Delaunay old)
        {
            // this works because Vec3, CircumSphere and DTetrahedron are all immutable, so by cloning the current state
            // of old we are good to be valid in that state forever...
            TetsRW = new HashSet<DTetrahedron>(old.TetsRW);
            VecToTet = new Dictionary<Vec3, List<DTetrahedron>>(old.VecToTet);
            Tolerance = old.Tolerance;
            Tags = old.Tags;
        }

        // HashSet keys off Reference identity, not any sort of geometry,
        // because all we need to do is find items we already have and remove them...
        HashSet<DTetrahedron> TetsRW { get; }
        Dictionary<Vec3, List<DTetrahedron>> VecToTet { get; }

        Dictionary<Vec3, List<String>> Tags;

        #region IDelaunay
        public IEnumerable<DTetrahedron> Tets => TetsRW;
        public IEnumerable<DTetrahedron> TetsForVert(Vec3 vert)
        {
            if (VecToTet.ContainsKey(vert))
            {
                foreach(var tet in VecToTet[vert])
                {
                    yield return tet;
                }
            }
        }
        public IEnumerable<Vec3> Verts => VecToTet.Keys;
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
        public IReadOnlyList<Vec3> BoundingPoints { get; set; }

        #endregion

        #region IPolyhedronSet
        public IEnumerable<IVPolyhedron> Polyhedrons => Tets.Select(tet => tet.ToPolyhedron());
        #endregion

        public void AddTet(DTetrahedron tet)
        {
            TetsRW.Add(tet);

            foreach(var v in tet.Verts)
            {
                List<DTetrahedron> tets;
                
                if (!VecToTet.TryGetValue(v, out tets))
                {
                    tets = new List<DTetrahedron>();
                    VecToTet[v] = tets;
                }

                tets.Add(tet);
            }
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
                    if (!tet.Verts.Contains(p))
                    {
                        if (tet.Sphere.Contains(p, Tolerance))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public void AddVert(Vec3 vert)
        {
            PoorMansProfiler.Start("AddVert");
            PoorMansProfiler.Start("Find Tets");
            // SPATIAL SEARCH
            List<DTetrahedron> bad_tets = Tets.Where(tet => tet.Sphere.Contains(vert, 0)).ToList();
            PoorMansProfiler.End("Find Tets");

            PoorMansProfiler.Start("Build Triangular Poly");
            TriangularPolyhedron pt = new TriangularPolyhedron(bad_tets);
            PoorMansProfiler.End("Build Triangular Poly");

            PoorMansProfiler.Start("Remove Tets");
            foreach (var tet in bad_tets)
            {
                RemoveTetInner(tet);
            }
            PoorMansProfiler.End("Remove Tets");

            PoorMansProfiler.Start("Add Tets");
            foreach (var tri in pt.TriFaces)
            {
                PoorMansProfiler.Start("Tet Ctor");
                var tet = new DTetrahedron(vert, tri.V1, tri.V2, tri.V3);
                PoorMansProfiler.End("Tet Ctor");

                PoorMansProfiler.Start("AddTet");
                AddTet(tet);
                PoorMansProfiler.End("AddTet");
            }
            PoorMansProfiler.End("Add Tets");
            PoorMansProfiler.End("AddVert");

            MyAssert.IsTrue(Validate(), "Invalid");
        }

        private void RemoveTetInner(DTetrahedron tet)
        {
            TetsRW.Remove(tet);

            foreach (var v in tet.Verts)
            {
                if (VecToTet[v].Count == 1)
                {
                    VecToTet.Remove(v);
                }
                else
                {
                    VecToTet[v].Remove(tet);
                }
            }
        }

        public void InitialiseWithTet(Vec3[] verts)
        {
            var bounding_tet = new DTetrahedron(verts[0], verts[1], verts[2], verts[3]);

            AddTet(bounding_tet);

            BoundingPoints = new List<Vec3>(verts);
        }

        public void InitialiseWithVerts(Vec3[] verts)
        {
            VBounds b = new VBounds();

            foreach (var p in verts)
            {
                b = b.Encapsulating(p);
            }

            // looks like in Unit terms, points on the edge occasionally are considered outside the bound?
            b = b.ExpandedBy(0.001f);

            foreach (var c in verts)
            {
                MyAssert.IsTrue(b.Contains(c), "Constructed vert outside bounds");
            }

            // build an encapsulating tetrahedron, using whichever axis is longest, and padding by 1 on each dimension
            Vec3 c0 = new Vec3(b.Min.X - 1, b.Min.Y - 1, b.Min.Z - 1);
            float size = Mathf.Max(b.Size.X, b.Size.Y, b.Size.Z) + 2;

            // a right-angled prism which contains that box has its corners on the axes at 3x the
            // box dimensions
            var c1 = c0 + new Vec3(size * 3, 0, 0);
            var c2 = c0 + new Vec3(0, size * 3, 0);
            var c3 = c0 + new Vec3(0, 0, size * 3);

            BoundingPoints = new List<Vec3> (verts);

            var bounding_tet = new DTetrahedron(c0, c1, c2, c3);
            //System.Diagnostics.Debug.Assert(bounding_tet.Valid);

            // all the corners of the bounding volume we just invented should be within the sphere of this tet...
            // as should c0 -> c3
            // and the original input points

            foreach (var c in b.Corners)
            {
                MyAssert.IsTrue(bounding_tet.Sphere.Contains(c, -Tolerance / 10), "Bounds corner not in circumsphere");
            }
            foreach (var c in new Vec3[] {c0, c1, c2, c3})
            {
                MyAssert.IsTrue(bounding_tet.Sphere.Contains(c, -Tolerance / 10), "Tet corner not in circumsphere");
            }
            foreach (var c in verts)
            {
                MyAssert.IsTrue(bounding_tet.Sphere.Contains(c, -Tolerance / 10), "Vert not in circumsphere");
            }


            AddTet(bounding_tet);

            // all the corners of the bounding volume we just invented should be within the sphere of this tet...

            //System.Diagnostics.Debug.Assert(Validate());

            foreach (var v in verts)
            {
                AddVert(v);

                //UnityEngine.Debug.Assert(Validate());
            }

            // remove the encapsulating verts we started with, and any associated tets
            var initial_vert_tets = TetsForVert(c0).Concat(TetsForVert(c1)).Concat(TetsForVert(c2)).Concat(TetsForVert(c3)).Distinct().ToList();

            foreach (var tet in initial_vert_tets)
            {
                RemoveTetInner(tet);
            }

            MyAssert.IsTrue(!Verts.Contains(c0), "Initial vert not found");
            MyAssert.IsTrue(!Verts.Contains(c1), "Initial vert not found");
            MyAssert.IsTrue(!Verts.Contains(c2), "Initial vert not found");
            MyAssert.IsTrue(!Verts.Contains(c3), "Initial vert not found");
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
