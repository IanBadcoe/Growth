using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Code.Voronoi
{
    //public class VPolyhedron
    //{
    //    IReadOnlyList<Face> Faces { get; }
    //    IReadOnlyList<Vert> Verts { get; }
    //    IReadOnlyList<CircumSphere> Spheres { get; }
    //    IReadOnlyList<VPolyhedron> Heighbours { get; }

    //    Vector3 Vert { get; }
    //}

    //public class Face
    //{

    //}

    //public class Vert
    //{

    //}

    public interface IVoronoi
    {
        //IReadOnlyList<Face> Faces { get; }
        //IReadOnlyList<Vert> Verts { get; }
        //IReadOnlyList<CircumSphere> Spheres { get; }
        //IReadOnlyList<VPolyhedron> Regions { get; }
        //IReadOnlyList<Vector3> Verts { get; }
    }

    public class Voronoi : IVoronoi
    {
        internal void InitialiseFromBoundedDelaunay(IDelaunay d)
        {
            foreach (var v in d.Verts)
            { }
        }
    }

    [DebuggerDisplay("({X}, {Y}, {Z})")]
    public class Vec3
    {
        public Vec3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vec3(Vector3 p)
        {
            X = p.x;
            Y = p.y;
            Z = p.z;
        }

        public readonly float X;
        public readonly float Y;
        public readonly float Z;

        public static Vec3 operator +(Vec3 lhs, Vec3 rhs)
        {
            return new Vec3(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z);
        }
        public static Vec3 operator -(Vec3 lhs, Vec3 rhs)
        {
            return new Vec3(lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z);
        }
        public float Size2()
        {
            return X * X + Y * Y + Z * Z;
        }
    }

    public class CircumSphere
    {
        public CircumSphere(IReadOnlyList<Vec3> Verts)
        {
            var css = new CircumcentreSolver(Verts[0], Verts[1], Verts[2], Verts[3]);

            Valid = css.Valid;
            Centre = css.Centre;
            Radius = (float)css.Radius;
        }

        public bool Valid { get; }
        public Vec3 Centre { get; }
        public float Radius { get; }

        // we must overlap by more than t to be considered containing,
        // set t -ve if you want to make the test looser
        public bool Contains(Vec3 p, float t)
        {
            return (Centre - p).Size2() < (Radius - t) * (Radius - t);
        }
    }

    public class Triangle
    {
        public Triangle(Vec3 p1, Vec3 p2, Vec3 p3)
        {
            V1 = p1;
            V2 = p2;
            V3 = p3;
        }

        public readonly Vec3 V1;
        public readonly Vec3 V2;
        public readonly Vec3 V3;

        public IEnumerable<Vec3> Verts
        {
            get
            {
                yield return V1;
                yield return V2;
                yield return V3;
            }
        }
    }

    [DebuggerDisplay("(({Verts[0].X}, {Verts[0].Y}, {Verts[0].Z}) ({Verts[1].X}, {Verts[1].Y}, {Verts[1].Z}) ({Verts[2].X}, {Verts[2].Y}, {Verts[2].Z}) ({Verts[3].X}, {Verts[3].Y}, {Verts[3].Z}))")]
    public class DTetrahedron
    {
        public DTetrahedron(Vec3 p0, Vec3 p1, Vec3 p2, Vec3 p3)
        {
            Verts = new List<Vec3> { p0, p1, p2, p3 };
            Sphere = new CircumSphere(Verts);
        }

        public IReadOnlyList<Vec3> Verts { get; }
        public CircumSphere Sphere { get; }

        public enum AdjoinsResult
        {
            Separate,
            Point,
            Edge,
            Face,
            Identity
        }

        public AdjoinsResult Adjoins(DTetrahedron tet)
        {
            int count = 0;

            foreach (var p in Verts)
            {
                if (tet.Verts.Contains(p))
                {
                    count++;
                }
            }

            switch (count)
            {
                case 1:
                    return AdjoinsResult.Point;
                case 2:
                    return AdjoinsResult.Edge;
                case 3:
                    return AdjoinsResult.Face;
                case 4:
                    return AdjoinsResult.Identity;
            }

            return AdjoinsResult.Separate;
        }

        public bool Valid
        {
            get
            {
                return Sphere.Valid;
            }
        }

        public IEnumerable<Triangle> Faces
        {
            get
            {
                // trying to get these rotating the same way
                // but at the moment it doesn't matter
                yield return new Triangle(Verts[0], Verts[1], Verts[2]);
                yield return new Triangle(Verts[0], Verts[3], Verts[1]);
                yield return new Triangle(Verts[0], Verts[2], Verts[3]);
                yield return new Triangle(Verts[2], Verts[1], Verts[3]);
            }
        }

        internal bool UsesVert(Vec3 p)
        {
            return Verts.Contains(p);
        }
    }

    // a polyhedron made of triangles
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

        public readonly List<TriIndex> Faces = new List<TriIndex>();
        public IEnumerable<Triangle> TriFaces
        {
            get
            {
                return Faces.Select(f => new Triangle(Verts[f.I1], Verts[f.I2], Verts[f.I3]));
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
            foreach (var f in tet.Faces)
            {
                var vert_idxs = f.Verts.Select(vert_idxs => AddFindVert(vert_idxs)).ToArray();

                var tri = new TriIndex(vert_idxs[0], vert_idxs[1], vert_idxs[2]);

                // we build ourselves from tets, when a tet we already saw contains the same face
                // as one being added, it means that is becoming an internal face between two tets
                // and we do not want it...
                if (Faces.Contains(tri))
                {
                    Faces.Remove(tri);
                }
                else
                {
                    Faces.Add(tri);
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

    public interface IDelaunay
    {
        public IDelaunay Clone();
        public IReadOnlyList<DTetrahedron> Tets { get; }
        public IEnumerable<Vec3> Verts { get; }
    }

    public class Delaunay : IDelaunay
    {
        readonly float Tolerance;

        public Delaunay(float tolerance)
        {
            TetsRW = new List<DTetrahedron>();
            Tolerance = tolerance;
        }

        public Delaunay(Delaunay old)
        {
            // this works because Vec3, CircumSphere and DTetrahedron are all immutable, so by cloning the current state
            // of old we are good to be valid in that state forever...
            TetsRW = TetsRW;
            Tolerance = old.Tolerance;
        }

        List<DTetrahedron> TetsRW { get; }

        public IReadOnlyList<DTetrahedron> Tets { get { return TetsRW; } }
        public IEnumerable<Vec3> Verts { get { return Tets.SelectMany(x => x.Verts).Distinct(); } }

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
            var bad_tets = Tets.Where(tet => tet.Sphere.Contains(v, Tolerance)).ToList();

            var pt = new TriangularPolyhedron(bad_tets);

            foreach(var tet in bad_tets)
            {
                TetsRW.Remove(tet);
            }

            foreach(var tri in pt.TriFaces)
            {
                var tet = new DTetrahedron(v, tri.V1, tri.V2, tri.V3);

                AddTet(tet);
            }
        }

        internal void InitialiseWithVerts(Vector3[] verts)
        {
            Bounds b = new Bounds();

            foreach (var p in verts)
            {
                b.Encapsulate(p);
            }

            // build an encapsulating tetrahedron, using whichever axis is longest, and padding by 1 on each dimension
            Vec3 c0 = new Vec3(b.min.x - 1, b.min.y - 1, b.min.z - 1);
            float size = Mathf.Max(b.size.x, b.size.y, b.size.z) + 2;

            // a right-angled prism which contains that box has its corners on the axes at 3x the
            // box dimensions
            var c1 = c0 + new Vec3(size * 3, 0, 0);
            var c2 = c0 + new Vec3(0, size * 3, 0);
            var c3 = c0 + new Vec3(0, 0, size * 3);

            var bounding_tet = new DTetrahedron(c0, c1, c2, c3);
            System.Diagnostics.Debug.Assert(bounding_tet.Valid);

            AddTet(bounding_tet);

            System.Diagnostics.Debug.Assert(Validate());

            foreach (var p in verts)
            {
                var v = new Vec3(p);

                AddVert(v);

                UnityEngine.Debug.Assert(Validate());
            }

            // remove the encapsulating verts we started with, and any associated tets
            var encapsulating_vert_tets = Tets.Where(x => x.UsesVert(c0) || x.UsesVert(c1) || x.UsesVert(c2) || x.UsesVert(c3)).ToList();

            TetsRW.RemoveAll(tet => encapsulating_vert_tets.Contains(tet));

            UnityEngine.Debug.Assert(!Verts.Contains(c0));
            UnityEngine.Debug.Assert(!Verts.Contains(c1));
            UnityEngine.Debug.Assert(!Verts.Contains(c2));
            UnityEngine.Debug.Assert(!Verts.Contains(c3));
        }

        public IDelaunay Clone()
        {
            return new Delaunay(this);
        }
    }

    public class VoronoiUtil
    {
        public Delaunay CreateDelaunay(Vector3[] verts)
        {
            var d = new Delaunay(1e-3f);

            d.InitialiseWithVerts(verts);

            return d;
        }

        // makes a voronoi for the given verts, clipping all the edge polyhedra
        // against an axis-aligned bounding box "bound_extension" larger than the minimum
        // box that would contain the verts
        public IVoronoi CreateBoundedVoronoi(Vector3[] verts, float bound_extension)
        {
            Bounds b = new Bounds();

            foreach (var p in verts)
            {
                b.Encapsulate(p);
            }

            // build an encapsulating cuboid bound_extension bigger than the minimum
            b.Expand(bound_extension);

            var extended_verts = verts.Concat(new Vector3[] {
                new Vector3(b.min.x, b.min.y, b.min.z),
                new Vector3(b.min.x, b.min.y, b.max.z),
                new Vector3(b.min.x, b.max.y, b.min.z),
                new Vector3(b.min.x, b.max.y, b.max.z),
                new Vector3(b.max.x, b.min.y, b.min.z),
                new Vector3(b.max.x, b.min.y, b.max.z),
                new Vector3(b.max.x, b.max.y, b.min.z),
                new Vector3(b.max.x, b.max.y, b.max.z),
            }).ToArray();

            var d = CreateDelaunay(extended_verts);

            return CreateVoronoiInternal(d);
        }

        private IVoronoi CreateVoronoiInternal(IDelaunay d)
        {
            var v = new Voronoi();

            v.InitialiseFromBoundedDelaunay(d);

            return v;
        }

    }
}
