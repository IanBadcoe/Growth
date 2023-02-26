//#define PROFILE_ON

using System.Collections.Generic;

namespace Growth.Voronoi
{
    // interface for classes which transform ProgressiveVoronoi cells into points in space
    // and 
    interface IPVMapper
    {
        enum CellDir
        {
            PlusX,
            MinusX,
            PlusY,
            MinusY,
            PlusZ,
            MinusZ,
        }

        // for simple mappings can do this mathematically, for more complex
        // keep a look-up
        Vec3Int Vert2Cell(Vec3 vert);

        // not called Cell2Vert
        // because if we are storing anything this is a create operation
        // ProgressiveVoronoi won't be calling this if it alrady has the vert stored
        Vec3 MakeVertForCell(Vec3Int cell);

        // is cell within the grid, solid cells may have a smaller range than vacuum ones, owing to
        // solid cells needing to be surrounded with vacuum ones 
        bool InRange(Vec3Int cell, IProgressiveVoronoi.Solidity permitted_for);

        // permitted_for has same effect as on InRange
        IEnumerable<Vec3Int> AllGridNeighbours(Vec3Int pnt, IProgressiveVoronoi.Solidity permitted_for = IProgressiveVoronoi.Solidity.Vacuum);

        // permitted_for has same effect as on InRange
        IEnumerable<Vec3Int> OrthoGridNeighbours(Vec3Int pnt, IProgressiveVoronoi.Solidity permitted_for = IProgressiveVoronoi.Solidity.Vacuum);

        VBounds Bounds();

        Vec3Int StepCell(Vec3Int cell, CellDir dir, IProgressiveVoronoi.Solidity permitted_for);
    }
}