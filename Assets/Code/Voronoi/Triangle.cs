using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Growth.Voronoi
{
    [DebuggerDisplay("({V1.X}, {V1.Y}, {V1.Z}) ({V2.X}, {V2.Y}, {V2.Z}) ({V3.X}, {V3.Y}, {V3.Z})")]
    public class Triangle
    {
        public Triangle(Vec3 p1, Vec3 p2, Vec3 p3)
        {
            V1 = p1;
            V2 = p2;
            V3 = p3;
        }

        public readonly Vec3 V1;
        public readonly Vec3 V2;
        public readonly Vec3 V3;

        public IEnumerable<Vec3> Verts
        {
            get
            {
                yield return V1;
                yield return V2;
                yield return V3;
            }
        }

        public Vec3 Centre => Verts.Aggregate((v1, v2) => v1 + v2) / 3;

        public Face ToFace(Vec3 normal)
        {
            return new Face(Verts.ToList(), normal);
        }
    }
}
