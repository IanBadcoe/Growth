using Growth.Util;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Growth.Voronoi
{
    [DebuggerDisplay("Faces: {Faces.Count} Verts {Verts.Count}")]
    public class VPolyhedron : IVPolyhedron
    {
        public VPolyhedron(Vec3 centre, IVPolyhedron.MeshType type)
        {
            Centre = centre;
            FacesRW = new List<Face>();
            Type = type;
        }

        List<Face> FacesRW;

        #region IVPolyhedron
        public IReadOnlyList<Face> Faces => FacesRW;
        public IReadOnlyList<Vec3> Verts => Faces.SelectMany(f => f.Verts).Distinct().ToList();
        public Vec3 Centre { get; }
        public IVPolyhedron.MeshType Type { get; }
        #endregion

        public void AddFace(Face face)
        {
            MyAssert.IsTrue(face.Normal.Dot((face.Centre - Centre).Normalised()) > 0,
                "Face's idea of its normal not pointing away from polygon centre");

            FacesRW.Add(face);
        }

        public static VPolyhedron Cube(float size)
        {
            var ret = new VPolyhedron(new Vec3(0, 0, 0), IVPolyhedron.MeshType.Faces);

            float hs = size / 2;

            ret.AddFace(new Face(new List<Vec3>
            {
                new Vec3(-hs, -hs, -hs),
                new Vec3( hs, -hs, -hs),
                new Vec3( hs,  hs, -hs),
                new Vec3(-hs,  hs, -hs),
            }, new Vec3(0, 0, -1)));

            ret.AddFace(new Face(new List<Vec3>
            {
                new Vec3(-hs, -hs,  hs),
                new Vec3( hs, -hs,  hs),
                new Vec3( hs,  hs,  hs),
                new Vec3(-hs,  hs,  hs),
            }, new Vec3(0, 0, 1)));

            ret.AddFace(new Face(new List<Vec3>
            {
                new Vec3(-hs, -hs, -hs),
                new Vec3( hs, -hs, -hs),
                new Vec3( hs, -hs,  hs),
                new Vec3(-hs, -hs,  hs),
            }, new Vec3(0, -1, 0)));

            ret.AddFace(new Face(new List<Vec3>
            {
                new Vec3(-hs,  hs, -hs),
                new Vec3( hs,  hs, -hs),
                new Vec3( hs,  hs,  hs),
                new Vec3(-hs,  hs,  hs),
            }, new Vec3(0, 1, 0)));

            ret.AddFace(new Face(new List<Vec3>
            {
                new Vec3(-hs, -hs, -hs),
                new Vec3(-hs,  hs, -hs),
                new Vec3(-hs,  hs,  hs),
                new Vec3(-hs, -hs,  hs),
            }, new Vec3(-1, 0, 0)));

            ret.AddFace(new Face(new List<Vec3>
            {
                new Vec3( hs, -hs, -hs),
                new Vec3( hs,  hs, -hs),
                new Vec3( hs,  hs,  hs),
                new Vec3( hs, -hs,  hs),
            }, new Vec3(-1, 0, 0)));

            return ret;
        }
    }
}
