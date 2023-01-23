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

        public IVPolyhedron Polyhedron { get; set; }

        public Mesh Mesh { get; set; }
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
        
        public IReadOnlyList<IVPolyhedron> Polyhedrons => throw new NotImplementedException();
        
        #endregion

        #region IProgressiveVoronoi
        public void AddPoint(Vec3Int cell)
        {
            // fill in neighbouring vacuum points, where required, to bound this one...
            foreach(var pp in this.Neighbours(cell).Select(pnt => Point(pnt)))
            {
                if (!pp.Exists)
                {
                    AddPointInner(PerturbPoint(pp.Cell), IProgressiveVoronoi.Solidity.Vacuum);
                }
            }

            AddPointInner(PerturbPoint(cell), IProgressiveVoronoi.Solidity.Solid);
        }

        private ProgressivePoint AddPointInner(Vec3 pnt, IProgressiveVoronoi.Solidity solid)
        {
            Debug.Assert(solid != IProgressiveVoronoi.Solidity.Unknown);

            var cell = pnt.ToVec3Int();

            // should not try to add one we already have...
            Debug.Assert(!Points.ContainsKey(cell));

            var pp = new ProgressivePoint(pnt, cell);
            pp.Exists = true;
            pp.Solidity = solid;

            Points[cell] = pp;

            return pp;
        }

        private Vec3 PerturbPoint(Vec3Int cell)
        {
            return new Vec3(
                cell.X + Random.FloatRange(-1f / 3, 1f / 3),
                cell.Y + Random.FloatRange(-1f / 3, 1f / 3),
                cell.Z + Random.FloatRange(-1f / 3, 1f / 3));
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

        IEnumerable<Vec3Int> Neighbours(Vec3Int pnt)
        {
            foreach (var n in pnt.OrthoNeighbours)
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