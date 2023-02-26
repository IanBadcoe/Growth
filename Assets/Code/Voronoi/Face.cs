using Growth.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Growth.Voronoi
{
    [DebuggerDisplay("Verts: {Verts.Count}")]
    public class Face : IEquatable<Face>
    {
        // approx normal is indicative of the hemisphere in which the real normal lies
        public Face(List<Vec3> verts, Vec3 approx_normal)
        {
            Vec3 actual_normal;
            // fix the rotation direction to match the normal
            switch (CalcRotationDirection(verts, approx_normal, out actual_normal))
            {
                case Face.RotationDirection.Clockwise:
                    break;

                case Face.RotationDirection.Anticlockwise:
                    verts.Reverse();
                    break;

                case Face.RotationDirection.Indeterminate:
                    MyAssert.IsTrue(false, "Usually means we got a degenerate or near degenerate face");
                    break;
            }

            Normal = actual_normal;

            Verts = FixPermute(verts);
        }

        private List<Vec3> FixPermute(List<Vec3> verts)
        {
            var first_vec = verts.Aggregate(verts.First(), (v1, v2) => v1.IsBefore(v2) ? v1 : v2);

            var ret = new List<Vec3>();

            int j = verts.IndexOf(first_vec);

            for (int i = 0; i < verts.Count; i++)
            {
                ret.Add(verts[j]);

                j = (j + 1) % verts.Count;
            }

            return ret;
        }

        public IReadOnlyList<Vec3> Verts;
        public Vec3 Normal { get; }
        public Vec3 Centre => Verts.Aggregate((v1, v2) => v1 + v2) / Verts.Count;

        public enum RotationDirection
        {
            Clockwise,
            Anticlockwise,
            Indeterminate
        }

        public static RotationDirection CalcRotationDirection(List<Vec3> verts, Vec3 approx_normal, out Vec3 actual_normal)
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

            var prev = verts[1] - verts[0];

            // as we are relative to Vert[0], all cross-products involving that disappear, so we start with
            // Verts[2] x Verts[1] and go up to Verts[N] x Verts[N-1]
            // e.g. for a triangle the only one left is 2,1 (losing 1,0 and 0,2)
            // and for a square re have 2,1 and 3,1
            Vec3 accum = new Vec3();
            for (int i = 2; i < verts.Count; i++)
            {
                var here = verts[i] - verts[0];
                accum += prev.Cross(here);

                prev = here;
            }

            // however, it is not the magnitude (area) of accum we want, but its direction
            //
            // accum is normal to the face such that viewing _down_ it the face looks anticlockwise

            // so if we get the projection on to that of a vector from our centre to any point in the face
            // that will be -ve for anticlockwise, positive for clockwise and close to zero if something is wrong
            // like a degenerate face or the centre being in the plane of the face
            Vec3 accum_normalised = accum.Normalised();

            var prod = approx_normal.Dot(accum_normalised);

            if (prod > 1e-6f)
            {
                actual_normal = accum_normalised;
                return RotationDirection.Clockwise;
            }

            if (prod < -1e-6f)
            {
                actual_normal = -accum_normalised;
                return RotationDirection.Anticlockwise;
            }

            actual_normal = null;
            return RotationDirection.Indeterminate;
        }

        public bool IsInside(Vec3 vert)
        {
            throw new NotImplementedException();
        }

        public Face Reversed()
        {
            return new Face(Verts.Reverse().ToList(), -Normal);
        }

        public bool Equals(Face other)
        {
            if (!Normal.Equals(other.Normal)
                || Verts.Count != other.Verts.Count)
            {
                return false;
            }

            for(int i = 0; i < Verts.Count; i++)
            {
                if (!Verts[i].Equals(other.Verts[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            int ret = Normal.GetHashCode();

            foreach(var vert in Verts)
            {
                ret = ret * 13 + vert.GetHashCode();
            }

            return ret;
        }
    }
}
