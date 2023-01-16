using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Code.Voronoi;
using UnityEngine;

namespace Assets.Code
{
    public class VoronoiTestBhv : MonoBehaviour
    {
        public GameObject MeshContainer;
        public GameObject MeshTemplate;
        public List<Material> Materials;

        // Use this for initialization
        void Start()
        {
            //DTetrahedron tet = new DTetrahedron(
            //    new Vec3(0, 0, 0),
            //    new Vec3(1, 0, 0),
            //    new Vec3(0, 1, 0),
            //    new Vec3(0, 0, 1));

            //GenerateMesh(tet.ToPolyhedron(), Materials[0]);

            Voronoi.VoronoiUtil vu = new Voronoi.VoronoiUtil();

            //IDelaunay d = vu.CreateDelaunay(new Vec3[]
            //{
            //    new Vec3(-0.5f, -0.5f, -0.5f),
            //    new Vec3(-0.5f, -0.5f,  0.5f),
            //    new Vec3(-0.5f,  0.5f, -0.5f),
            //    new Vec3(-0.5f,  0.5f,  0.5f),
            //    new Vec3( 0.5f, -0.5f, -0.5f),
            //    new Vec3( 0.5f, -0.5f,  0.5f),
            //    new Vec3( 0.5f,  0.5f, -0.5f),
            //    new Vec3( 0.5f,  0.5f,  0.5f),
            //    new Vec3( 0,     0,     0   ),
            //});

            //GenerateMeshes(d);

            IVoronoi v = vu.CreateBoundedVoronoi(new Vec3[]
            {
                new Vec3 (0, 0, 0),
                new Vec3 (1, 0, 0),
                new Vec3 (0, 1, 0),
                new Vec3 (0, 0, 1),
                new Vec3 (1, 1, 0),
                new Vec3 (1, 0, 1),
                new Vec3 (0, 1, 1),
                new Vec3 (1, 1, 1),
            }, 1);

            GenerateMeshes(v);
        }

        private void GenerateMeshes(IPolyhedronSet ps)
        {
            int i = 0;
            foreach(var poly in ps.Polyhedrons)
            {
                var mat = Materials[i];
                i = (i + 1) % Materials.Count();

                GenerateMesh(poly, mat);
            }
        }

        private void GenerateMesh(IVPolyhedron poly, Material mat)
        {
            var mesh = new Mesh();

            Vector3[] verts = poly.Verts.Select(v => v.ToVector3()).ToArray();
            mesh.vertices = verts;

            List<int> tris = new List<int>();

            var v3_verts = poly.Verts.ToArray();

            foreach(var face in poly.Faces)
            {
                List<int> vert_idxs = face.Verts.Select(v => Array.IndexOf(v3_verts, v)).ToList();

                for(int i = 1; i < vert_idxs.Count - 1; i++)
                {
                    tris.AddRange(new int[]{ vert_idxs[0], vert_idxs[i], vert_idxs[i + 1]});
                }
            }

            mesh.triangles = tris.ToArray();

            var go = Instantiate(MeshTemplate, MeshContainer.transform);

            go.GetComponent<MeshFilter>().mesh = mesh;
            go.GetComponent<MeshRenderer>().material = mat;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}