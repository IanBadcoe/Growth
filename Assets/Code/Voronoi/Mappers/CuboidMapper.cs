using Growth.Util;
using System.Collections.Generic;

namespace Growth.Voronoi.Mappers
{
    class CuboidMapper : IPVMapper
    {
        readonly Vec3 Size;
        readonly Vec3Int Cells;
        readonly Vec3 Perturbation;
        readonly ClRand Random;

        public CuboidMapper(Vec3 size, Vec3Int cells, Vec3 perturbation, ClRand random)
        {
            Size = size;
            Cells = cells;
            Perturbation = perturbation;
            Random = random;
        }

        #region IPVMapper
        public Vec3 MakeVertForCell(Vec3Int cell)
        {
            // our point is in the centre of the cell +/- a randomisation
            return new Vec3(
                (cell.X + Random.FloatRange(-Perturbation.X, Perturbation.X) + 0.5f) * Size.X / Cells.X,
                (cell.Y + Random.FloatRange(-Perturbation.Y, Perturbation.Y) + 0.5f) * Size.Y / Cells.Y,
                (cell.Z + Random.FloatRange(-Perturbation.Z, Perturbation.Z) + 0.5f) * Size.Z / Cells.Z);
        }

        public bool InRange(Vec3Int cell, IProgressiveVoronoi.Solidity permitted_for)
        {
            MyAssert.IsTrue(permitted_for != IProgressiveVoronoi.Solidity.Unknown, "Asking InRange question about unknown solidity");

            // vacuum points are allowed all the way to the edge
            if (permitted_for == IProgressiveVoronoi.Solidity.Vacuum)
            {
                return cell.X >= 0 && cell.X < Cells.X
                    && cell.Y >= 0 && cell.Y < Cells.Y
                    && cell.Z >= 0 && cell.Z < Cells.Z;
            }

            // solid points must have room for a vacuum point next to them...
            return cell.X >= 1 && cell.X < Cells.X - 1
                && cell.Y >= 1 && cell.Y < Cells.Y - 1
                && cell.Z >= 1 && cell.Z < Cells.Z - 1;
        }

        public IEnumerable<Vec3Int> AllGridNeighbours(Vec3Int pnt, IProgressiveVoronoi.Solidity permitted_for = IProgressiveVoronoi.Solidity.Vacuum)
        {
            foreach (var n in pnt.AllNeighbours)
            {
                if (InRange(n, permitted_for))
                {
                    yield return n;
                }
            }
        }

        public IEnumerable<Vec3Int> OrthoGridNeighbours(Vec3Int pnt, IProgressiveVoronoi.Solidity permitted_for = IProgressiveVoronoi.Solidity.Vacuum)
        {
            foreach (var n in pnt.OrthoNeighbours)
            {
                if (InRange(n, permitted_for))
                {
                    yield return n;
                }
            }
        }

        public Vec3Int Vert2Cell(Vec3 vert)
        {
            return new Vec3Int(
                (int)(vert.X / Size.X * Cells.X),
                (int)(vert.Y / Size.Y * Cells.Y),
                (int)(vert.Z / Size.Z * Cells.Z));
        }

        public VBounds Bounds()
        {
            return new VBounds(new Vec3(0, 0, 0),
                Size);
        }

        public Vec3Int StepCell(Vec3Int cell, IPVMapper.CellDir dir)
        {
            switch (dir)
            {
                case IPVMapper.CellDir.PlusX:
                    if (cell.X < Cells.X - 1)
                    {
                        return new Vec3Int(cell.X + 1, cell.Y, cell.Z);
                    }
                    break;

                case IPVMapper.CellDir.MinusX:
                    if (cell.X > 0)
                    {
                        return new Vec3Int(cell.X - 1, cell.Y, cell.Z);
                    }
                    break;

                case IPVMapper.CellDir.PlusY:
                    if (cell.Y < Cells.Y - 1)
                    {
                        return new Vec3Int(cell.X, cell.Y + 1, cell.Z);
                    }
                    break;

                case IPVMapper.CellDir.MinusY:
                    if (cell.Y > 0)
                    {
                        return new Vec3Int(cell.X, cell.Y - 1, cell.Z);
                    }
                    break;

                case IPVMapper.CellDir.PlusZ:
                    if (cell.Z < Cells.Z - 1)
                    {
                        return new Vec3Int(cell.X, cell.Y, cell.Z + 1);
                    }
                    break;

                case IPVMapper.CellDir.MinusZ:
                    if (cell.Z > 0)
                    {
                        return new Vec3Int(cell.X, cell.Y, cell.Z - 1);
                    }
                    break;
            }

            return null;
        }
        #endregion
    }
}
