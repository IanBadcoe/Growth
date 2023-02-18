using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Growth.Util;

namespace Growth.Voronoi
{

    public static class VoronoiUtil
    {
        public static IDelaunay CreateDelaunay(Vec3[] verts)
        {
            return CreateDelaunayInternal(verts);
        }

        static Delaunay CreateDelaunayInternal(Vec3[] verts)
        {
            var d = new Delaunay(1e-3f);

            d.InitialiseWithVerts(verts);

            return d;
        }

        public static IProgressiveVoronoi CreateProgressiveVoronoi(int size, float tolerance, float perturbation, ClRand random)
        {
            return new ProgressiveVoronoi(size, tolerance, perturbation, random);
        }
    }
}
