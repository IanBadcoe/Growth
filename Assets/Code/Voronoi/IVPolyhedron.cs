using Growth.Util;
using System.Collections.Generic;
using UnityEngine;

namespace Growth.Voronoi
{
    public interface IVPolyhedron
    {
        enum MeshType
        {
            Unknown,
            Smooth,
            Faces,

            // may add more types later...
        }

        IEnumerable<Face> Faces { get; }
        IEnumerable<Vec3> Verts { get; }
        Vec3 Centre { get; }
        MeshType Type { get; }

        Face GetFaceByKey(object key);
    }
}
