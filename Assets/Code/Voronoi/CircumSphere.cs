using System.Collections.Generic;
using System.Diagnostics;

namespace Growth.Voronoi
{
    [DebuggerDisplay("Centre: ({Centre.X}, {Centre.Y}, {Centre.Z}) Radius: {Radius}")]
    public class CircumSphere
    {
        public CircumSphere(IReadOnlyList<Vec3> Verts)
        {
            var css = new CircumcentreSolver(Verts[0], Verts[1], Verts[2], Verts[3]);

            Valid = css.Valid;
            Centre = css.Centre;
            Radius = (float)css.Radius;
        }

        public bool Valid { get; }
        public Vec3 Centre { get; }
        public float Radius { get; }

        // we must overlap by more than t to be considered containing,
        // set t -ve if you want to make the test looser
        public bool Contains(Vec3 p, float t)
        {
            return (Centre - p).Size2() < (Radius - t) * (Radius - t);
        }
    }
}
