using Growth.Util;
using Growth.Voronoi;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Growth.Assets.Code
{
    public class ProgVoronoiScaleTest : MonoBehaviour
    {
        public int RSeed = 123;
        public Material[] Materials;
        public VoronoiTrackerBhv VTB;

        ProgressiveVoronoi Voronoi;
        ClRand Random;
        bool Running = false;

        int Number = 0;
        float DecayTime = 0;

        // Use this for initialization
        void Start()
        {
            Random = new ClRand(RSeed);
            Voronoi = new ProgressiveVoronoi(100, 1e-3f, 0.1f, Random.NewClRand());

            for (int i = 0; i < 10; i++)
            {
                Voronoi.AddPoint((Random.Vec3() * 98 + new Vec3(1, 1, 1)).ToVec3Int(), IVPolyhedron.MeshType.Faces, Materials[i % Materials.Length]);
                Number++;
            }

            VTB.Init(Voronoi);
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                Running = !Running;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }

            if (!Running)
                return;

            var solid_point_list = Voronoi.AllPoints.Where(p => p.Solidity == IProgressiveVoronoi.Solidity.Solid).ToList();

            var point = Random.RandomFromList(solid_point_list);

            var neighbs = Voronoi.OrthoGridNeighbours(point.Cell, IProgressiveVoronoi.Solidity.Solid).ToList();

            var neighb = Voronoi.Point(Random.RandomFromList(neighbs));

            if (neighb.Solidity == IProgressiveVoronoi.Solidity.Solid)
                return;

            var mat = point.Material;

            Voronoi.AddPoint(neighb.Cell, IVPolyhedron.MeshType.Faces, mat);

            Number++;
        }

        void OnGUI()
        {
            DecayTime = DecayTime * 0.9f + Time.deltaTime;
            GUI.Label(new Rect(10, 10, 100, 300), $"{DecayTime / 10 * 60} <-- {Number}");
        }
    }
}