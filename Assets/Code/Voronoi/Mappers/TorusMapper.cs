using Growth.Util;
using System;
using System.Collections.Generic;

namespace Growth.Voronoi.Mappers
{
    // Y is out from the torus, with a minimum of MinorRadius, and Max of (MajorRadius - MinorRadius) / 2 (to leave space in the middle)
    // X is round the major radius, Z is round the minor radius
    //
    class TorusMapper : IPVMapper
    {
        readonly float MinorRadius;
        readonly float MajorRadius;
        readonly Vec3Int Cells;
        readonly Vec3 Perturbation;
        readonly ClRand Random;

        Dictionary<Vec3, Vec3Int> ReverseLookup { get; } = new Dictionary<Vec3, Vec3Int>();

        public TorusMapper(float maj_radius, float min_radius, Vec3Int cells, Vec3 perturbation, ClRand random)
        {
            MinorRadius = min_radius;
            MajorRadius = maj_radius;
            Cells = cells;
            Perturbation = perturbation;
            Random = random;
        }

        #region IPVMapper
        public Vec3 MakeVertForCell(Vec3Int cell)
        {
            var perturbed = new Vec3(
                (cell.X + Random.FloatRange(-Perturbation.X, Perturbation.X) + 0.5f) * MathF.PI * 2 / Cells.X,
                (cell.Y + Random.FloatRange(-Perturbation.Y, Perturbation.Y) + 0.5f) * (MajorRadius - MinorRadius) / 2 / Cells.Y + MinorRadius,
                (cell.Z + Random.FloatRange(-Perturbation.Z, Perturbation.Z) + 0.5f) * MathF.PI * 2 / Cells.Z);

            var sx = MathF.Sin(perturbed.X);
            var cx = MathF.Cos(perturbed.X);

            var sz = MathF.Sin(perturbed.Z);
            var cz = MathF.Cos(perturbed.Z);


            var ret = new Vec3(
                sx * (MajorRadius + perturbed.Y * sz),
                perturbed.Y * cz,
                cx * (MajorRadius + perturbed.Y * sz));

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
            // except on X and Z where we are cyclic
            return cell.X >= 0 && cell.X < Cells.X
                && cell.Y >= 1 && cell.Y < Cells.Y - 1
                && cell.Z >= 0 && cell.Z < Cells.Z;
        }

        public IEnumerable<Vec3Int> AllGridNeighbours(Vec3Int pnt, IProgressiveVoronoi.Solidity permitted_for = IProgressiveVoronoi.Solidity.Vacuum)
        {
            foreach (var n in pnt.AllNeighbours)
            {
                var h_n = new Vec3Int(
                    (n.X + Cells.X) % Cells.X,
                    n.Y,
                    (n.Z + Cells.Z) % Cells.Z);

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
                    (n.Z + Cells.Z) % Cells.Z);

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
            float YCombinedRadii = MinorRadius + (MajorRadius - MinorRadius) / 2;
            float XZCombinedRadii = MajorRadius + YCombinedRadii;

            return new VBounds(
                new Vec3(XZCombinedRadii, YCombinedRadii, XZCombinedRadii),
                new Vec3(-XZCombinedRadii, -YCombinedRadii, -XZCombinedRadii));
        }

        public Vec3Int StepCell(Vec3Int cell, IPVMapper.CellDir dir, IProgressiveVoronoi.Solidity permitted_for)
        {
            Vec3Int ret = null;

            switch (dir)
            {
                case IPVMapper.CellDir.PlusX:
                    ret = new Vec3Int((cell.X + 1) % Cells.X, cell.Y, cell.Z);
                    break;

                case IPVMapper.CellDir.MinusX:
                    ret = new Vec3Int((cell.X + Cells.X - 1) % Cells.X, cell.Y, cell.Z);
                    break;

                case IPVMapper.CellDir.PlusY:
                    ret = new Vec3Int(cell.X, cell.Y + 1, cell.Z);
                    break;

                case IPVMapper.CellDir.MinusY:
                    ret = new Vec3Int(cell.X, cell.Y - 1, cell.Z);
                    break;

                case IPVMapper.CellDir.PlusZ:
                    ret = new Vec3Int(cell.X, cell.Y, (cell.Z + 1) % Cells.Z);
                    break;


                case IPVMapper.CellDir.MinusZ:
                    ret = new Vec3Int(cell.X, cell.Y, (cell.Z + Cells.Z - 1) % Cells.Z);
                    break;

            }

            if (InRange(ret, permitted_for))
            {
                return ret;
            }

            return null;
        }
        #endregion
    }
}
