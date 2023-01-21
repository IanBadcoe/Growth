using System;
using System.Collections.Generic;
using System.Linq;
using Growth.Voronoi;
using UnityEngine;
using Growth.Util;

namespace Growth
{

public class VoronoiTestBhv : MonoBehaviour
{
    public GameObject MeshContainer;
    public GameObject MeshTemplate;
    public List<Material> Materials;

    bool done = false;
    // Use this for initialization
    void Update()
    {
        if (!done && Input.GetKey(KeyCode.Space))
        {
            //DTetrahedron tet = new DTetrahedron(
            //    new Vec3(0, 0, 0),
            //    new Vec3(1, 0, 0),
            //    new Vec3(0, 1, 0),
            //    new Vec3(0, 0, 1));

            //GenerateMesh(tet.ToPolyhedron(), Materials[0]);

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

            var start = Time.realtimeSinceStartup;

            Voronoi.VoronoiUtil vu = new Voronoi.VoronoiUtil();

            ClRand rand = new ClRand(123);

            List<Vec3> points = new List<Vec3>();

            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    for (int k = 0; k < 6; k++)
                    {
                        points.Add(new Vec3(
                            i + rand.FloatRange(-1.0f / 6, 1.0f / 6),
                            j + rand.FloatRange(-1.0f / 6, 1.0f / 6),
                            k + rand.FloatRange(-1.0f / 6, 1.0f / 6)
                        ));
                    }
                }
            }

            IVoronoi v = vu.CreateBoundedVoronoi(points, 1);

            var mid = Time.realtimeSinceStartup;

            GenerateMeshes(v);

            var end = Time.realtimeSinceStartup;

            var first = mid - start;
            var second = end - mid;
            var total = end - start;

            var line = $"First half: {first}\nSecond half: {second}\nTotal: {total}";

            System.Diagnostics.Debug.WriteLine(line);

            done = true;
        }
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
}

}