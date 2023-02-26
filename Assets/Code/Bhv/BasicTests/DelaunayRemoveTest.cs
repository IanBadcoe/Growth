using Growth.Util;
using Growth.Voronoi;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Growth.Assets.Code.Bhv.BasicTests
{
    public class DelaunayRemoveTest : MonoBehaviour
    {
        Delaunay Delaunay;
        List<DTetrahedron> CheckList;
        Dictionary<DTetrahedron, GameObject> GameObjects = new Dictionary<DTetrahedron, GameObject>();

        public GameObject MeshTemplate;
        public GameObject MeshContainer;
        public Material[] Materials;

        int MatIdx = 0;

        int Step = 0;

        Vec3 AddedVert = new Vec3(5, 5, 5);

        ClRand Random;
        public int RandomSeed;

        int NumPoints = 5;

        // Use this for initialization
        void Start()
        {
            Random = new ClRand(RandomSeed);

            var camera = Transform.FindFirstObjectByType<Camera>();

            camera.transform.LookAt(new Vector3(5, 5, 5));
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                NumPoints++;
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                NumPoints--;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                switch(Step)
                {
                    case 0:
                        Delaunay = new Delaunay(1e-3f);

                        List<Vec3> init = new List<Vec3>();

                        for (int i = 0; i < NumPoints; i++)
                        {
                            init.Add(Random.Vec3() * 10.0f);
                        }

                        Delaunay.InitialiseWithVerts(init);

                        break;

                    case 1:
                        CheckList = Delaunay.Tets.ToList();

                        Delaunay.AddVert(AddedVert);

                        break;

                    case 2:
                        Delaunay.RemoveVert(AddedVert);

                        MyAssert.IsTrue(Delaunay.Validate(), "Invalid!");

                        var now_extras = new List<DTetrahedron>();
                        var now_missing = new List<DTetrahedron>();

                        foreach(var tet in Delaunay.Tets)
                        {
                            if (CheckList.FirstOrDefault(t => t.Equals(tet)) == null)
                            {
                                now_extras.Add(tet);
                            }
                        }

                        foreach(var tet in CheckList)
                        {
                            if (Delaunay.Tets.FirstOrDefault(t => t.Equals(tet)) == null)
                            {
                                now_missing.Add(tet);
                            }
                        }

                        MyAssert.IsTrue(now_missing.Count == 0 && now_extras.Count == 0, "something changed!");

                        break;
                }

                Step = (Step + 1) % 3;
            }

            if (Delaunay == null)
                return;

            foreach(var tet in Delaunay.Tets)
            {
                if (!GameObjects.ContainsKey(tet))
                {
                    var mat = Materials[MatIdx];
                    MatIdx = (MatIdx + 1) % Materials.Length;

                    var go = Instantiate(MeshTemplate, tet.Centre.ToVector3(), Quaternion.identity, MeshContainer.transform);

                    go.GetComponent<MeshFilter>().mesh = MeshUtil.Polyhedron2Mesh(tet.ToPolyhedron(), tet.Centre);
                    go.GetComponent<MeshRenderer>().material = mat;

                    GameObjects[tet] = go;
                }
            }

            var key_list = GameObjects.Keys.ToList();

            foreach (var tet in key_list)
            {
                if (!Delaunay.Tets.Contains(tet))
                {
                    Destroy(GameObjects[tet]);

                    GameObjects.Remove(tet);
                }
            }
        }
    }
}