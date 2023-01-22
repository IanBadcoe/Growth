using System;
using System.Collections.Generic;
using System.Linq;
using Growth.Voronoi;
using UnityEngine;
using Growth.Util;

namespace Growth
{

public class ProgVoronoiTestBhv : MonoBehaviour
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
            IProgressiveVoronoi v = VoronoiUtil.CreateProgressiveVoronoi(10, 1e-3f);

            //GenerateMeshes(v);

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