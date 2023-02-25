using UnityEngine;

namespace Growth.Util
{
    public class MyAssert
    {
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void IsTrue(bool comparison, string message)
        {
            if (!comparison)
            {
                Debug.LogWarning(message);
                Debug.Break();
            }
        }

        //[System.Diagnostics.Conditional("UNITY_EDITOR")]
        //public static void Throw(string message)
        //{
        //    Debug.LogWarning(message);
        //}
    }
}
