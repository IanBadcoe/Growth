using System.Collections;
using UnityEngine;

namespace Assets.Code
{
    public class VoronoiTestBhv : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {
            Voronoi.VoronoiUtil vu = new Voronoi.VoronoiUtil();

            vu.CreateDelauney(new Vector3[]
            {
                new Vector3 (0, 0, 0),
                new Vector3 (1, 0, 0),
                new Vector3 (0, 1, 0),
                new Vector3 (0, 0, 1),
                new Vector3 (1, 1, 0),
                new Vector3 (1, 0, 1),
                new Vector3 (0, 1, 1),
                new Vector3 (1, 1, 1),
            });
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}