using Growth.Voronoi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Growth.Util
{
    public class MeshUtil
    {
        // points are translated to be relative to origin,
        // so that when we instantiate this we set its position equal to that
        // and everything lands in the right position
        // *BUT* the cell position can be read off the GameObject transform
        public static Mesh Polyhedron2Mesh(IVPolyhedron poly, Vec3 origin)
        {
            var mesh = new Mesh();

            Vector3[] verts = poly.Verts.Select(v => (v - origin).ToVector3()).ToArray();
            mesh.vertices = verts;

            List<int> tris = new List<int>();

            var v3_verts = poly.Verts.ToArray();

            foreach (var face in poly.Faces)
            {
                List<int> vert_idxs = face.Verts.Select(v => Array.IndexOf(v3_verts, v)).ToList();

                for (int i = 1; i < vert_idxs.Count - 1; i++)
                {
                    tris.AddRange(new int[] { vert_idxs[0], vert_idxs[i], vert_idxs[i + 1] });
                }
            }

            mesh.triangles = tris.ToArray();

            return mesh;
        }

        //static Mesh FaceMesh(IVPolyhedron poly, Vec3 origin)
        //{
        //    var mesh = new Mesh();

        //    Vector3[] verts = poly.Verts.Select(v => (v - origin).ToVector3()).ToArray();
        //    mesh.vertices = verts;

        //    List<int> tris = new List<int>();

        //    var v3_verts = poly.Verts.ToArray();

        //    List<Vector3> duplicated_verts = new List<Vector3>();
        //    List<Vector3> normals = new List<Vector3>();

        //    foreach (var face in poly.Faces)
        //    {
        //        int vert_base = duplicated_verts.Count;

        //        int num_verts = face.Verts.Count;
        //        normals.AddRange(Enumerable.Repeat(face.Normal.ToVector3(), num_verts));
        //        duplicated_verts.AddRange(face.Verts.Select(v => (v - origin).ToVector3()));

        //        for (int i = 1; i < num_verts - 1; i++)
        //        {
        //            tris.AddRange(new int[] { vert_base, vert_base + i, vert_base + i + 1 });
        //        }
        //    }

        //    mesh.vertices = duplicated_verts.ToArray();
        //    mesh.normals = normals.ToArray();
        //    mesh.triangles = tris.ToArray();

        //    return mesh;
        //}
    }
}
