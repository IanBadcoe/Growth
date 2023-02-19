using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Growth.Util;
using Growth.Voronoi;

namespace Growth.Voronoi.Mappers
{
    // Y is up the cylinder, X is round it, and Z is out from the centre
    //
    // MinRadius is the internal radius at Z = 0
    // MaxRadius is at Z = Cells.Z - 1
    class CylinderMapper : IPVMapper
    {
        readonly float MinRadius;
        readonly float MaxRadius;
        readonly float Height;
        readonly Vec3Int Cells;
        readonly Vec3 Perturbation;
        readonly ClRand Random;

        Dictionary<Vec3, Vec3Int> ReverseLookup { get; } = new Dictionary<Vec3, Vec3Int>();

        public CylinderMapper(float min_radius, float max_radius, float height, Vec3Int cells, Vec3 perturbation, ClRand random)
        {
            MinRadius = min_radius;
            MaxRadius = max_radius;
            Height = height;
            Cells = cells;
            Perturbation = perturbation;
            Random = random;
        }

        #region IPVMapper
        public Vec3 MakeVertForCell(Vec3Int cell)
        {
            var perturbed = new Vec3((cell.X + Random.FloatRange(-Perturbation.X, Perturbation.X) + 0.5f) * MathF.PI * 2 / Cells.X,
                (cell.Y + Random.FloatRange(-Perturbation.Y, Perturbation.Y) + 0.5f) * Height / Cells.Y,
                (cell.Z + Random.FloatRange(-Perturbation.Z, Perturbation.Z) + 0.5f) * (MaxRadius - MinRadius) / Cells.Z + MinRadius);

            var sk = MathF.Sin(perturbed.X);
            var ck = MathF.Cos(perturbed.X);


            var ret = new Vec3(sk * perturbed.Z, perturbed.Y, ck * perturbed.Z);

            ReverseLookup[ret] = cell;

            return ret;
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
            // except on X where we are cyclic
            return cell.X >= 0 && cell.X < Cells.X
                && cell.Y >= 1 && cell.Y < Cells.Y - 1
                && cell.Z >= 1 && cell.Z < Cells.Z - 1;
        }

        public IEnumerable<Vec3Int> AllGridNeighbours(Vec3Int pnt, IProgressiveVoronoi.Solidity permitted_for = IProgressiveVoronoi.Solidity.Vacuum)
        {
            foreach (var n in pnt.AllNeighbours)
            {
                var h_n = new Vec3Int(
                    (n.X + Cells.X) % Cells.X,
                    n.Y,
                    n.Z);

                if (InRange(h_n, permitted_for))
                {
                    yield return h_n;
                }
            }
        }

        public IEnumerable<Vec3Int> OrthoGridNeighbours(Vec3Int pnt, IProgressiveVoronoi.Solidity permitted_for = IProgressiveVoronoi.Solidity.Vacuum)
        {
            foreach (var n in pnt.OrthoNeighbours)
            {
                var h_n = new Vec3Int(
                    (n.X + Cells.X) % Cells.X,
                    n.Y,
                    n.Z);

                if (InRange(h_n, permitted_for))
                {
                    yield return h_n;
                }
            }
        }

        public Vec3Int Vert2Cell(Vec3 vert)
        {
            Vec3Int ret = null;

            ReverseLookup.TryGetValue(vert, out ret);

            return ret;
        }

        public VBounds Bounds()
        {
            return new VBounds(new Vec3(-MaxRadius, 0, -MaxRadius),
                new Vec3(MaxRadius, Height, MaxRadius));
        }

        public Vec3Int StepCell(Vec3Int cell, IPVMapper.CellDir dir)
        {
            switch (dir)
            {
                case IPVMapper.CellDir.PlusX:
                    return new Vec3Int((cell.X + 1) % Cells.X, cell.Y, cell.Z);                    
                case IPVMapper.CellDir.MinusX:
                    return new Vec3Int((cell.X + Cells.X - 1) % Cells.X, cell.Y, cell.Z);
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
