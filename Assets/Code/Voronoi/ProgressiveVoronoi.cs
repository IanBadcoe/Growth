using Growth.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Growth.Voronoi
{
    class ProgressivePoint : IProgressivePoint
    {
        public ProgressivePoint(Vec3 pos, Vec3Int cell)
        {
            Position = pos;
            Cell = cell;
        }

        public bool Exists { get; set; } = false;

        public Vec3 Position { get; }

        public Vec3Int Cell { get; }

        public IProgressiveVoronoi.Solidity Solidity { get; set; } = IProgressiveVoronoi.Solidity.Unknown;

        public IVPolyhedron Polyhedron => PolyhedronRW;

        public VPolyhedron PolyhedronRW { get; set; }

        public Mesh Mesh { get; set; }

        public Face FaceWithNeighbour(IProgressivePoint neighbour)
        {
            Face ret = null;

            FacesMap.TryGetValue(neighbour, out ret);

            return ret;
        }

        public Dictionary<IProgressivePoint, Face> FacesMap = new Dictionary<IProgressivePoint, Face>();
    }

    class ProgressiveVoronoi : IProgressiveVoronoi
    {
        readonly float Size;
        readonly Delaunay Delaunay;
        readonly Dictionary<Vec3Int, ProgressivePoint> Points;
        readonly ClRand Random;

        public ProgressiveVoronoi(float size, float tolerance, ClRand random)
        {
            Size = size;
            Delaunay = new Delaunay(tolerance);
            Points = new Dictionary<Vec3Int, ProgressivePoint>();
            Random = random;

            InitialiseDelaunay(size);
        }
        
        #region IPolyhedronSet
        public IEnumerable<IVPolyhedron> Polyhedrons => Points.Values.Select(pv => pv.Polyhedron);
        #endregion

        #region IProgressiveVoronoi
        public void AddPoint(Vec3Int cell)
        {
            // fill in neighbouring vacuum points, where required, to bound this one...
            // could maybe use only OrthoNeighbour here, but then when adding a diagonal neighbour we
            // might change the shape of this cell, requiring a regeneration of part of it
            // so do this for immutability/simplicity for the moment
            foreach(var pp in this.AllGridNeighbours(cell).Select(pnt => Point(pnt)))
            {
                if (!pp.Exists)
                {
                    AddPointInner(PerturbPoint(pp.Cell), IProgressiveVoronoi.Solidity.Vacuum);
                }
            }

            AddPointInner(PerturbPoint(cell), IProgressiveVoronoi.Solidity.Solid);

            GeneratePolyhedron(cell);
        }

        private void GeneratePolyhedron(Vec3Int cell)
        {
            // this one should exist...
            ProgressivePoint point = Points[cell];

            if (point.Polyhedron == null)
            {
                point.PolyhedronRW = new VPolyhedron(point.Position);
            }

            foreach (ProgressivePoint neighbour in PointNeighbours(point))
            {
                // our neighbour may already have the relevant face...
                // (if it is non-solid)
                Face face = neighbour.FaceWithNeighbour(point);

                if (face == null)
                {
                    face = CreateFace(point.Position, neighbour.Position);

                    // in here, we are a new face to the neighbour as well...
                    neighbour.FacesMap[point] = face;
                }

                point.FacesMap[neighbour] = face;

                point.PolyhedronRW.AddFace(face);

                // currently our use-case is building a poly for one (new) point at a time,
                // so no need to add the face to any poly on the neighbour
                // but other types of update might need that in time...
            }
        }

        private Face CreateFace(Vec3 v1, Vec3 v2)
        {
            // find all the tets that use this edge
            var edge_tets = Delaunay.Tets.Where(tet => tet.UsesVert(v1) && tet.UsesVert(v2)).ToList();

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

                // when we move on, we will be moving off using old forwards vert
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

            return new Face(face_verts);
        }

        private IEnumerable<ProgressivePoint> PointNeighbours(IProgressivePoint point)
        {
            return Delaunay.Tets
                .Where(tet => tet.UsesVert(point.Position))
                .SelectMany(tet => tet.Verts)
                .Where(vert => vert != point.Position)
                .Distinct()
                .Select(vert => Points[Vert2Cell(vert)]);
        }

        private Vec3Int Vert2Cell(Vec3 vert)
        {
            // unit-scale and no -ves as yet...
            return vert.ToVec3Int();
        }

        private ProgressivePoint AddPointInner(Vec3 pnt, IProgressiveVoronoi.Solidity solid)
        {
            Debug.Assert(solid != IProgressiveVoronoi.Solidity.Unknown);

            // for the moment we jsut truncate (and we have no -ve values, so no problem)
            // want to try and keep it so that if the grid scale changes, or we get -ves, or whatever
            // about the grid, just changing Vert2Cell will fix things...
            //
            // (the reverse lookup comes from Points...)
            var cell = Vert2Cell(pnt);

            // should not try to add one we already have...
            Debug.Assert(!Points.ContainsKey(cell));

            var pp = new ProgressivePoint(pnt, cell);
            pp.Exists = true;
            pp.Solidity = solid;

            Points[cell] = pp;

            Delaunay.AddVert(pnt);

            return pp;
        }

        private Vec3 PerturbPoint(Vec3Int cell)
        {
            // our point is in the centre of the cell +/- a randomisation
            return new Vec3(
                cell.X + Random.FloatRange(-1f / 3, 1f / 3) + 0.5f,
                cell.Y + Random.FloatRange(-1f / 3, 1f / 3) + 0.5f,
                cell.Z + Random.FloatRange(-1f / 3, 1f / 3) + 0.5f);
        }

        public IProgressivePoint Point(Vec3Int cell)
        {
            ProgressivePoint pp;

            if (Points.TryGetValue(cell, out pp))
            {
                return pp;
            }

            // default ProgressivePoint has Exists = false, and Solitity = Unknown...
            return new ProgressivePoint(cell.ToVec3(), cell);
        }

        public void RemovePoint(Vec3Int position)
        {
            throw new System.NotImplementedException();
        }

        public void SetSolidity(Vec3Int pos, IProgressiveVoronoi.Solidity solid)
        {
            throw new System.NotImplementedException();
        }
        #endregion

        private void InitialiseDelaunay(float size)
        {
            float half_size = size / 2;

            float sphere_radius = Mathf.Sqrt(half_size * half_size * 3);

            float Q = 2f / 9f * 3f / 4f / Mathf.Sqrt(2f / 3f);

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

            Delaunay.InitialiseWithVerts(new Vec3[] { a, b, c, d });
        }

        IEnumerable<Vec3Int> AllGridNeighbours(Vec3Int pnt)
        {
            foreach (var n in pnt.AllNeighbours)
            {
                if (InRange(n, IProgressiveVoronoi.Solidity.Vacuum))
                {
                    yield return n;
                }
            }
        }

        bool InRange(Vec3Int pnt, IProgressiveVoronoi.Solidity solid)
        {
            Debug.Assert(solid != IProgressiveVoronoi.Solidity.Unknown);

            // vacuum points are allowed all the way to the edge
            if (solid == IProgressiveVoronoi.Solidity.Vacuum)
            {
                return pnt.X >= 0 && pnt.X < Size
                    && pnt.Y >= 0 && pnt.Y < Size
                    && pnt.Z >= 0 && pnt.Z < Size;
            }

            // solid points must have room for a vacuum point next to them...
            return pnt.X >= 1 && pnt.X < Size - 1
                && pnt.Y >= 1 && pnt.Y < Size - 1
                && pnt.Z >= 1 && pnt.Z < Size - 1;
        }
    }
}