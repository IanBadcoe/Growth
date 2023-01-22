using System;
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
            // 0.25 because this is the size, not the radius/half-size
            float sphere_radius = Mathf.Sqrt(size * size * 3 * 0.25f);

            Vec3 sphere_centre = new Vec3(0, 0, 0);

            // q is the distance of the tet centre to the closest point on each face as a fraction of the tet edge length
            // derived from a slab of maths, which I will photograph and attach
            float Q = 2f / 9f * 3f / 4f / Mathf.Sqrt(2f / 3f);

            float tet_size = sphere_radius / Q;

            // tet_size is the edge length of the tet

            // tet corners are:
            //
            // lower face in XZ plane:
            // a = (-1/2,  -Q,     -1/3 sqrt(3/4))
            // b = (+1/2,  -Q,     -1/3 sqrt(3/4))
            // c = ( 0,    -Q,     +2/3 sqrt(3/4))
            //
            // apex on y axis:
            // d = ( 0,     1 - Q,  0            )
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
            // and then just scale all that up by tet_size
        }
    }
}