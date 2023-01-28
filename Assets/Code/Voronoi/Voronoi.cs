using Growth.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Growth.Voronoi
{
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
        public IEnumerable<IVPolyhedron> Polyhedrons => RegionsRW.Values;
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

                    // could, here, eliminate any face_verts which are identical (or within a tolerance) of the previous
                    // but (i) with randomized seed data we do not expect that to happen much and (ii) all ignoring this does is add degenerate
                    // polys/edges to the output, which I do not think will be a problem at the moment

                    // if it does become a problem, probably handle it by AddFind'ing all verts into a set stored on this Voronoi
                    // and then using the resulting indices to eliminate duplicates, before looking up the actual coords
                    // as this should yield the same results no matter what poly we are testing and/or what order its verts are in

                    // if we do that, then input points (d.Verts) which are neighbours become such by virtue of still having
                    // a common face after this process, NOT because the Delaunay said they were (e.g. they are technically Delaunay-neighbours
                    // but the contact polygon is of negligeable area so the neighbour-ness can be ignored, it is only as if the
                    // two points were minutely further apart in the first place...)

                    Face face = new Face(face_verts, (v2 - v1).Normalised());

                    VPolyhedron v1_poly;
                    VPolyhedron v2_poly;

                    if (!RegionsRW.TryGetValue(v1, out v1_poly))
                    {
                        v1_poly = new VPolyhedron(v1, IVPolyhedron.MeshType.Smooth);
                        RegionsRW[v1] = v1_poly;
                    }

                    v1_poly.AddFace(face);

                    // if v2 is a vert we want a poly for (non-bound) then this face belongs to that too...
                    if (!d.GetVertTags(v2).Contains("bound"))
                    {
                        if (!RegionsRW.TryGetValue(v2, out v2_poly))
                        {
                            v2_poly = new VPolyhedron(v2, IVPolyhedron.MeshType.Smooth);
                            RegionsRW[v2] = v2_poly;
                        }

                        v2_poly.AddFace(face.Reversed());
                    } 
                }
            }
        }
    }
}
