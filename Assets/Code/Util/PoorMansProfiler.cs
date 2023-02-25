// define this in the files which are to be profiled...
//#define PROFILE_ON

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Growth.Util
{
    static class PoorMansProfiler
    {
        static Dictionary<String, float> SliceTimings = new Dictionary<string, float>();
        static Dictionary<String, float> StackTimings = new Dictionary<string, float>();

        static List<Tuple<String, float>> Stack = new List<Tuple<string, float>>();


        [System.Diagnostics.Conditional("PROFILE_ON")]
        public static void Reset()
        {
            SliceTimings = new Dictionary<string, float>();
            StackTimings = new Dictionary<string, float>();
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
            var time = now - entry.Item2;

            Accumulate(name, time);

            Stack.RemoveAt(Stack.Count - 1);
        }

        private static void Accumulate(string name, float time)
        {
            if (!SliceTimings.ContainsKey(name))
            {
                SliceTimings[name] = 0;
            }

            SliceTimings[name] += time;

            string stack_name = BuildStackName();

            if (!StackTimings.ContainsKey(stack_name))
            {
                StackTimings[stack_name] = 0;
            }

            StackTimings[stack_name] += time;
        }

        [System.Diagnostics.Conditional("PROFILE_ON")]
        public static void Dump()
        {
            foreach (var s in DumpToEnum())
            {
                System.Diagnostics.Debug.Write(s);
            }
        }

        [System.Diagnostics.Conditional("PROFILE_ON")]
        public static void Dump(string filename)
        {
            File.WriteAllLinesAsync(filename, DumpToEnum());
        }

        public static IEnumerable<string> DumpToEnum()
        {
            yield return "By section name\n";

            foreach (var pair in SliceTimings.OrderBy(p => p.Key))
            {
                yield return $"{pair.Key}\t->\t{pair.Value}";
            }

            yield return "\n\nBy stack path\n";

            foreach (var pair in StackTimings.OrderBy(p => p.Key))
            {
                yield return $"{pair.Key}\t->\t{pair.Value}";
            }
        }

        static string BuildStackName()
        {
            return Stack.Aggregate("", (str, s2) => str + "  |  " + s2.Item1);
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
