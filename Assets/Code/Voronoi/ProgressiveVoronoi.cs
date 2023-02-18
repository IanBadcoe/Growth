//#define PROFILE_ON

using Growth.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Growth.Voronoi
{
    class ProgressivePoint : IProgressivePoint
    {
        public ProgressivePoint(Vec3 pos, 
            Vec3Int cell,
            ProgressiveVoronoi pv,
            IVPolyhedron.MeshType mesh_type,
            Material material)
        {
            Cell = cell;
            Voronoi = pv;
            Material = material;

            PolyhedronRW = new VPolyhedron(pos, mesh_type);
        }
        
        public readonly ProgressiveVoronoi Voronoi;

        #region IProgressivePoint
        public bool Exists { get; set; } = false;

        public Vec3 Position => Polyhedron.Centre;

        public Vec3Int Cell { get; }

        public IProgressiveVoronoi.Solidity Solidity { get; set; } = IProgressiveVoronoi.Solidity.Unknown;

        public IVPolyhedron Polyhedron => PolyhedronRW;

        public Face FaceWithNeighbour(IProgressivePoint neighbour)
        {
            Face ret = null;

            FacesMap.TryGetValue(neighbour, out ret);

            return ret;
        }

        public Mesh Mesh
        {
            get
            {
                // on-demand mesh generation...
                if (Polyhedron == null)
                {
                    // if we had one once, scrap it
                    MeshInner = null;

                    return null;
                }

                if (MeshInner == null)
                {
                    MeshInner = MeshUtil.Polyhedron2Mesh(Polyhedron, Voronoi.Cell2Vert(Cell, IProgressiveVoronoi.CellPosition.Origin));
                }

                return MeshInner;
            }
        }

        public IVPolyhedron.MeshType MeshType {
            get
            {
                return Polyhedron.Type;
            }

            set
            {
                PolyhedronRW.Type = value;
            }
        }

        public Material Material { get; set; }
        #endregion

        public VPolyhedron PolyhedronRW { get; }

        private Mesh MeshInner;

        public Dictionary<IProgressivePoint, Face> FacesMap = new Dictionary<IProgressivePoint, Face>();
    }

    class ProgressiveVoronoi : IProgressiveVoronoi
    {
        readonly int Size;
        readonly Delaunay Delaunay;
        readonly Dictionary<Vec3Int, ProgressivePoint> Points;
        readonly ClRand Random;
        readonly RTree<Vec3> PolyVerts;    // these are the polygon/polyhedron verts
                                           // just kept here so we can merge those which are too close together
                                           // to help eliminate degenerate polygons
        readonly float Perturbation;

        public ProgressiveVoronoi(int size, float tolerance, float perturbation, ClRand random)
        {
            Size = size;
            Delaunay = new Delaunay(tolerance);
            Points = new Dictionary<Vec3Int, ProgressivePoint>();
            Random = random;
            PolyVerts = new RTree<Vec3>();
            Perturbation = perturbation;

            InitialiseDelaunay(size);
        }
        
        #region IPolyhedronSet
        public IEnumerable<IVPolyhedron> Polyhedrons => Points.Values.Select(pv => pv.Polyhedron).Where(p => p != null);
        #endregion

        #region IProgressiveVoronoi
        public void AddPoint(Vec3Int cell,
            IVPolyhedron.MeshType mesh_type, Material material)
        {
            PoorMansProfiler.Start("AddPoint");

            if (!InRange(cell, IProgressiveVoronoi.Solidity.Solid))
            {
                throw new ArgumentOutOfRangeException("cell", "solid cells must be 1 cell deep inside the bounds");
            }

            PoorMansProfiler.Start("Adding Points");

            // fill in neighbouring vacuum points, where required, to bound this one...
            // could maybe use only OrthoNeighbour here, but then when adding a diagonal neighbour we
            // might change the shape of this cell, requiring a regeneration of part of it
            // so do this for immutability/simplicity for the moment
            foreach(var pp in this.AllGridNeighbours(cell).Select(pnt => Point(pnt)))
            {
                if (!pp.Exists)
                {
                    AddPointInner(pp.Cell, PerturbPoint(pp.Cell), IProgressiveVoronoi.Solidity.Vacuum,
                        IVPolyhedron.MeshType.Unknown, null);
                }
            }

            var npp = AddPointInner(cell, PerturbPoint(cell),
                IProgressiveVoronoi.Solidity.Solid, mesh_type,
                material);

            PoorMansProfiler.End("Adding Points");

            PoorMansProfiler.Start("Generate Polyhedron");
            GeneratePolyhedron(npp);
            PoorMansProfiler.End("Generate Polyhedron");

            PoorMansProfiler.End("AddPoint");
        }

        public IProgressivePoint Point(Vec3Int cell)
        {
            ProgressivePoint pp;

            if (Points.TryGetValue(cell, out pp))
            {
                return pp;
            }

            // default ProgressivePoint has Exists = false, and Solitity = Unknown...
            return new ProgressivePoint(Cell2Vert(cell, IProgressiveVoronoi.CellPosition.Centre), cell, this, IVPolyhedron.MeshType.Unknown, null);
        }

        public void RemovePoint(Vec3Int position)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<Vec3Int> AllGridNeighbours(Vec3Int pnt, IProgressiveVoronoi.Solidity permitted_for = IProgressiveVoronoi.Solidity.Vacuum)
        {
            foreach (var n in pnt.AllNeighbours)
            {
                if (InRange(n, permitted_for))
                {
                    yield return n;
                }
            }
        }

        public IEnumerable<Vec3Int> OrthoGridNeighbours(Vec3Int pnt, IProgressiveVoronoi.Solidity permitted_for = IProgressiveVoronoi.Solidity.Vacuum)
        {
            foreach (var n in pnt.OrthoNeighbours)
            {
                if (InRange(n, permitted_for))
                {
                    yield return n;
                }
            }
        }

        public IEnumerable<IProgressivePoint> AllPoints => Points.Values;

        public bool InRange(Vec3Int cell, IProgressiveVoronoi.Solidity solid)
        {
            MyAssert.IsTrue(solid != IProgressiveVoronoi.Solidity.Unknown, "Asking InRange question about unknown solitity");

            // vacuum points are allowed all the way to the edge
            if (solid == IProgressiveVoronoi.Solidity.Vacuum)
            {
                return cell.X >= 0 && cell.X < Size
                    && cell.Y >= 0 && cell.Y < Size
                    && cell.Z >= 0 && cell.Z < Size;
            }

            // solid points must have room for a vacuum point next to them...
            return cell.X >= 1 && cell.X < Size - 1
                && cell.Y >= 1 && cell.Y < Size - 1
                && cell.Z >= 1 && cell.Z < Size - 1;
        }

        public Vec3Int Vert2Cell(Vec3 vert)
        {
            // unit-scale for the moment...
            return vert.ToVec3Int();
        }

        public Vec3 Cell2Vert(Vec3Int cell, IProgressiveVoronoi.CellPosition pos)
        {
            // this is giving a "fake" vert, for a cell that doesn't contain one for us to look-up
            // so return the cell centre

            // unit-scale for the moment
            if (pos == IProgressiveVoronoi.CellPosition.Origin)
            {
                return cell.ToVec3();
            }

            return new Vec3(cell.X + 0.5f, cell.Y + 0.5f, cell.Z + 0.5f);
        }

        #endregion

        private void InitialiseDelaunay(int size)
        {
            float half_size = size / 2.0f;

            float sphere_radius = Mathf.Sqrt(half_size * half_size * 3);

            // did a whole slew of maths to show Q = 1/6 * sqrt(2/3) is the closest approach (centre) of
            // a tet face to the tet centroid (when the tet edge length is 1)
            float Q = 1f / 6f / Mathf.Sqrt(2f / 3f);

            float tet_size = sphere_radius / Q;

            // tet_size is the edge length of the tet

            // tet corners are:
            //
            // lower face in XZ plane:
            // a = (-1/2,  -Q,             -1/3 sqrt(3/4))
            // b = (+1/2,  -Q,             -1/3 sqrt(3/4))
            // c = ( 0,    -Q,             +2/3 sqrt(3/4))
            //
            // apex on y axis:
            // d = ( 0,     sqrt(2/3) - Q,  0            )
            //
            // (I did a whole slew of trig to show Q = 1/6 * sqrt(2/3) is the closest approach (centre) of
            //  a tet face to the tet centroid (when the tet edge length is 1) see Docs/TetCentreDistanceMaths.jpg)
            //
            // e.g. XZ plane
            //                         ______________________
            //                        c                  ^   ^
            //                       /|\                 |   |
            //                      / | \                |   |
            //                     /  |  \               |   |
            //                    /   |   \          2/3 |   |
            //                   /    |    \             |   |
            //                1 /     |     \ 1          |   | sqrt(3/4)
            //                 /      |      \           v   |
            //   (x = 0) -----/-------d       \ ---------    |
            //               /        |(above) \         ^   |
            //              /         |         \    1/3 |   |
            //             a__________|__________b_______v___v
            //                 1/2    |    1/2
            //                        |
            //                         (z = 0)
            //
            // and then we'll scale all that up by tet_size


            Vec3 a = new Vec3(-1f / 2, -Q, -1f / 3 * Mathf.Sqrt(3f / 4));
            Vec3 b = new Vec3(+1f / 2, -Q, -1f / 3 * Mathf.Sqrt(3f / 4));
            Vec3 c = new Vec3(0, -Q, 2f / 3 * Mathf.Sqrt(3f / 4));
            Vec3 d = new Vec3(0, Mathf.Sqrt(2f / 3) - Q, 0);

            a *= tet_size;
            b *= tet_size;
            c *= tet_size;
            d *= tet_size;

            // translate the tet so that our requested cube is located between 0 and size:
            var offset = new Vec3(half_size, half_size, half_size);

            a += offset;
            b += offset;
            c += offset;
            d += offset;

            Delaunay.InitialiseWithVerts(new Vec3[] { a, b, c, d });
        }

        private void GeneratePolyhedron(ProgressivePoint pnt)
        {
            // this one should exist...
            MyAssert.IsTrue(Points.ContainsKey(pnt.Cell), "unknown point");

            foreach (ProgressivePoint neighbour in PointNeighbours(pnt))
            {
                //PoorMansProfiler.Start("FaceWithNeighbour");
                // our neighbour may already have the relevant face...
                // (if it is non-solid)
                Face face = neighbour.FaceWithNeighbour(pnt);
                //PoorMansProfiler.End("FaceWithNeighbour");

                if (face == null)
                {
                    //PoorMansProfiler.Start("TryCreateFace");
                    face = TryCreateFace(pnt.Position, neighbour.Position);
                    //PoorMansProfiler.End("TryCreateFace");

                    if (face != null)
                    {
                        // in here, we are a new face to the neighbour as well...
                        neighbour.FacesMap[pnt] = face;
                    }
                }
                else
                {
                    // the face our neighbour has is backwards compared to what we want...
                    face = face.Reversed();
                }

                if (face != null)
                {
                    pnt.FacesMap[neighbour] = face;

                    //PoorMansProfiler.Start("AddFace");
                    pnt.PolyhedronRW.AddFace(face);
                    //PoorMansProfiler.End("AddFace");
                }
            }
        }

        // can return null for a degenerate face
        private Face TryCreateFace(Vec3 our_point, Vec3 other_point)
        {
            PoorMansProfiler.Start("FindTets");
            // find all the tets that use this edge
            var edge_tets = Delaunay.TetsForVert(our_point).Where(tet => tet.UsesVert(other_point)).Distinct().ToList();
            PoorMansProfiler.End("FindTets");

            var face_verts = new List<Vec3>();

            var current_tet = edge_tets[0];

            PoorMansProfiler.Start("FindFromVert");
            // get any one other vert of this tet, this indicates the direction we are
            // "coming from"
            var v_from = current_tet.Verts.Where(v => v != our_point && v != other_point).First();
            PoorMansProfiler.End("FindFromVert");

            PoorMansProfiler.Start("Loop");

            do
            {
                edge_tets.Remove(current_tet);

                face_verts.Add(current_tet.Sphere.Centre);

                // the remaining local vert when we strike off the two common to the edge and the one we came from
                var v_towards = current_tet.Verts.Where(v => v != our_point && v != other_point && v != v_from).First();

                // we move to the tet which shares this face with us...
                var tet_next = edge_tets.Where(tet => tet.UsesVert(our_point) && tet.UsesVert(other_point) && tet.UsesVert(v_towards)).FirstOrDefault();

                current_tet = tet_next;

                // when we move on, we will be moving off using old forwards vert
                v_from = v_towards;
            }
            while (current_tet != null);

            PoorMansProfiler.End("Loop");

            // eliminate any face_verts which are identical (or within a tolerance) of the previous
            // but (i) with randomized seed data we do not expect that to happen much and (ii) all ignoring this does is add degenerate
            // polys/edges to the output, which I do not think will be a problem at the moment

            PoorMansProfiler.Start("MergeVerts");

            Vec3 first_vec = AddFindPolyVert(face_verts[0]);
            Vec3 prev_vec = first_vec;

            List<Vec3> merged_verts = new List<Vec3>(face_verts.Count);
            merged_verts.Add(first_vec);

            // first: AddFind all verts into a set stored on this Voronoi
            for (int i = 1; i < face_verts.Count - 1; i++)
            {
                Vec3 here_vec = AddFindPolyVert(face_verts[i]);

                if (here_vec != prev_vec)
                {
                    merged_verts.Add(here_vec);
                }

                prev_vec = here_vec;
            }

            Vec3 last_vec = AddFindPolyVert(face_verts[face_verts.Count - 1]);

            // because we unconditionally added the first vert
            // the last must be different from the previous and the first
            if (last_vec != first_vec && last_vec != prev_vec)
            {
                merged_verts.Add(last_vec);
            }

            PoorMansProfiler.End("MergeVerts");

            // now, input points (Delaunay.Verts) which are neighbours become such by virtue of still having
            // a common face after this process of eliminating tiny edges/degenerate polys,
            // NOT just because the Delaunay said they were
            //
            // (e.g. they are technically Delaunay-neighbours but the contact polygon is of negligeable area
            // so the neighbour-ness can be ignored, it is only as if the two points were minutely further apart
            // in the first place...)
            //
            // we may get tiny cracks between polys as a result, but the polys themselves should still be closed...

            if (merged_verts.Count < 3)
            {
                return null;
            }

            // can still have faces with tiny area, e.g. very long thin triangles with a vert in the middle,
            // but those are hard to eliminate because the middle vert needs to disappear on this poly
            // but probably _not_ on its neighbour
            //
            // and I think all that happens with those is we do not know which was round to draw them, 
            // but they are all but invisible anyway...

            PoorMansProfiler.Start("Face ctor");

            var ret = new Face(merged_verts, (other_point - our_point).Normalised());

            PoorMansProfiler.End("Face ctor");

            return ret;
        }

        private Vec3 AddFindPolyVert(Vec3 v)
        {
            // take the first existing vert, if any, within tolerance of the new vert
            var tol_vec = new Vec3(Delaunay.Tolerance, Delaunay.Tolerance, Delaunay.Tolerance);
            var tol_bounds = new VBounds(v - tol_vec, v + tol_vec);
            var ret = PolyVerts.Search(tol_bounds).FirstOrDefault();

            if (ret != null)
            {
                return ret;
            }

            PolyVerts.Insert(v);

            return v;
        }

        private IEnumerable<ProgressivePoint> PointNeighbours(IProgressivePoint point)
        {
            return Delaunay.TetsForVert(point.Position)
                .SelectMany(tet => tet.Verts)
                .Where(vert => vert != point.Position)
                .Distinct()
                .Select(vert => Points[Vert2Cell(vert)]);
        }

        private ProgressivePoint AddPointInner(Vec3Int cell, Vec3 pnt,
            IProgressiveVoronoi.Solidity solid, IVPolyhedron.MeshType mesh_type,
            Material material)
        {
            PoorMansProfiler.Start("AddPointInner");
            MyAssert.IsTrue(solid != IProgressiveVoronoi.Solidity.Unknown, "Trying to set point with unknown solitity");

            ProgressivePoint pp;

            if (!Points.TryGetValue(cell, out pp))
            {
                pp = new ProgressivePoint(pnt, cell, this, mesh_type, material);

                Points[cell] = pp;

                MyAssert.IsTrue(Points.ContainsKey(Vert2Cell(pnt)), "Added a point but now cannot find it");

                Delaunay.AddVert(pnt);
            }

            pp.Exists = true;
            pp.Solidity = solid;
            pp.Material = material;
            pp.MeshType = mesh_type;
            pp.Material = material;

            PoorMansProfiler.End("AddPointInner");

            return pp;
        }

        private Vec3 PerturbPoint(Vec3Int cell)
        {
            // our point is in the centre of the cell +/- a randomisation
            return new Vec3(
                cell.X + Random.FloatRange(-Perturbation, Perturbation) + 0.5f,
                cell.Y + Random.FloatRange(-Perturbation, Perturbation) + 0.5f,
                cell.Z + Random.FloatRange(-Perturbation, Perturbation) + 0.5f);
        }
    }
}