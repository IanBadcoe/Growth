using UnityEngine;

namespace Growth.Behaviour
{
    public class OrbitCamera : MonoBehaviour
    {
        public float Dist;

        public float Heading;
        public float Elevation;

        public float AutoAngularSpeed;
        public float ManualSpeed;
        public float ElevationSpeed;

        bool Auto = true;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Auto = !Auto;
            }

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                Heading -= ManualSpeed;
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                Heading += ManualSpeed;
            }

            if (Input.GetKey(KeyCode.UpArrow))
            {
                Elevation += ElevationSpeed;
            }

            if (Input.GetKey(KeyCode.DownArrow))
            {
                Elevation -= ElevationSpeed;
            }

            if (Auto)
            {
                Heading += AutoAngularSpeed;
            }

            var sh = Mathf.Sin(Heading);
            var ch = Mathf.Cos(Heading);

            var se = Mathf.Sin(Elevation);
            var ce = Mathf.Cos(Elevation);

            var vect = new Vector3(sh * ce, se, ch * ce) * Dist;

            transform.position = vect;
            transform.LookAt(new Vector3(0, 0, 0));
        }
    }
}