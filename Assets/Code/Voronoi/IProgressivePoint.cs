using UnityEngine;

namespace Growth.Voronoi
{
    public interface IProgressivePoint
    {
        bool Exists { get; }            // we need something to return, even if we have no point at this point
        Vec3 Position { get; }
        Vec3Int Cell { get; }           // even if we have no point, this is filled in with the centre of the cell asked about
        IProgressiveVoronoi.Solidity Solidity { get; }
        IVPolyhedron Polyhedron { get; }
        Face FaceWithNeighbour(IProgressivePoint neighbour);
        Mesh Mesh { get; }
        IVPolyhedron.MeshType MeshType { get; }
        Material Material { get; }
    }
}