using Growth.Util;
using Growth.Voronoi;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Growth.Behaviour
{
    public class ScaleTest_Torus : MonoBehaviour
    {
        public int RSeed = 123;
        public Material[] Materials;
        public VoronoiTrackerBhv VTB;

        ProgressiveVoronoi Voronoi;
        IPVMapper Mapper;

        ClRand Random;
        bool Running = false;

        int Number = 0;
        float DecayTime = 0;

        Dictionary<IProgressivePoint, List<IPVMapper.CellDir>> OpenCells = new Dictionary<IProgressivePoint, List<IPVMapper.CellDir>>();

        // Use this for initialization
        void Start()
        {
            Random = new ClRand(RSeed);
            //Voronoi = new ProgressiveVoronoi(100, 1e-3f, 0.1f, Random.NewClRand());
            Mapper = new Growth.Voronoi.Mappers.TorusMapper(
                                150, 50,
                                new Vec3Int(60, 10, 28),
                                new Vec3(0.01f, 0.01f, 0.01f),
                                Random.NewClRand()
                            );
            Voronoi = new ProgressiveVoronoi(
                Mapper,
                1e-3f
            );

            int num_seeds = 10;

            for (int i = 0; i < num_seeds; i++)
            {
                var new_point = Voronoi.AddPoint(
                    new Vec3Int(
                        (int)(i / (float)num_seeds * 60), 1, 0),
                        IVPolyhedron.MeshType.Faces,
                        Materials[i % Materials.Length]);

                AddOpenCell(new_point);

                Number++;
            }

            VTB.Init(Voronoi);
        }

        private void AddOpenCell(IProgressivePoint new_point)
        {
            var list = new List<IPVMapper.CellDir>();

            if (new_point.Cell.X % 6 == 0 && new_point.Cell.Y % 3 == 1)
            {
                list.Add(IPVMapper.CellDir.PlusZ);
                list.Add(IPVMapper.CellDir.MinusZ);
            }

            if (new_point.Cell.Z % 6 == 0 && new_point.Cell.Y % 3 == 1)
            {
                list.Add(IPVMapper.CellDir.PlusX);
                list.Add(IPVMapper.CellDir.MinusX);
            }

            if (new_point.Cell.X % 6 == 0 && new_point.Cell.Z % 6 == 0)
            {
                list.Add(IPVMapper.CellDir.PlusY);
                list.Add(IPVMapper.CellDir.MinusY);
            }

            if (list.Count > 0)
            {
                OpenCells[new_point] = list;
            }
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

            IProgressivePoint found = null;
            IProgressivePoint point = null;

            while (found == null && OpenCells.Count > 0)
            {
                var solid_point_list = OpenCells.Keys.ToList();

                point = solid_point_list.First();

                var dir = OpenCells[point].First();

                Vec3Int try_found = Mapper.StepCell(point.Cell, dir, IProgressiveVoronoi.Solidity.Solid);

                TidyOpenList(point, dir);

                if (try_found != null)
                {
                    var neighb = Voronoi.Point(try_found);

                    if (neighb.Solidity == IProgressiveVoronoi.Solidity.Vacuum)
                    {
                        found = neighb;
                    }
                }
            }

            var mat = point.Material;

            var new_point = Voronoi.AddPoint(found.Cell, IVPolyhedron.MeshType.Faces, mat);
            AddOpenCell(new_point);

            Number++;
        }

        private void TidyOpenList(IProgressivePoint point, IPVMapper.CellDir dir)
        {
            OpenCells[point].Remove(dir);

            if (OpenCells[point].Count == 0)
            {
                OpenCells.Remove(point);
            }
        }

        void OnGUI()
        {
            DecayTime = DecayTime * 0.9f + Time.deltaTime;
            GUI.Label(new Rect(10, 500, 100, 300), $"{DecayTime / 10 * 60} <-- {Number}");
        }
    }
}