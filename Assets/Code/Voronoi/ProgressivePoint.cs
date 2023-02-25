//#define PROFILE_ON

using Growth.Util;
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
            return PolyhedronRW.GetFaceByKey(neighbour);
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

        public IVPolyhedron.MeshType MeshType
        {
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
    }
}