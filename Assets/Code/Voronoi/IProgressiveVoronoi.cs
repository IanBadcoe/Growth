using Growth.Util;
using System.Collections.Generic;
using UnityEngine;

namespace Growth.Voronoi
{
    // create a large, empty space and add volumes progressively as required
    // - intended for "sparse" usage, with little cost to large empty areas
    // - points can be added with or without "solid" flag (opposite is "vacuum")
    // -- solid points add a volume, vacuum ones do not
    // -- vacuum points are added around solid ones and control their size
    // -- so the "sparseness" works by having areas of solid points, surrounded by vacuum ones to control their boundary
    // -- and no points between them and the next non-sparse area
    // - eventually may add ability to truly remove a point, at the moment just switch it back to vacuum
    // - the "large empty space" is a cube, to keep that inside the Delauney triangularisation we wrap that in a large "vacuum"
    //   tetrahedron
    // -- the size of that is detemined by it is the smallest, regular tetrahedron which can contain a sphere which can contain the cube
    // -- will put the maths for that somewhere in the implementation...
    // - there is an underlying cubic grid, which is only used to allow the auto-addition of vacuum points around new additions
    // -- for the moment limit to one point per grid cell, but this may not ultimately be required as a constraint
    // -- e.g. surrounding the added point/shape any grid cells which do not already contain a point need a vacuum point added
    // -- the cell size of this grid defines the average size for added volumes
    // - will probably internally use a spatial subdivision to find circumspheres fast
    public interface IProgressiveVoronoi : IPolyhedronSet
    {
        enum Solidity
        {
            Unknown,
            Solid,
            Vacuum
        }

        enum CellPosition
        {
            Centre,
            Origin
        }

        void AddPoint(Vec3Int position);            // currently this is implicitly "solid" as we do not need any manual way of adding
                                                                                    // vacuum points...
        void RemovePoint(Vec3Int position);         // currently a euphemism for setting it vacuum, but want a true delete later
                                                    // will need to make that "smart" in that if we are a bound of a point which is still
                                                    // solid, then we need not to delete, and similarly, if we have vacuum neighbours
                                                    // which are only there for us, they need to delete too...
        IProgressivePoint Point(Vec3Int pos);
        void SetSolidity(Vec3Int pos, Solidity solid);

        public IEnumerable<Vec3Int> AllGridNeighbours(Vec3Int pnt);

        IEnumerable<IProgressivePoint> AllPoints { get; }

        bool InRange(Vec3Int cell, IProgressiveVoronoi.Solidity solid);     // "vacuum" cells can go right up to the edge
                                                                            // "solid" cells must remain 1 inside that
        Vec3 Cell2Vert(Vec3Int cell, CellPosition pos);
        Vec3Int Vert2Cell(Vec3 vert);
    }

    public interface IProgressivePoint
    {
        bool Exists { get; }            // we need something to return, even if we have no point at this point
        Vec3 Position { get; }
        Vec3Int Cell { get; }           // even if we have no point, this is filled in with the centre of the cell asked about
        IProgressiveVoronoi.Solidity Solidity { get; }
        IVPolyhedron Polyhedron { get; }
        Face FaceWithNeighbour(IProgressivePoint neighbour);
        Mesh Mesh { get; }
    }
}