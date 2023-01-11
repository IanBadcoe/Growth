using System;
using System.Collections.Generic;
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
    }

    // a polyhedron made of triangles
    public class PolyTriangle
    {
        struct TriIndex
        {
            int i1;
            int i2;
            int i3;
        }

        List<TriIndex> Faces;
        List<Vec3> Points;

        public PolyTriangle(List<DTetrahedron> tets)
        {
            foreach(var tet in tets)
            {
                foreach(var p in tet.Points)
                {
                    if (!Points.Contains(p))
                    {
                        Points.Add(p);

                        // @@
                    }
                }
            }
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
        //public DTetrahedron[] TetNeighbours(DTetrahedron tet)
        //{
        //    throw new NotImplementedException();
        //}

        public void AddTet(DTetrahedron tet)
        {
            //List<DTetrahedron> neighbs = new List<DTetrahedron>();

            //foreach(var other in Tets)
            //{
            //    if (other.Adjoins(tet) == DTetrahedron.AdjoinsResult.Face)
            //    {
            //        neighbs.Append(other);
            //    }
            //}
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

            var pt = new PolyTriangle(bad_tets);
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
            }

            return d;
        }
    }
}
