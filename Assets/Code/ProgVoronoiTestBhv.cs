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
        int MaterialIdx = 0;

        public int RSeed;

        int done = 0;

        IProgressiveVoronoi v = null;

        Dictionary<Mesh, GameObject> InstantiatedMeshes = new Dictionary<Mesh, GameObject>();

        // Use this for initialization
        void Update()
        {
            if (done == 0 && Input.GetKey(KeyCode.Space))
            {
                v = VoronoiUtil.CreateProgressiveVoronoi(10, 1e-3f, new ClRand(RSeed));

                v.AddPoint(new Vec3Int(5, 5, 6));

                ProgressiveInstantiateMeshes(v);

                done = 1;
            }
            else if (done == 1 && Input.GetKey(KeyCode.Space))
            {
                v.AddPoint(new Vec3Int(5, 5, 7));

                ProgressiveInstantiateMeshes(v);

                done = 2;
            }
        }

        private void ProgressiveInstantiateMeshes(IProgressiveVoronoi ps)
        {
            foreach (var mesh in ps.AllPoints
                .Where(p => p.Mesh != null)
                .Where(p => !InstantiatedMeshes.ContainsKey(p.Mesh))
                .Select(p => p.Mesh))
            {
                var mat = Materials[MaterialIdx];
                MaterialIdx = (MaterialIdx + 1) % Materials.Count();

                InstantiatedMeshes[mesh] = InstantiateMesh(mesh, mat);
            }
        }

        private GameObject InstantiateMesh(Mesh mesh, Material mat)
        {
            var go = Instantiate(MeshTemplate, MeshContainer.transform);

            go.GetComponent<MeshFilter>().mesh = mesh;
            go.GetComponent<MeshRenderer>().material = mat;

            return go;
        }
    }

}