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
            Solid,
            Vacuum
        }

        void AddPoint(Vec3 position, Solidity solid);
        void RemovePoint(Vec3 position);        // currently a euphemism for setting it vacuum
        IProgressivePoint Point(Vec3Int pos);
        void SetSolidity(Vec3Int pos, Solidity solid);
    }

    public interface IProgressivePoint
    {
        Vec3 Position { get; }
        IProgressiveVoronoi.Solidity Solidity { get; }
        IVPolyhedron Polyhedron { get; }
        Mesh Mesh { get; }
    }
}