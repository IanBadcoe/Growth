using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Code.Voronoi
{
    public interface IVPolyhedron
    {
        IReadOnlyList<Face> Faces { get; }
        IReadOnlyList<Vec3> Verts { get; }
        Vec3 Centre { get; }
    }

    [DebuggerDisplay("Faces: {Faces.Count} Verts {Verts.Count}")]
    public class VPolyhedron : IVPolyhedron
    {
        public VPolyhedron(Vec3 centre)
        {
            Centre = centre;
            FacesRW = new List<Face>();
        }

        List<Face> FacesRW;

        public IReadOnlyList<Face> Faces => FacesRW;
        public IReadOnlyList<Vec3> Verts => Faces.SelectMany(f => f.Verts).Distinct().ToList();
        public Vec3 Centre { get; }

        public void AddFace(Face face)
        {
            switch (face.CalcRotationDirection(Centre))
            {
                case Face.RotationDirection.Clockwise:
                    FacesRW.Add(face);
                    break;

                case Face.RotationDirection.Anticlockwise:
                    FacesRW.Add(face.Reversed());
                    break;

                case Face.RotationDirection.Indeterminate:
                    UnityEngine.Debug.Assert(false);
                    break;
            }
        }
    }

    [DebuggerDisplay("Verts: {Verts.Count}")]
    public class Face
    {
        public Face(List<Vec3> verts)
        {
            Verts = verts;
        }

        public IReadOnlyList<Vec3> Verts;

        public enum RotationDirection
        {
            Clockwise,
            Anticlockwise,
            Indeterminate
        }

        public RotationDirection CalcRotationDirection(Vec3 from_centre)
        {
            var prev = Verts.Last();

            Vec3 accum = new Vec3();
            foreach(var v in Verts)
            {
                accum += prev.Cross(v);

                prev = v;
            }

            // accum is normal to the face such that viewing _down_ it the face looks anticlockwise

            // so if we get the projection on to that of a vector from our centre to any point in the face
            // that will be -ve for anticlockwise, positive for clockwise and close to zero if something is wrong
            // like a degenerate face or the centre being in the plane of the face
            var prod = (Verts.First() - from_centre).Dot(accum);

            if (prod > 1e-6f)
            {
                return RotationDirection.Clockwise;
            }

            if (prod < -1e-6f)
            {
                return RotationDirection.Anticlockwise;
            }

            return RotationDirection.Indeterminate;
        }

        internal Face Reversed()
        {
            return new Face(Verts.Reverse().ToList());
        }
    }

    public interface IPolyhedronSet
    {
        IReadOnlyList<IVPolyhedron> Polyhedrons { get; }
    }

    public interface IVoronoi : IPolyhedronSet
    {
        IEnumerable<Face> Faces { get; }
        IEnumerable<Vec3> Verts { get; }
        IDelaunay Delaunay { get; }
        float Tolerance { get; }
    }

    [DebuggerDisplay("Regions: {Regions.Count}")]
    public class Voronoi : IVoronoi
    {
        public Voronoi()
        {
            RegionsRW = new Dictionary<Vec3, VPolyhedron>();
        }

        #region IVoronoi
        public IEnumerable<Face> Faces => Polyhedrons.SelectMany(reg => reg.Faces).Distinct();
        public IEnumerable<Vec3> Verts => Polyhedrons.SelectMany(reg => reg.Verts).Distinct();
        public IReadOnlyList<IVPolyhedron> Polyhedrons => RegionsRW.Values.ToList();
        public IReadOnlyList<IVPolyhedron> VPolyhedrons => RegionsRW.Values.ToList();
        public IDelaunay Delaunay { get; set; }
        public float Tolerance => Delaunay.Tolerance;
        #endregion

        Dictionary<Vec3, VPolyhedron> RegionsRW;

        struct Edge : IEquatable<Edge>
        {
            public Edge(Vec3 a, Vec3 b)
            {
                // we do this so that whatever order we fould the verts in
                // the line is trivially comparible
                if (a.IsBefore(b))
                {
                    V1 = a;
                    V2 = b;
                }
                else
                {
                    V1 = b;
                    V2 = a;
                }
            }

            public readonly Vec3 V1;
            public readonly Vec3 V2;

            public bool Equals(Edge other)
            {
                return V1 == other.V1 && V2 == other.V2;
            }

            public override int GetHashCode()
            {
                return V1.GetHashCode() ^ (31 * V2.GetHashCode());
            }
        }

        public void InitialiseFromBoundedDelaunay(IDelaunay d)
        {
            Delaunay = d;

            HashSet<Edge> done = new HashSet<Edge>();

            foreach (var v1 in d.Verts)
            {
                // not interested in trying to find polyhedra for the verts we added just to make a bound...
                if (d.GetVertTags(v1).Contains("bound"))
                {
                    continue;
                }

                // the verts which neighbour v are all the ones used in tets which also use this vert
                // omitting this v itself
                //
                // relying on reference identity/hash for "Distinct" because we do not ever use duplicated verts...
                var neighboring_verts = d.Tets.Where(tet => tet.UsesVert(v1)).SelectMany(tet => tet.Verts).Distinct().Where(v => v != v1).ToList();

                foreach(var v2 in neighboring_verts)
                {
                    var edge = new Edge(v1, v2);
                    if (done.Contains(edge))
                    {
                        continue;
                    }

                    done.Add(edge);

                    // find all the tets that use this edge
                    var edge_tets = d.Tets.Where(tet => tet.UsesVert(v1) && tet.UsesVert(v2)).ToList();

                    var face_verts = new List<Vec3>();

                    var current_tet = edge_tets[0];

                    // get any one other vert of this tet, this indicates the direction we are
                    // "coming from"
                    var v_from = current_tet.Verts.Where(v => v != v1 && v != v2).First();

                    do
                    {
                        edge_tets.Remove(current_tet);

                        face_verts.Add(current_tet.Sphere.Centre);

                        // the remaining local vert when we strike off the two common to the edge and the one we came from
                        var v_towards = current_tet.Verts.Where(v => v != v1 && v != v2 && v != v_from).First();

                        // we move to the tet which shares this face with us...
                        var tet_next = edge_tets.Where(tet => tet.UsesVert(v1) && tet.UsesVert(v2) && tet.UsesVert(v_towards)).FirstOrDefault();

                        current_tet = tet_next;

                        // when we move on, we will be moving off using 
                        v_from = v_towards;
                    }
                    while (current_tet != null);

                    // could, here, eliminate any face_verts which are identical (or withing a tolerance) of the previous
                    // but (i) with randomized seed data we do not expect that to happen much and (ii) all ignoring this does is add degenerate
                    // polys/edges to the output, which I do not think will be a problem at the moment

                    // if it does become a problem, probably handle it by AddFind'ing all verts into a set stored on this Voronoi
                    // and then using the resulting indices to eliminate duplicates, before looking up the actual coords
                    // as this should yield the same results no matter what poly we are testing and/or what order its verts are in

                    // if we do that, then input points (d.Verts) which are neighbours become such by virtue of still having
                    // a common face after this process, NOT because the Delaunay said they were (e.g. they are technically Delaunay-neighbours
                    // but the contact polygon is of negligeable area so the neighbour-ness can be ignored, it is only as if the
                    // two points were minutely further apart in the first place...)

                    Face face = new Face(face_verts);

                    VPolyhedron v1_poly;
                    VPolyhedron v2_poly;

                    if (!RegionsRW.TryGetValue(v1, out v1_poly))
                    {
                        v1_poly = new VPolyhedron(v1);
                        RegionsRW[v1] = v1_poly;
                    }

                    v1_poly.AddFace(face);

                    // if v2 is a vert we want a poly for (non-bound) then this face belongs to that too...
                    if (!d.GetVertTags(v2).Contains("bound"))
                    {
                        if (!RegionsRW.TryGetValue(v2, out v2_poly))
                        {
                            v2_poly = new VPolyhedron(v2);
                            RegionsRW[v2] = v2_poly;
                        }

                        v2_poly.AddFace(face);
                    }                }
            }
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

        public Vec3()
        {
            X = Y = Z = 0;
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

        public static Vec3 operator *(Vec3 lhs, float rhs)
        {
            return new Vec3(lhs.X * rhs, lhs.Y * rhs, lhs.Z * rhs);
        }

        public float Size2()
        {
            return X * X + Y * Y + Z * Z;
        }

        public bool IsBefore(Vec3 other)
        {
            if (X < other.X)
            {
                return true;
            }
            else if (X > other.X)
            {
                return false;
            }

            if (Y < other.Y)
            {
                return true;
            }
            else if (Y > other.Y)
            {
                return false;
            }

            if (Z < other.Z)
            {
                return true;
            }
            else if (Z > other.Z)
            {
                return false;
            }

            // the two points are the same, so "IsBefore" is false, but we really do not expect to get asked this...
            return false;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(X, Y, Z);
        }

        public Vec3 Cross(Vec3 rhs)
        {
            return new Vec3(
                Y * rhs.Z - Z * rhs.Y,
                Z * rhs.X - X * rhs.Z,
                X * rhs.Y - Y * rhs.X);
        }

        public float Dot(Vec3 rhs)
        {
            return X * rhs.X + Y * rhs.Y + Z * rhs.Z;
        }
    }

    [DebuggerDisplay("Centre: ({Centre.X}, {Centre.Y}, {Centre.Z}) Radius: {Radius}")]
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

    [DebuggerDisplay("({V1.X}, {V1.Y}, {V1.Z}) ({V2.X}, {V2.Y}, {V2.Z}) ({V3.X}, {V3.Y}, {V3.Z})")]
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

        public Face ToFace()
        {
            return new Face(Verts.ToList());
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

        public IEnumerable<Triangle> Triangles
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

        public bool UsesVert(Vec3 p)
        {
            return Verts.Contains(p);
        }

        public VPolyhedron ToPolyhedron()
        {
            //            var ret = new VPolyhedron(Sphere.Centre);
            // the centre here is the geometric center of the tetrahedron, not the circumcentre
            var ret = new VPolyhedron(Verts.Aggregate((x, y) => x + y) * 0.25f);

            foreach (var tri in Triangles)
            {
                ret.AddFace(tri.ToFace());
            }

            return ret;
        }
    }

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
                var vert_idxs = f.Verts.Select(vert_idxs => AddFindVert(vert_idxs)).ToArray();

                var tri = new TriIndex(vert_idxs[0], vert_idxs[1], vert_idxs[2]);

                // we build ourselves from tets, when a tet we already saw contains the same face
                // as one being added, it means that is becoming an internal face between two tets
                // and we do not want it...
                if (Triangles.Contains(tri))
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

    public interface IDelaunay : IPolyhedronSet
    {
        public IDelaunay Clone();
        public IReadOnlyList<DTetrahedron> Tets { get; }
        public IEnumerable<Vec3> Verts { get; }
        public List<String> GetVertTags(Vec3 v);
        public float Tolerance { get; }
    }

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
            System.Diagnostics.Debug.Assert(bounding_tet.Valid);

            AddTet(bounding_tet);

            System.Diagnostics.Debug.Assert(Validate());

            foreach (var v in verts)
            {
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

    public class VBounds
    {
        Bounds Bounds = new Bounds();

        public void Encapsulate(Vec3 v)
        {
            Bounds.Encapsulate(v.ToVector3());
        }

        public Vec3 Min => new Vec3(Bounds.min);
        public Vec3 Max => new Vec3(Bounds.max);
        public Vec3 Size => new Vec3(Bounds.size);

        public void Expand(float bound_extension)
        {
            Bounds.Expand(bound_extension);
        }
    }

    public class VoronoiUtil
    {
        public IDelaunay CreateDelaunay(Vec3[] verts)
        {
            return CreateDelaunayInternal(verts);
        }

        Delaunay CreateDelaunayInternal(Vec3[] verts)
        {
            var d = new Delaunay(1e-3f);

            d.InitialiseWithVerts(verts);

            return d;
        }

        // makes a voronoi for the given verts, clipping all the edge polyhedra
        // against an axis-aligned bounding box "bound_extension" larger than the minimum
        // box that would contain the verts
        public IVoronoi CreateBoundedVoronoi(Vec3[] verts, float bound_extension)
        {
            VBounds b = new VBounds();

            foreach (var p in verts)
            {
                b.Encapsulate(p);
            }

            // build an encapsulating cuboid bound_extension bigger than the minimum
            b.Expand(bound_extension);

            var bound_verts = new Vec3[] {
                new Vec3(b.Min.X, b.Min.Y, b.Min.Z),
                new Vec3(b.Min.X, b.Min.Y, b.Max.Z),
                new Vec3(b.Min.X, b.Max.Y, b.Min.Z),
                new Vec3(b.Min.X, b.Max.Y, b.Max.Z),
                new Vec3(b.Max.X, b.Min.Y, b.Min.Z),
                new Vec3(b.Max.X, b.Min.Y, b.Max.Z),
                new Vec3(b.Max.X, b.Max.Y, b.Min.Z),
                new Vec3(b.Max.X, b.Max.Y, b.Max.Z),
            };

            var d = CreateDelaunayInternal(verts.Concat(bound_verts).ToArray());

            foreach(var v in bound_verts)
            {
                d.TagVert(v, "bound");
            }

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
