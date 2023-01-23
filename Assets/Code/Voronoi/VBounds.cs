using System.Collections.Generic;
using UnityEngine;

namespace Growth.Voronoi
{
    // the only purpose here is to make a Vec3 version of the wrapped UnityEngine.Bounds
    public class VBounds
    {
        Bounds Bounds;
        bool empty = true;

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
            Bounds.Expand(bound_extension);
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
    }
}
