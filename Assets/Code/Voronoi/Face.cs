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
            // this works by calculating 2* the signed area of the polygon...
            //
            // running round the polygon, summing v(n - 1).Cross(v(n)) gives us a vector whose length is 2x the area
            // (because the cross product of two vectors is the area of the parallelogram they define, and the summation of those parallelograms
            //  +ve and -ve gives us an area independent of where the coordinate system is centred (because the poly is a closed loop)
            //  so if we imagine the case where the centre is in the middle of the triangle:
            //
            //
            //
            //
            //                   a
            //               .  /|\  .
            //            e    / | \    d
            //            |   /  |  \   |
            //            |  /   |   \  |
            //            | /    o    \ |
            //            |/  .     .  \|
            //            b_____________c
            //                .     .
            //                   f
            //
            // then the hexagon adcgbe has twice the area of the triangle abc (and moving o does not change that, although when o leaves
            // the triangle some of the areas go -ve...
            //
            // HOWEVER to make the calculatio easier, we move o on top of our first point, this makes one of the
            // vectors zero length, so two of the cross-products disappear
            //
            // ALSO, by making all the vectors relative, we avoid any precision problems with cross-products of very long vectors
            // that produce very large results which we have to subract from each other and arrive at a relatively very small number...

            var prev = Verts[1] - Verts[0];

            // as we are relative to Vert[0], all cross-products involving that disappear, so we start with
            // Verts[2] x Verts[1] and go up to Verts[N] x Verts[N-1]
            // e.g. for a triangle the only one left is 2,1 (losing 1,0 and 0,2)
            // and for a square re have 2,1 and 3,1
            Vec3 accum = new Vec3();
            for (int i = 2; i < Verts.Count; i++)
            {
                var here = Verts[i] - Verts[0];
                accum += prev.Cross(here);

                prev = here;
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
