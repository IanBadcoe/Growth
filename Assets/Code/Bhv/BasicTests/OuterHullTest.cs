using Growth.Behaviour;
using Growth.Util;
using Growth.Voronoi;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OuterHullTest : MonoBehaviour
{
    public GameObject MeshContainer;
    public GameObject MeshTemplate;

    public Material Material;

    public int RSeed;
    public int Points = 4;

    int LastRSeed = -1;
    int LastPoints;

    GameObject Obj;
    List<GameObject> Objs = new List<GameObject>();

    bool Wait = true;

    // Start is called before the first frame update
    void Start()
    {
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Wait = false;
        }

        if (Wait) {
            return; 
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            RSeed++;
        }

        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            Points++;
        }

        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            Points--;
        }

        if (RSeed == LastRSeed && Points == LastPoints)
            return;

        if (Obj != null)
        {
            Destroy(Obj);
        }  
        
        foreach(var o in Objs)
        {
            Destroy(o);
        }

        Objs.Clear();

        var random = new ClRand(RSeed);
        LastRSeed = RSeed;
        LastPoints = Points;

        var del = new Delaunay(1e-3f);
        var verts = new List<Vec3>();

        for (int j = 0; j < Points; j++)
        {
            var vert = random.Vec3() * 10 - new Vec3(5, 5, 5);

            verts.Add(vert);
        }

        DTetrahedron bound = del.GetBoundingTet(verts);

        del.InitialiseWithTet(bound.Verts.ToArray());

        foreach(var v in verts)
        {
            del.AddVert(v);
        }

        IVPolyhedron hull = del.OuterHull();

        Obj = GenerateMesh(hull, Material);

        foreach(var poly in del.Polyhedrons)
        {
            var o = GenerateMesh(poly, Material);
            Objs.Add(o);

            o.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        }
    }

    private GameObject GenerateMesh(IVPolyhedron poly, Material mat)
    {
        var mesh = MeshUtil.Polyhedron2Mesh(poly, poly.Centre);

        var go = Instantiate(MeshTemplate, MeshContainer.transform);

        go.GetComponent<MeshFilter>().mesh = mesh;
        go.GetComponent<MeshRenderer>().material = mat;
        go.transform.position = poly.Centre.ToVector3();

        return go;
    }
}
