using UnityEngine;

namespace Growth.Behaviour
{
    public class OrbitCamera : MonoBehaviour
    {
        public float Height;
        public float Dist;
        public float Angle;
        public float AngularSpeed;
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            Angle += AngularSpeed;

            var x = Mathf.Sin(Angle) * Dist;
            var z = Mathf.Cos(Angle) * Dist;

            transform.position = new Vector3(x, Height, z);
            transform.LookAt(new Vector3(0, 0, 0));
        }
    }
}