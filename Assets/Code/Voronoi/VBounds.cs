﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Growth.Voronoi
{
    // the only purpose here is to make a Vec3 version of the wrapped UnityEngine.Bounds
    public class VBounds
    {
        Bounds Bounds;
        bool empty = true;

        public VBounds()
        {

        }

        public VBounds(Vec3 min, Vec3 max)
        {
            Bounds = new Bounds(((min + max) / 2).ToVector3(), (max - min).ToVector3());
            empty = false;
        }

        public void Encapsulate(Vec3 v)
        {
            if (empty)
            {
                Bounds = new Bounds(v.ToVector3(), Vector3.zero);
                empty = false;
            }
            else
            {
                Bounds.Encapsulate(v.ToVector3());
            }
        }

        public Vec3 Min => new Vec3(Bounds.min);
        public Vec3 Max => new Vec3(Bounds.max);
        public Vec3 Size => new Vec3(Bounds.size);
        public Vec3 Centre => new Vec3(Bounds.center);
        public bool Contains(Vec3 pnt)
        {
            return Bounds.Contains(pnt.ToVector3());
        }

        public void Expand(float bound_extension)
        {
            // Unity put half on either end
            Bounds.Expand(bound_extension * 2);
        }

        public IEnumerable<Vec3> Corners {
            get
            {
                yield return new Vec3(Bounds.min.x, Bounds.min.y, Bounds.min.z);
                yield return new Vec3(Bounds.min.x, Bounds.min.y, Bounds.max.z);
                yield return new Vec3(Bounds.min.x, Bounds.max.y, Bounds.min.z);
                yield return new Vec3(Bounds.min.x, Bounds.max.y, Bounds.max.z);
                yield return new Vec3(Bounds.max.x, Bounds.min.y, Bounds.min.z);
                yield return new Vec3(Bounds.max.x, Bounds.min.y, Bounds.max.z);
                yield return new Vec3(Bounds.max.x, Bounds.max.y, Bounds.max.z);
                yield return new Vec3(Bounds.max.x, Bounds.max.y, Bounds.min.z);
            }
        }

        public float Volume => Size.X * Size.Y * Size.Z;

        public VBounds UnionedWith(VBounds other)
        {
            if (empty)
            {
                return other;
            }
            
            if (other.empty)
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
    }
}
