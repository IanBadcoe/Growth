using Growth.Util;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Growth.Voronoi
{
    [DebuggerDisplay("Faces: {Faces.Count} Verts {Verts.Count}")]
    public class VPolyhedron : IVPolyhedron
    {
        public VPolyhedron(Vec3 centre)
        {
            Centre = centre;
            FacesRW = new List<Face>();
        }

        List<Face> FacesRW;

        public IReadOnlyList<Face> Faces => FacesRW;
        public IReadOnlyList<Vec3> Verts => Faces.SelectMany(f => f.Verts).Distinct().ToList();
        public Vec3 Centre { get; }

        public void AddFace(Face face)
        {
            switch (face.CalcRotationDirection(Centre))
            {
                case Face.RotationDirection.Clockwise:
                    FacesRW.Add(face);
                    break;

                case Face.RotationDirection.Anticlockwise:
                    FacesRW.Add(face.Reversed());
                    break;

                case Face.RotationDirection.Indeterminate:
                    MyAssert.IsTrue(false, "Usually means we got a degenerate or near degenerate face");
                    break;
            }
        }

        public static VPolyhedron Cube(float size)
        {
            var ret = new VPolyhedron(new Vec3(0, 0, 0));

            float hs = size / 2;

            ret.AddFace(new Face(new List<Vec3>
            {
                new Vec3(-hs, -hs, -hs),
                new Vec3( hs, -hs, -hs),
                new Vec3( hs,  hs, -hs),
                new Vec3(-hs,  hs, -hs),
            }));

            ret.AddFace(new Face(new List<Vec3>
            {
                new Vec3(-hs, -hs,  hs),
                new Vec3( hs, -hs,  hs),
                new Vec3( hs,  hs,  hs),
                new Vec3(-hs,  hs,  hs),
            }));

            ret.AddFace(new Face(new List<Vec3>
            {
                new Vec3(-hs, -hs, -hs),
                new Vec3( hs, -hs, -hs),
                new Vec3( hs, -hs,  hs),
                new Vec3(-hs, -hs,  hs),
            }));

            ret.AddFace(new Face(new List<Vec3>
            {
                new Vec3(-hs,  hs, -hs),
                new Vec3( hs,  hs, -hs),
                new Vec3( hs,  hs,  hs),
                new Vec3(-hs,  hs,  hs),
            }));

            ret.AddFace(new Face(new List<Vec3>
            {
                new Vec3(-hs, -hs, -hs),
                new Vec3(-hs,  hs, -hs),
                new Vec3(-hs,  hs,  hs),
                new Vec3(-hs, -hs,  hs),
            }));

            ret.AddFace(new Face(new List<Vec3>
            {
                new Vec3( hs, -hs, -hs),
                new Vec3( hs,  hs, -hs),
                new Vec3( hs,  hs,  hs),
                new Vec3( hs, -hs,  hs),
            }));

            return ret;
        }
    }
}
