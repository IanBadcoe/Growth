// define this in the files which are to be profiled...
//#define PROFILE_ON

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Growth.Util
{
    static class PoorMansProfiler
    {
        static Dictionary<String, float> CumulativeTimings = new Dictionary<string, float>();

        static List<Tuple<String, float>> Stack = new List<Tuple<string, float>>();

        [System.Diagnostics.Conditional("PROFILE_ON")]
        public static void Reset()
        {
            CumulativeTimings = new Dictionary<string, float>();
            Stack = new List<Tuple<string, float>>();
        }

        [System.Diagnostics.Conditional("PROFILE_ON")]
        public static void Start(String name)
        {
            var now = Time.realtimeSinceStartup;

            Stack.Add(new Tuple<String, float>(name, now));
        }

        [System.Diagnostics.Conditional("PROFILE_ON")]
        public static void End(String name)
        {
            var now = Time.realtimeSinceStartup;

            MyAssert.IsTrue(name == Stack.Last().Item1, "Trying to pop item which is not the top of the stack");

            var entry = Stack.Last();
            Stack.RemoveAt(Stack.Count - 1);

            var time = now - entry.Item2;

            Accumulate(name, time);
        }

        private static void Accumulate(string name, float time)
        {
            if (!CumulativeTimings.ContainsKey(name))
            {
                CumulativeTimings[name] = 0;
            }

            CumulativeTimings[name] += time;
        }

        [System.Diagnostics.Conditional("PROFILE_ON")]
        public static void Dump()
        {
            foreach(var pair in CumulativeTimings)
            {
                System.Diagnostics.Debug.Write($"{pair.Key}\t->\t{pair.Value}");
            }
        }
    }

    // neater for code to use the raw Start/End calls and not force extra nesting on things with using(){}...
    //public class ProfileSection : IDisposable
    //{
    //    String Name { get; }

    //    public ProfileSection(String name)
    //    {
    //        Name = name;
    //        PoorMansProfiler.Start(name);
    //    }

    //    // fairly sure I do not need the "Dispose Pattern" if I am just using this for RAII in "using" statements
    //    public void Dispose()
    //    {
    //        PoorMansProfiler.End(Name);
    //    }
    //}
}
