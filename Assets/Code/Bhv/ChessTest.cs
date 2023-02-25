#define PROFILE_ON

using Growth.Util;
using Growth.Voronoi;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Growth.Behaviour
{

    public class ChessTest : MonoBehaviour
    {
        public GameObject MeshContainer;
        public GameObject MeshTemplate;

        public List<Material> Materials;
        int MaterialIdx = 0;

        public int RSeed;

        bool running = false;

        ClRand Random;
        IProgressiveVoronoi Voronoi;

        Dictionary<IProgressivePoint, GameObject> GameObjects = new Dictionary<IProgressivePoint, GameObject>();

        void Start()
        {
            Random = new ClRand(RSeed);
            Voronoi = VoronoiUtil.CreateProgressiveVoronoi(100, 1e-3f, 0.15f, Random.NewClRand());
        }

        // Use this for initialization
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                running = !running;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }

            if (running)
            {
                PoorMansProfiler.Reset();

                PoorMansProfiler.Start("Outer");

                foreach (Transform go in MeshContainer.transform)
                {
                    Destroy(go.gameObject);
                }

                Material material = Materials[MaterialIdx];
                IVPolyhedron.MeshType mesh_type = MaterialIdx > 1 ? IVPolyhedron.MeshType.Faces : IVPolyhedron.MeshType.Smooth;
                MaterialIdx = (MaterialIdx + 1) % Materials.Count;

                float start_time = Time.realtimeSinceStartup;
                float row_time = start_time;

                for (int i = 10; i < 26; i++)
                {
                    for (int j = 10; j < 26; j++)
                    {
                        Voronoi.AddPoint(new Vec3Int(i, 10, j),
                            IVPolyhedron.MeshType.Faces, Materials[(i + j + 10) % 2]);
                    }

                    float now = Time.realtimeSinceStartup;
                    System.Diagnostics.Debug.Write($"Row {i - 10} time {now - row_time}");
                    row_time = now;
                }

                float new_now = Time.realtimeSinceStartup;
                System.Diagnostics.Debug.Write($"\nVoronoi time {new_now - start_time}\n");

                row_time = new_now;

                ProgressiveInstantiateGameObjects(Voronoi);

                new_now = Time.realtimeSinceStartup;
                System.Diagnostics.Debug.WriteLine($"Mesh time {new_now - row_time}\n");
                System.Diagnostics.Debug.WriteLine($"Total time {new_now - start_time}");

                PoorMansProfiler.End("Outer");

                PoorMansProfiler.Dump();

                PoorMansProfiler.Dump(Application.persistentDataPath + "\\profile.txt");

                running = false;
            }
        }

        private void ProgressiveInstantiateGameObjects(IProgressiveVoronoi ps)
        {
            foreach (var pnt in ps.AllPoints
                .Where(p => p.Solidity == IProgressiveVoronoi.Solidity.Solid)
                .Where(p => !GameObjects.ContainsKey(p)))
            {
                var mat = pnt.Material;
                MaterialIdx = (MaterialIdx + 1) % Materials.Count();

                GameObjects[pnt] = InstantiateGameObjects(pnt, mat);
            }
        }

        private GameObject InstantiateGameObjects(IProgressivePoint pnt, Material mat)
        {
            var go = Instantiate(MeshTemplate, Voronoi.Cell2Vert(pnt.Cell, IProgressiveVoronoi.CellPosition.Origin).ToVector3(), Quaternion.identity, MeshContainer.transform);

            go.GetComponent<MeshFilter>().mesh = pnt.Mesh;
            go.GetComponent<MeshRenderer>().material = mat;

            return go;
        }
    }

}