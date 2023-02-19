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

        IProgressivePoint AddPoint(Vec3Int position,    // currently this is implicitly "solid" as we do not need any manual way of adding
            IVPolyhedron.MeshType mesh_type,            // vacuum points...
            Material material);                 
                                                
        IProgressivePoint Point(Vec3Int pos);

        // vaccuum points are allowed right up to the grid edge
        // solid points must be one cell in (so they can be bounded by vacuum)
        public IEnumerable<Vec3Int> AllGridNeighbours(Vec3Int pnt,
            Solidity permitted_for = IProgressiveVoronoi.Solidity.Vacuum);

        public IEnumerable<Vec3Int> OrthoGridNeighbours(Vec3Int pnt,
            Solidity permitted_for = IProgressiveVoronoi.Solidity.Vacuum);

        IEnumerable<IProgressivePoint> AllPoints { get; }

        bool InRange(Vec3Int cell, IProgressiveVoronoi.Solidity solid);     // "vacuum" cells can go right up to the edge
                                                                            // "solid" cells must remain 1 inside that
        Vec3 Cell2Vert(Vec3Int cell, CellPosition pos);
        Vec3Int Vert2Cell(Vec3 vert);

        IDelaunay Delaunay { get; }
    }
}