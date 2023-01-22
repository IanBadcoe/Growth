using UnityEngine;

namespace Growth.Voronoi
{
    // the only purpose here is to make an immutable version of the wrapped UnityEngine.Bounds
    //
    // only exposing the exact set of members from that which currently need, will add more as required
    //
    // may add the odd custom capability in time, if that makes sense
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

        public void Expand(float bound_extension)
        {
            Bounds.Expand(bound_extension);
        }
    }
}
