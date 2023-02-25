using Growth.Voronoi;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TetTestBhv : MonoBehaviour
{
    public GameObject MeshContainer;
    public GameObject MeshTemplate;
    public Mesh SphereMesh;
    public List<Material> Materials;
    public float Size;

    // Start is called before the first frame update
    void Start()
    {
        float half_size = Size / 2;

        float sphere_radius = Mathf.Sqrt(half_size * half_size * 3);

        float Q = 2f / 9f * 3f / 4f / Mathf.Sqrt(2f / 3f);

        float tet_size = sphere_radius / Q;

        // tet_size is the edge length of the tet

        // tet corners are:
        //
        // lower face in XZ plane:
        // a = (-1/2,  -Q,             -1/3 sqrt(3/4))
        // b = (+1/2,  -Q,             -1/3 sqrt(3/4))
        // c = ( 0,    -Q,             +2/3 sqrt(3/4))
        //
        // apex on y axis:
        // d = ( 0,     sqrt(2/3) - Q,  0            )

        Vector3 a = new Vector3(-1f / 2, -Q, -1f / 3 * Mathf.Sqrt(3f / 4));
        Vector3 b = new Vector3(+1f / 2, -Q, -1f / 3 * Mathf.Sqrt(3f / 4));
        Vector3 c = new Vector3(0, -Q, 2f / 3 * Mathf.Sqrt(3f / 4));
        Vector3 d = new Vector3(0, Mathf.Sqrt(2f / 3) - Q, 0);

        a *= tet_size;
        b *= tet_size;
        c *= tet_size;
        d *= tet_size;

        DTetrahedron dt = new DTetrahedron(new Vec3(a), new Vec3(b), new Vec3(c), new Vec3(d));

        //IDelaunay del = VoronoiUtil.CreateDelaunay(new Vec3[] {
        //    new Vec3(a), new Vec3(b), new Vec3(c), new Vec3(d)
        //});

        GenerateMesh(dt.ToPolyhedron(), Materials[1]);

        var go = Instantiate(MeshTemplate, MeshContainer.transform);

        go.GetComponent<MeshFilter>().mesh = SphereMesh;
        // looks like Unity's standard sphere is diameter 1, not radius 1...
        go.transform.localScale = new Vector3(sphere_radius * 2, sphere_radius * 2, sphere_radius * 2);
        go.GetComponent<MeshRenderer>().material = Materials[1];

        GenerateMesh(VPolyhedron.Cube(Size), Materials[2]);
    }

    private void GenerateMeshes(IPolyhedronSet ps)
    {
        int i = 0;
        foreach (var poly in ps.Polyhedrons)
        {
            var mat = Materials[i];
            i = (i + 1) % Materials.Count;

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

        foreach (var face in poly.Faces)
        {
            List<int> vert_idxs = face.Verts.Select(v => Array.IndexOf(v3_verts, v)).ToList();

            for (int i = 1; i < vert_idxs.Count - 1; i++)
            {
                tris.AddRange(new int[] { vert_idxs[0], vert_idxs[i], vert_idxs[i + 1] });
            }
        }

        mesh.triangles = tris.ToArray();

        var go = Instantiate(MeshTemplate, MeshContainer.transform);

        go.GetComponent<MeshFilter>().mesh = mesh;
        go.GetComponent<MeshRenderer>().material = mat;
    }
}
