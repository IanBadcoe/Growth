using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Growth.Util;
using Growth.Voronoi;

namespace Growth.Voronoi.Mappers
{
    class CubeMapper : IPVMapper
    {
        readonly int Size;
        readonly float Perturbation;
        readonly ClRand Random;

        public CubeMapper(int size, ClRand random, float perturbation)
        {
            Size = size;
            Random = random;
            Perturbation = perturbation;
        }

        #region IPVMapper
        public Vec3 MakeVertForCell(Vec3Int cell)
        {
            // our point is in the centre of the cell +/- a randomisation
            return new Vec3(
                cell.X + Random.FloatRange(-Perturbation, Perturbation) + 0.5f,
                cell.Y + Random.FloatRange(-Perturbation, Perturbation) + 0.5f,
                cell.Z + Random.FloatRange(-Perturbation, Perturbation) + 0.5f);
        }

        public bool InRange(Vec3Int cell, IProgressiveVoronoi.Solidity permitted_for)
        {
            MyAssert.IsTrue(permitted_for != IProgressiveVoronoi.Solidity.Unknown, "Asking InRange question about unknown solidity");

            // vacuum points are allowed all the way to the edge
            if (permitted_for == IProgressiveVoronoi.Solidity.Vacuum)
            {
                return cell.X >= 0 && cell.X < Size
                    && cell.Y >= 0 && cell.Y < Size
                    && cell.Z >= 0 && cell.Z < Size;
            }

            // solid points must have room for a vacuum point next to them...
            return cell.X >= 1 && cell.X < Size - 1
                && cell.Y >= 1 && cell.Y < Size - 1
                && cell.Z >= 1 && cell.Z < Size - 1;
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
            return vert.ToVec3Int();
        }

        public VBounds Bounds()
        {
            return new VBounds(new Vec3(0, 0, 0), new Vec3(Size, Size, Size));
        }
        #endregion

        public Vec3Int StepCell(Vec3Int cell, IPVMapper.CellDir dir)
        {
            switch (dir)
            {
                case IPVMapper.CellDir.PlusX:
                    if (cell.X < Size - 1)
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
                    if (cell.Y < Size - 1)
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
                    if (cell.Z < Size - 1)
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
    }
}
