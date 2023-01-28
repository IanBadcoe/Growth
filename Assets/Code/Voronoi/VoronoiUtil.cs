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

        // makes a voronoi for the given verts, clipping all the edge polyhedra
        // against an axis-aligned bounding box "bound_extension" larger than the minimum
        // box that would contain the verts
        //
        // "probe_value" is the granularity for defining the bounds around the points:
        // - we extend the bounding box by this in each direction
        // - we generate additional points on that box in a square grid at this interval
        public static IVoronoi CreateBoundedVoronoi(IEnumerable<Vec3> verts, float probe_value)
        {
            VBounds b = new VBounds();

            foreach (var p in verts)
            {
                b.Encapsulate(p);
            }

            // build an encapsulating cuboid bound_extension bigger than the minimum
            b.Expand(probe_value * 2);

            var bound_verts = new List<Vec3>();

            // to get the grid size, we take each bound size (X, Y, Z), divide by the probe value to find how many we need to divide it
            // into (e.g. we're going to do an integer number of steps of _approx_ probe_value size).  We always want 1 point at the top
            // and 1 at the bottom but us having expanded the bounds by 2 * probe_value above should guarrantee that...
            //
            // BUT we still need to add 1 to the steps, to allow for one at the top and one at the bottom
            // 
            // Thus:
            // - if the size is < probe_value, we will have two points
            // - if it is between 1 and 2 probe sizes, we will have three points
            // - etc
            int x_steps = (int)(b.Size.X / probe_value);
            int y_steps = (int)(b.Size.Y / probe_value);
            int z_steps = (int)(b.Size.Z / probe_value);

            float x_step = b.Size.X / x_steps;
            float y_step = b.Size.Y / y_steps;
            float z_step = b.Size.Z / z_steps;

            for (int i = 0; i <= x_steps; i++)
            {
                for (int j = 0; j <= y_steps; j++)
                {
                    bound_verts.Add(new Vec3(b.Min.X + i * x_step, b.Min.X + j * y_step, b.Min.Z));
                    bound_verts.Add(new Vec3(b.Min.X + i * x_step, b.Min.X + j * y_step, b.Max.Z));
                }
            }

            // the edges of each plane's range (e.g. on the xy planes, the x = 0, y - 0, x = max, and y = max column/rows)
            // over lap with other planes, so adopt a strategy of:
            // xy -> supplies x and y
            // xz -> supplies only x
            // yz -> supplies neither

            for (int i = 0; i <= x_steps; i++)
            {
                for (int j = 1; j < z_steps; j++)
                {
                    bound_verts.Add(new Vec3(b.Min.X + i * x_step, b.Min.Y, b.Min.Z + j * z_step));
                    bound_verts.Add(new Vec3(b.Min.X + i * x_step, b.Max.Y, b.Min.Z + j * z_step));
                }
            }

            for (int i = 1; i < y_steps; i++)
            {
                for (int j = 1; j < z_steps; j++)
                {
                    bound_verts.Add(new Vec3(b.Min.X, b.Min.Y + i * y_step, b.Min.Z + j * z_step));
                    bound_verts.Add(new Vec3(b.Max.X, b.Min.Y + i * y_step, b.Min.Z + j * z_step));
                }
            }

            Delaunay d = CreateDelaunayInternal(verts.Concat(bound_verts).ToArray());

            foreach (var v in bound_verts)
            {
                d.TagVert(v, "bound");
            }

            return CreateVoronoiInternal(d);
        }

        private static IVoronoi CreateVoronoiInternal(IDelaunay d)
        {
            var v = new Voronoi();

            v.InitialiseFromBoundedDelaunay(d);

            return v;
        }

        public static IProgressiveVoronoi CreateProgressiveVoronoi(int size, float tolerance, float perturbation, ClRand random)
        {
            return new ProgressiveVoronoi(size, tolerance, perturbation, random);
        }
    }
}
