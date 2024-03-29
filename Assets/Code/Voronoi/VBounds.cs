﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Growth.Voronoi
{
    // C# equality is a total mess, having a policy here of:
    // - never override operator ==/!=
    // -- so that x != null always means pure reference comparison
    // - never override Equals(object obj)
    // -- so we get type-safety on x.Equals(y)
    // - use T.Equals(T other) for _value_ compare
    // -- so it errors for null
    // -- which is the override for IEquatable<T> which at least some of the built-in containers use
    // - and override GetHashCode when you override that, also for containers...

    // immutable bounds object...
    [DebuggerDisplay("({Min.X}, {Min.Y}, {Min.Z}) -> ({Max.X}, {Max.Y}, {Max.Z})")]
    public class VBounds : IEquatable<VBounds>
    {
        public Vec3 Min { get; }
        public Vec3 Max { get; }

        public VBounds()
        {
            // we have exactly one representation of empty
            Min = new Vec3(1, 0, 0);
            Max = new Vec3(0, 0, 0);
        }

        public VBounds(Vec3 min, Vec3 max)
        {
            if (min.X > max.X || min.Y > max.Y || min.Z > max.Z)
            {
                // we have exactly one representation of empty
                Min = new Vec3(1, 0, 0);
                Max = new Vec3(0, 0, 0);
            }

            Min = min;
            Max = max;
        }

        public VBounds(Vec3 v)
        {
            // make the zero-sized bounds for a point...
            Min = Max = v;
        }

        public VBounds Encapsulating(Vec3 v)
        {
            var point_bound = new VBounds(v, v);
            if (IsEmpty)
            {
                return point_bound;
            }
            else
            {
                return this.UnionedWith(point_bound);
            }
        }

        public Vec3 Size => Max - Min;
        public Vec3 Centre => (Min + Max) / 2;

        public bool Contains(Vec3 pnt)
        {
            return pnt.X >= Min.X && pnt.X <= Max.X
                && pnt.Y >= Min.Y && pnt.Y <= Max.Y
                && pnt.Z >= Min.Z && pnt.Z <= Max.Z;
        }

        public VBounds ExpandedBy(float bound_extension)
        {
            var exp_vec = new Vec3(bound_extension, bound_extension, bound_extension);

            return new VBounds(Min - exp_vec, Max + exp_vec);
        }

        public IEnumerable<Vec3> Corners
        {
            get
            {
                yield return new Vec3(Min.X, Min.Y, Min.Z);
                yield return new Vec3(Min.X, Min.Y, Max.Z);
                yield return new Vec3(Min.X, Max.Y, Min.Z);
                yield return new Vec3(Min.X, Max.Y, Max.Z);
                yield return new Vec3(Max.X, Min.Y, Min.Z);
                yield return new Vec3(Max.X, Min.Y, Max.Z);
                yield return new Vec3(Max.X, Max.Y, Max.Z);
                yield return new Vec3(Max.X, Max.Y, Min.Z);
            }
        }

        public float Volume => Size.X * Size.Y * Size.Z;

        public bool IsEmpty => Min.X == 1 && Max.X == 0;

        public VBounds UnionedWith(VBounds other)
        {
            if (IsEmpty)
            {
                return other;
            }

            if (other.IsEmpty)
            {
                return this;
            }

            return new VBounds(Min.Min(other.Min), Max.Max(other.Max));
        }

        public bool Overlaps(VBounds b)
        {
            return !ClearOf(b);
        }

        public bool ClearOf(VBounds b)
        {
            return Min.X > b.Max.X
                || Min.Y > b.Max.Y
                || Min.Z > b.Max.Z
                || b.Min.X > Max.X
                || b.Min.Y > Max.Y
                || b.Min.Z > Max.Z;
        }

        #region IEquatable
        public bool Equals(VBounds other)
        {
            return Min.Equals(other.Min) && Max.Equals(other.Max);
        }
        #endregion

        public override int GetHashCode()
        {
            return Min.GetHashCode() + Max.GetHashCode() * 3;
        }
    }
}
