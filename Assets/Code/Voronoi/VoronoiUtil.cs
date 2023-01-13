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

    //    Vector3 Point { get; }
    //}

    //public class Face
    //{

    //}

    //public class Vert
    //{

    //}

    //public class VDiagram
    //{
    //    IReadOnlyList<Face> Faces { get; }
    //    IReadOnlyList<Vert> Verts { get; }
    //    IReadOnlyList<CircumSphere> Spheres { get; }
    //    IReadOnlyList<VPolyhedron> Regions { get; }
    //    IReadOnlyList<Vector3> Points { get; }
    //}

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
        public CircumSphere(IReadOnlyList<Vec3> Points)
        {
            var css = new CircumcentreSolver(Points[0], Points[1], Points[2], Points[3]);

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

    [DebuggerDisplay("(({Points[0].X}, {Points[0].Y}, {Points[0].Z}) ({Points[1].X}, {Points[1].Y}, {Points[1].Z}) ({Points[2].X}, {Points[2].Y}, {Points[2].Z}) ({Points[3].X}, {Points[3].Y}, {Points[3].Z}))")]
    public class DTetrahedron
    {
        public DTetrahedron(Vec3 p0, Vec3 p1, Vec3 p2, Vec3 p3)
        {
            PointsRW = new List<Vec3> { p0, p1, p2, p3 };
            Sphere = new CircumSphere(Points);
        }

        List<Vec3> PointsRW;

        public IReadOnlyList<Vec3> Points { get { return PointsRW; } }
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

            foreach (var p in Points)
            {
                if (tet.Points.Contains(p))
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

        public bool Validate()
        {
            return Sphere.Valid;
        }

        public IEnumerable<Triangle> Faces
        {
            get
            {
                // trying to get these rotating the same way
                // but at the moment it doesn't matter
                yield return new Triangle(Points[0], Points[1], Points[2]);
                yield return new Triangle(Points[0], Points[3], Points[1]);
                yield return new Triangle(Points[0], Points[2], Points[3]);
                yield return new Triangle(Points[2], Points[1], Points[3]);
            }
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

    public interface IDelauney
    {
        public IReadOnlyList<DTetrahedron> Tets { get; }
        public IEnumerable<Vec3> Points { get; }

        //        public DTetrahedron[] TetNeighbours(DTetrahedron tet);
    }

    public class Delauney : IDelauney
    {
        readonly float Tolerance;

        public Delauney(float tolerance)
        {
            TetsRW = new List<DTetrahedron>();
            Tolerance = tolerance;
        }

        List<DTetrahedron> TetsRW { get; }

        Dictionary<DTetrahedron, List<DTetrahedron>> Neighbours;

        public IReadOnlyList<DTetrahedron> Tets { get { return TetsRW; } }
        public IEnumerable<Vec3> Points { get { return Tets.SelectMany(x => x.Points).Distinct(); } }

        public void AddTet(DTetrahedron tet)
        {
            TetsRW.Add(tet);
        }

        public bool Validate()
        {
            foreach (var tet in Tets)
            {
                if (!tet.Validate())
                {
                    return false;
                }

                foreach (var p in Points)
                {
                    if (tet.Sphere.Contains(p, Tolerance))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void AddPoint(Vec3 v)
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
    }

    public class VoronoiUtil
    {
        public Delauney CreateDelauney(Vector3[] points)
        {
            Bounds b = new Bounds();

            foreach(var p in points)
            {
                b.Encapsulate(p);
            }

            // build an encapsulating cube, using whichever axis is longest, and padding by 1 on each dimension
            Vec3 low_corner = new Vec3(b.min.x - 1, b.min.y - 1, b.min.z - 1);
            float size = Mathf.Max(b.size.x, b.size.y, b.size.z) + 2;

            // a right-angled prism which contains that box has its corners on the axes at 3x the
            // box dimensions
            // c0 is low_corner
            var c1 = low_corner + new Vec3(size * 3, 0, 0);
            var c2 = low_corner + new Vec3(0, size * 3, 0);
            var c3 = low_corner + new Vec3(0, 0, size * 3);

            var bounding_tet = new DTetrahedron(low_corner, c1, c2, c3);
            System.Diagnostics.Debug.Assert(bounding_tet.Validate());

            var d = new Delauney(1e-3f);
            d.AddTet(bounding_tet);

            System.Diagnostics.Debug.Assert(d.Validate());

            foreach(var p in points)
            {
                var v = new Vec3(p);

                d.AddPoint(v);

                UnityEngine.Debug.Assert(d.Validate());
            }

            return d;
        }
    }
}
