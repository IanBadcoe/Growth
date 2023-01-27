using Growth.Voronoi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Growth.Util
{
    class MeshUtil
    {
        public static Mesh Polyhedron2Mesh(IVPolyhedron poly)
        {
            var mesh = new Mesh();

            Vector3[] verts = poly.Verts.Select(v => v.ToVector3()).ToArray();
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
    }
}
