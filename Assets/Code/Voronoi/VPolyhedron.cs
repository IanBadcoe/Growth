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
            Type = type;
        }

        // faces can be added with a key or without, our total faces are the union of the two sets
        List<Face> FacesRW = new List<Face>();
        Dictionary<object, Face> FacesMapRW = new Dictionary<object, Face>();

        #region IVPolyhedron
        public IEnumerable<Face> Faces => FacesRW.Concat(FacesMapRW.Values);
        public IEnumerable<Vec3> Verts => Faces.SelectMany(f => f.Verts).Distinct();
        public Vec3 Centre { get; }
        public IVPolyhedron.MeshType Type { get; set; }

        public Face GetFaceByKey(object key)
        {
            Face ret = null;

            FacesMapRW.TryGetValue(key, out ret);

            return ret;
        }
        #endregion

        public void AddFace(object key, Face face)
        {
            MyAssert.IsTrue(face.Normal.Dot((face.Centre - Centre).Normalised()) > 0,
                "Face's idea of its normal not pointing away from polygon centre");

            if (key != null)
            {
                FacesMapRW[key] = face;
            }
            else
            {
                FacesRW.Add(face);
            }
        }

        public static VPolyhedron Cube(float size)
        {
            var ret = new VPolyhedron(new Vec3(0, 0, 0), IVPolyhedron.MeshType.Faces);

            float hs = size / 2;

            ret.AddFace(null, new Face(new List<Vec3>
            {
                new Vec3(-hs, -hs, -hs),
                new Vec3( hs, -hs, -hs),
                new Vec3( hs,  hs, -hs),
                new Vec3(-hs,  hs, -hs),
            }, new Vec3(0, 0, -1)));

            ret.AddFace(null, new Face(new List<Vec3>
            {
                new Vec3(-hs, -hs,  hs),
                new Vec3( hs, -hs,  hs),
                new Vec3( hs,  hs,  hs),
                new Vec3(-hs,  hs,  hs),
            }, new Vec3(0, 0, 1)));

            ret.AddFace(null, new Face(new List<Vec3>
            {
                new Vec3(-hs, -hs, -hs),
                new Vec3( hs, -hs, -hs),
                new Vec3( hs, -hs,  hs),
                new Vec3(-hs, -hs,  hs),
            }, new Vec3(0, -1, 0)));

            ret.AddFace(null, new Face(new List<Vec3>
            {
                new Vec3(-hs,  hs, -hs),
                new Vec3( hs,  hs, -hs),
                new Vec3( hs,  hs,  hs),
                new Vec3(-hs,  hs,  hs),
            }, new Vec3(0, 1, 0)));

            ret.AddFace(null, new Face(new List<Vec3>
            {
                new Vec3(-hs, -hs, -hs),
                new Vec3(-hs,  hs, -hs),
                new Vec3(-hs,  hs,  hs),
                new Vec3(-hs, -hs,  hs),
            }, new Vec3(-1, 0, 0)));

            ret.AddFace(null, new Face(new List<Vec3>
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
