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

        public static void Reset()
        {
            CumulativeTimings = new Dictionary<string, float>();
            Stack = new List<Tuple<string, float>>();
        }

        public static void Start(String name)
        {
            var now = Time.realtimeSinceStartup;

            Stack.Add(new Tuple<String, float>(name, now));
        }

        public static void End(String name)
        {
            var now = Time.realtimeSinceStartup;

            Debug.Assert(name == Stack.Last().Item1);

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
    }

    public class ProfileSection : IDisposable
    {
        String Name { get; }

        public ProfileSection(String name)
        {
            Name = name;
            PoorMansProfiler.Start(name);
        }

        // fairly sure I do not need the "Dispose Pattern" if I am just using this for RAII in "using" statements
        public void Dispose()
        {
            PoorMansProfiler.End(Name);
        }
    }
}
