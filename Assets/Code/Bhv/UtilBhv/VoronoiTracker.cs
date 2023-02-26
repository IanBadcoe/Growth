using Growth.Voronoi;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Growth.Behaviour
{
    public class VoronoiTracker : MonoBehaviour
    {
        public GameObject MeshTemplate;
        public GameObject MeshContainer;
        public IProgressiveVoronoi Voronoi;

        Dictionary<IProgressivePoint, GameObject> GameObjects = new Dictionary<IProgressivePoint, GameObject>();

        public void Init(IProgressiveVoronoi ipv)
        {
            Voronoi = ipv;
        }

        // Use this for initialization
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            if (Voronoi != null)
            {
                ProgressiveInstantiateGameObjects(Voronoi);
            }
        }

        private void ProgressiveInstantiateGameObjects(IProgressiveVoronoi ipv)
        {
            foreach (var pnt in ipv.AllPoints
                .Where(p => p.Solidity == IProgressiveVoronoi.Solidity.Solid)
                .Where(p => !GameObjects.ContainsKey(p)))
            {
                var mat = pnt.Material;

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