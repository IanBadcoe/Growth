using System;
using System.Collections.Generic;
using UnityEngine;

namespace Growth.Voronoi
{
    class ProgressiveVoronoi : IProgressiveVoronoi
    {
        readonly Delaunay Delaunay;

        public ProgressiveVoronoi(float size, float tolerance)
        {
            Delaunay = new Delaunay(tolerance);

            InitialiseDelaunay(size);
        }
        
        #region IPolyhedronSet
        
        public IReadOnlyList<IVPolyhedron> Polyhedrons => throw new NotImplementedException();
        
        #endregion

        #region IProgressiveVoronoi
        public void AddPoint(Vec3 position, IProgressiveVoronoi.Solidity solid)
        {
            throw new System.NotImplementedException();
        }

        public IProgressivePoint Point(Vec3Int pos)
        {
            throw new System.NotImplementedException();
        }

        public void RemovePoint(Vec3 position)
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
    }
}