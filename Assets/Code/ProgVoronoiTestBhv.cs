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

        bool running = false;

        ClRand Random;
        IProgressiveVoronoi Voronoi;

        Dictionary<IProgressivePoint, GameObject> InstantiatedMeshes = new Dictionary<IProgressivePoint, GameObject>();

        float NextUpdate = 0;

        void Start()
        {
            Random = new ClRand(RSeed);
            Voronoi = VoronoiUtil.CreateProgressiveVoronoi(10, 1e-3f, Random.NewClRand());
        }

        // Use this for initialization
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                running = !running;
            }

            if (running && Time.realtimeSinceStartup > NextUpdate)
            {
                if (!Voronoi.AllPoints.Any())
                {
                    Voronoi.AddPoint(new Vec3Int(5, 5, 6));
                }
                else
                {
                    var done = false;

                    do
                    {
                        var pnt = InstantiatedMeshes.Keys.ToList()[Random.IntRange(0, InstantiatedMeshes.Keys.Count)];

                        var expansion_points = pnt.Cell.OrthoNeighbours.ToList()
                            .Where(c => Voronoi.InRange(c, IProgressiveVoronoi.Solidity.Solid))
                            .Select(c => Voronoi.Point(c))
                            .Where(pnt => pnt.Solidity == IProgressiveVoronoi.Solidity.Vacuum)
                            .ToList();

                        if (expansion_points.Count > 0)
                        {
                            int idx = Random.IntRange(0, expansion_points.Count);
                            var exp_pnt = expansion_points[idx];
                            expansion_points.RemoveAt(idx);

                            Voronoi.AddPoint(exp_pnt.Cell);

                            done = true;
                        }
                    }
                    while (!done);
                }

                ProgressiveInstantiateMeshes(Voronoi);

                NextUpdate = Time.realtimeSinceStartup + 1;
            }
        }

        private void ProgressiveInstantiateMeshes(IProgressiveVoronoi ps)
        {
            foreach (var pnt in ps.AllPoints
                .Where(p => p.Mesh != null)
                .Where(p => !InstantiatedMeshes.ContainsKey(p)))
            {
                var mat = Materials[MaterialIdx];
                MaterialIdx = (MaterialIdx + 1) % Materials.Count();

                InstantiatedMeshes[pnt] = InstantiateMesh(pnt.Mesh, mat);
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