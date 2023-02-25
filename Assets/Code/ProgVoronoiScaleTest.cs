using Growth.Util;
using Growth.Voronoi;
using System.Collections;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace Growth.Assets.Code
{
    public class ProgVoronoiScaleTest : MonoBehaviour
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
            Mapper = new Growth.Voronoi.Mappers.CylinderMapper(
                                50, 100, 100,
                                new Vec3Int(60, 20, 10),
                                new Vec3(0.15f, 0.15f, 0.15f),
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
                        (int)(i / (float)num_seeds * 60), 1, 1),
                        IVPolyhedron.MeshType.Faces,
                        Materials[i % Materials.Length]);

                OpenCells[new_point] =
                    new List<IPVMapper.CellDir> {
                        IPVMapper.CellDir.PlusX,
                        IPVMapper.CellDir.MinusX,
                        IPVMapper.CellDir.PlusY,
                        IPVMapper.CellDir.PlusZ
                    };

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

            IProgressivePoint found = null;
            IProgressivePoint point = null;

            while(found == null)
            {
                var solid_point_list = OpenCells.Keys.ToList();

                point = Random.RandomFromList(solid_point_list);

                Vec3Int try_found = null;

                if (OpenCells[point].Contains(IPVMapper.CellDir.PlusX))
                {
                    try_found = Mapper.StepCell(point.Cell, IPVMapper.CellDir.PlusX);

                    TidyOpenList(point, IPVMapper.CellDir.PlusX);
                }
                
                if (try_found == null && OpenCells[point].Contains(IPVMapper.CellDir.MinusX))
                {
                    try_found = Mapper.StepCell(point.Cell, IPVMapper.CellDir.MinusX);

                    TidyOpenList(point, IPVMapper.CellDir.MinusX);
                }
                
                if(try_found == null && OpenCells[point].Contains(IPVMapper.CellDir.PlusY))
                {
                    try_found = Mapper.StepCell(point.Cell, IPVMapper.CellDir.PlusY);

                    TidyOpenList(point, IPVMapper.CellDir.PlusY);
                }

                if (try_found == null && OpenCells[point].Contains(IPVMapper.CellDir.PlusZ)) 
                {
                    try_found = Mapper.StepCell(point.Cell, IPVMapper.CellDir.PlusZ);

                    TidyOpenList(point, IPVMapper.CellDir.PlusZ);
                }

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
            OpenCells[new_point] =
                new List<IPVMapper.CellDir> {
                                    IPVMapper.CellDir.PlusX,
                                    IPVMapper.CellDir.MinusX,
                                    IPVMapper.CellDir.PlusY,
                                    IPVMapper.CellDir.PlusZ
                };

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