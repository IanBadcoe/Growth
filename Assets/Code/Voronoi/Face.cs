using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Growth.Voronoi
{
    [DebuggerDisplay("Verts: {Verts.Count}")]
    public class Face
    {
        public Face(List<Vec3> verts)
        {
            Verts = verts;
        }

        public IReadOnlyList<Vec3> Verts;

        public enum RotationDirection
        {
            Clockwise,
            Anticlockwise,
            Indeterminate
        }

        public RotationDirection CalcRotationDirection(Vec3 from_centre)
        {
            var prev = Verts.Last();

            Vec3 accum = new Vec3();
            foreach(var v in Verts)
            {
                accum += prev.Cross(v);

                prev = v;
            }

            // accum is normal to the face such that viewing _down_ it the face looks anticlockwise

            // so if we get the projection on to that of a vector from our centre to any point in the face
            // that will be -ve for anticlockwise, positive for clockwise and close to zero if something is wrong
            // like a degenerate face or the centre being in the plane of the face
            var prod = (Verts.First() - from_centre).Dot(accum);

            if (prod > 1e-6f)
            {
                return RotationDirection.Clockwise;
            }

            if (prod < -1e-6f)
            {
                return RotationDirection.Anticlockwise;
            }

            return RotationDirection.Indeterminate;
        }

        internal Face Reversed()
        {
            return new Face(Verts.Reverse().ToList());
        }
    }
}
